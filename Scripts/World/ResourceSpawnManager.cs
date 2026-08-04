using MLT.Core;
using MLT.Machine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MLT.World
{
    public class ResourceSpawnManager : MonoBehaviour
    {
        public static ResourceSpawnManager Instance { get; private set; }

        [Header("스폰 영역 설정 (왼쪽 구역 제한)")]
        [SerializeField] private Vector3Int _areaMin = new Vector3Int(-20, -15, 0);
        [SerializeField] private Vector3Int _areaMax = new Vector3Int(-1, 10, 0);

        [Header("참조 타일맵")]
        [SerializeField] private Tilemap _baseTilemap;

        [Header("프리팹 및 스폰 설정")]
        [SerializeField] private GameObject _treePrefab;
        [SerializeField] private GameObject _rockPrefab;

        [SerializeField] private int _maxTrees = 15;
        [SerializeField] private int _maxRocks = 15;
        [SerializeField] private int _spawnPerDayTree = 5; // 테스트를 위해 스폰 개수를 늘렸습니다
        [SerializeField] private int _spawnPerDayRock = 5;

        [Header("충돌 체크 (테스트 시 잠시 None이나 아무것도 체크 안 하셔도 됩니다)")]
        [SerializeField] private LayerMask _blockingLayer;

        private List<GameObject> _spawnedTrees = new List<GameObject>();
        private List<GameObject> _spawnedRocks = new List<GameObject>();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Debug.Log("[ResourceSpawnManager] Start() 진입 - 테스트 스폰을 시작합니다.");

            // ── 범인 예상 1번 해결 ──
            // 기존 코드는 '다음 날 아침' 이벤트가 터져야만 스폰이 돌았습니다.
            // 테스트를 위해 시작하자마자 강제로 스폰 로직을 한 번 실행합니다!
            HandleDayStarted();

            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnDayStarted += HandleDayStarted;
            }
        }

        private void OnDestroy()
        {
            if (TimeManager.Instance != null)
                TimeManager.Instance.OnDayStarted -= HandleDayStarted;
        }

        private void HandleDayStarted()
        {
            _spawnedTrees.RemoveAll(t => t == null);
            _spawnedRocks.RemoveAll(r => r == null);

            Debug.Log($"[ResourceSpawnManager] 현재 살아있는 나무: {_spawnedTrees.Count}, 돌: {_spawnedRocks.Count}");

            SpawnResources(_treePrefab, _spawnedTrees, _maxTrees, _spawnPerDayTree, "나무");
            SpawnResources(_rockPrefab, _spawnedRocks, _maxRocks, _spawnPerDayRock, "돌");
        }

        private void SpawnResources(GameObject prefab, List<GameObject> trackedList, int maxAmount, int spawnPerDay, string debugName)
        {
            if (prefab == null) { Debug.LogError($"[ResourceSpawnManager] {debugName} 프리팹이 비어있습니다(Null)!"); return; }
            if (_baseTilemap == null) { Debug.LogError("[ResourceSpawnManager] 바닥 타일맵이 연결되지 않았습니다(Null)!"); return; }

            int spawnCount = Mathf.Min(spawnPerDay, maxAmount - trackedList.Count);
            Debug.Log($"[ResourceSpawnManager] 이번에 스폰할 {debugName} 목표 개수: {spawnCount}개");

            if (spawnCount <= 0) return;

            var candidates = GetSpawnableCells();
            Debug.Log($"[ResourceSpawnManager] 지정된 영역 내에서 찾아낸 빈 타일 후보지: {candidates.Count}칸");

            if (candidates.Count == 0)
            {
                Debug.LogWarning("[ResourceSpawnManager] 후보지 타일이 0칸입니다! AreaMin/Max 좌표 범위가 타일맵과 맞는지 확인하세요.");
                return;
            }

            Shuffle(candidates);

            int spawned = 0;
            foreach (var pos in candidates)
            {
                if (spawned >= spawnCount) break;

                Vector3 worldPos = _baseTilemap.GetCellCenterWorld(pos);
                worldPos.z = 0f;

                // ── 범인 예상 2번 해결 및 로그 확인 ──
                // OverlapCircle이 바닥 타일맵의 콜라이더나 엉뚱한 레이어를 때려서 스폰이 씹히는지 확인합니다.
                Collider2D hit = Physics2D.OverlapCircle(worldPos, 0.4f, _blockingLayer);
                if (hit != null)
                {
                    Debug.Log($"[ResourceSpawnManager] {pos} 좌표는 [{hit.name}] 오브젝트에 막혀 스폰을 건너뜁니다.");
                    continue;
                }

                GameObject obj = Instantiate(prefab, worldPos, Quaternion.identity, transform);
                trackedList.Add(obj);
                spawned++;
            }

            Debug.Log($"[ResourceSpawnManager] 최종 스폰 완료된 {debugName}: {spawned}개");
        }

        private List<Vector3Int> GetSpawnableCells()
        {
            var result = new List<Vector3Int>();

            for (int x = _areaMin.x; x <= _areaMax.x; x++)
            {
                for (int y = _areaMin.y; y <= _areaMax.y; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);

                    // 해당 좌표에 진짜 타일이 존재하긴 하는지 체크
                    if (!_baseTilemap.HasTile(pos)) continue;

                    if (WorldObjectManager.Instance != null &&
                        WorldObjectManager.Instance.GetObject(pos) != null) continue;

                    result.Add(pos);
                }
            }
            return result;
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
        // ResourceSpawnManager 클래스 내부 맨 아래에 추가해 주세요!
        private void OnDrawGizmosSelected()
        {
            if (_baseTilemap == null) return;

            // 최소/최대 셀의 월드 중심 좌표 구하기
            Vector3 minWorld = _baseTilemap.GetCellCenterWorld(_areaMin);
            Vector3 maxWorld = _baseTilemap.GetCellCenterWorld(_areaMax);

            // 사각형의 중심점과 크기 계산
            Vector3 center = (minWorld + maxWorld) / 2f;
            Vector3 size = new Vector3(
                Mathf.Abs(maxWorld.x - minWorld.x) + _baseTilemap.cellSize.x,
                Mathf.Abs(maxWorld.y - minWorld.y) + _baseTilemap.cellSize.y,
                0.1f
            );

            // 씬 뷰에 반투명한 하늘색 박스와 선 그려주기
            Gizmos.color = new Color(0f, 0.8f, 1f, 0.2f);
            Gizmos.DrawCube(center, size);
            Gizmos.color = new Color(0f, 0.8f, 1f, 0.9f);
            Gizmos.DrawWireCube(center, size);
        }

    }
}