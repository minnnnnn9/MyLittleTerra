using MLT.Core;
using MLT.Data.ItemSO;
using MLT.Farm;
using MLT.World;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace MLT.Machine
{
    public class WorldObjectManager : MonoBehaviour
    {
        public static WorldObjectManager Instance { get; private set; }

        [SerializeField] private Grid _grid;
        private Dictionary<Vector3Int, InstallableObject> _objects = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (SceneTilemapProvider.Instance != null) BindTilemaps();
        }

        private void OnEnable()
        {
            SceneTilemapProvider.OnProviderReady += BindTilemaps;
        }

        private void OnDisable()
        {
            SceneTilemapProvider.OnProviderReady -= BindTilemaps;
        }

        private void BindTilemaps()
        {
            _grid = SceneTilemapProvider.Instance.Grid;

            // ── 핵심: 농장 씬에 들어와서 타일맵 세팅이 끝났을 때 금고를 엽니다 ──
            Debug.Log("🔄 [WorldObjectManager] 타일맵 바인딩 완료! 금고 복구 작업을 시작합니다.");
            RestoreMachinesFromData();
        }

        private void Start()
        {
            if (MLT.TimeManager.Instance != null)
                MLT.TimeManager.Instance.OnDayStarted += UpdateAllObjects;
        }

        private void OnDestroy()
        {
            if (MLT.TimeManager.Instance != null)
                MLT.TimeManager.Instance.OnDayStarted -= UpdateAllObjects;
        }

        // ── 금고 복구 로직 ──
        private void RestoreMachinesFromData()
        {
            if (SceneDataManager.Instance == null || _grid == null) return;

            // 이전 씬에서 파괴된 기계들의 껍데기(null) 제거
            var keys = new List<Vector3Int>(_objects.Keys);
            foreach (var key in keys)
            {
                if (_objects[key] == null) _objects.Remove(key);
            }

            int savedCount = SceneDataManager.Instance.PlacedMachines.Count;
            Debug.Log($"📦 [복구 확인] 금고에 저장된 기계 개수: {savedCount}개");

            if (savedCount == 0) return;

            foreach (var kvp in SceneDataManager.Instance.PlacedMachines)
            {
                Vector3Int cellPos = kvp.Key;
                ItemPlaceableData data = kvp.Value;

                if (_objects.ContainsKey(cellPos)) continue;

                if (data != null && data.Prefab != null)
                {
                    Vector3 worldPos = _grid.GetCellCenterWorld(cellPos);
                    Vector3 spawnPos = worldPos;

                    PlacementAnchor anchor = data.Prefab.GetComponentInChildren<PlacementAnchor>(true);
                    if (anchor != null) spawnPos -= anchor.transform.localPosition;

                    GameObject go = Instantiate(data.Prefab, spawnPos, Quaternion.identity);
                    InstallableObject newObj = go.GetComponent<InstallableObject>();

                    if (newObj != null)
                    {
                        newObj.Setup(cellPos);
                        _objects[cellPos] = newObj;
                        Debug.Log($"✅ [복구 완료] {data._name}이(가) 맵에 다시 생성되었습니다!");
                    }
                }
            }
        }

        // ── 설치 / 철거 (금고 연동) ──
        public bool PlaceObject(InstallableObject prefab, Vector3Int cellPos, ItemPlaceableData itemData = null)
        {
            if (_objects.ContainsKey(cellPos)) return false;

            Vector3 worldPos = _grid.GetCellCenterWorld(cellPos);
            InstallableObject newObj = Instantiate(prefab, worldPos, Quaternion.identity);
            newObj.Setup(cellPos);
            _objects.Add(cellPos, newObj);

            // 금고 저장
            if (SceneDataManager.Instance != null && itemData != null)
            {
                SceneDataManager.Instance.PlacedMachines[cellPos] = itemData;
            }

            return true;
        }

        public void RegisterObject(InstallableObject obj, Vector3Int cellPos, ItemPlaceableData itemData = null)
        {
            if (_objects.ContainsKey(cellPos))
            {
                Debug.LogWarning($"[WorldObjectManager] {cellPos}에 이미 등록된 오브젝트가 있습니다.");
                return;
            }

            obj.Setup(cellPos);
            _objects.Add(cellPos, obj);

            // 금고 저장
            if (SceneDataManager.Instance != null && itemData != null)
            {
                SceneDataManager.Instance.PlacedMachines[cellPos] = itemData;
                Debug.Log($"✅ [금고 저장 완료] {itemData._name}이(가) {cellPos}에 완벽하게 저장되었습니다!");
            }
        }

        public void UnregisterObject(Vector3Int cellPos)
        {
            _objects.Remove(cellPos);
        }

        public void RemoveObject(Vector3Int cellPos)
        {
            if (!_objects.TryGetValue(cellPos, out var obj)) return;

            obj.OnUninstall();
            _objects.Remove(cellPos);
            Destroy(obj.gameObject);

            // 철거 시 금고에서도 기록 삭제
            if (SceneDataManager.Instance != null)
            {
                SceneDataManager.Instance.PlacedMachines.Remove(cellPos);
                SceneDataManager.Instance.ExtractorStates.Remove(cellPos); // 데이터도 같이 날림
            }
        }

        public InstallableObject GetObject(Vector3Int cellPos)
        {
            _objects.TryGetValue(cellPos, out var obj);
            return obj;
        }

        public IEnumerable<InstallableObject> GetAllObjects() => _objects.Values;

        public void UpdateAllObjects()
        {
            foreach (var obj in _objects.Values)
                if (obj != null) obj.OnDayPassed();
        }
    }
}