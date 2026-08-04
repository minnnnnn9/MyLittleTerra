using MLT.Core;
using MLT.Data.ItemSO;
using MLT.Farm;
using MLT.Machine;
using MLT.Utils;
using MLT.World;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace MLT.Player
{
    public class PlayerPlacement : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private Tilemap _referenceTilemap;

        [Header("설치 차단 레이어")]
        [SerializeField] private LayerMask _blockingLayerMask;

        [SerializeField] private Transform _parent;

        private PlayerController _player;

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
            if (_mainCamera == null) _mainCamera = Camera.main;
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

        // OnSceneLoaded, HandleSceneLoaded, RefreshSceneReferences 전부 제거

        private void BindTilemaps()
        {
            _referenceTilemap = SceneTilemapProvider.Instance.ObjectTilemap;
            if (_mainCamera == null) _mainCamera = Camera.main;
        }

        public bool TryPlace(ItemPlaceableData itemData)
        {
            if (itemData == null || itemData.Prefab == null) return false;

            GameObject prefab = itemData.Prefab;
            Vector2Int size = itemData.Size;

            if (_mainCamera == null || _referenceTilemap == null) return false;
            if (Mouse.current == null) return false;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return false;
            if (size.x <= 0 || size.y <= 0) size = Vector2Int.one;

            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(mouseScreenPos);
            mouseWorld.z = 0f;

            Vector3Int targetCell = _referenceTilemap.WorldToCell(mouseWorld);
            Vector3Int playerCell = _referenceTilemap.WorldToCell(transform.position);

            if (!GridUtil.IsWithin3x3(playerCell, targetCell)) return false;

            if (WorldObjectManager.Instance != null && WorldObjectManager.Instance.GetObject(targetCell) != null) return false;

            if (FarmingManager.Instance != null)
            {
                for (int x = 0; x < size.x; x++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        Vector3Int checkCell = new(targetCell.x + x, targetCell.y + y, targetCell.z);
                        if (FarmingManager.Instance.HasActiveFarmTileAt(checkCell)) return false;
                    }
                }
            }

            if (!CanPlaceAt(targetCell, size)) return false;

            Vector3 cellCenterWorld = _referenceTilemap.GetCellCenterWorld(targetCell);
            cellCenterWorld.z = 0f;
            Vector3 spawnPos = CalculateSpawnPosition(prefab, cellCenterWorld);

            GameObject placed = Instantiate(prefab, spawnPos, Quaternion.identity, _parent);

            PlacedObject placedObject = placed.GetComponent<PlacedObject>();
            if (placedObject != null) placedObject.Initialize(targetCell, size);

            InstallableObject installable = placed.GetComponent<InstallableObject>();
            if (installable != null)
            {
                // [수정 핵심] RegisterObject에 itemData를 같이 넘겨서 금고에 저장하라고 시킵니다!
                WorldObjectManager.Instance.RegisterObject(installable, targetCell, itemData);
            }

            EventBus.RaiseSFX(SFXType.ITEM_PLACE);
            Debug.Log($"[Placement] 설치 성공 {prefab.name} at {targetCell}");
            return true;
        }

        private Vector3 CalculateSpawnPosition(GameObject prefab, Vector3 cellCenterWorld)
        {
            PlacementAnchor anchor = prefab.GetComponentInChildren<PlacementAnchor>(true);
            if (anchor == null)
            {
                Debug.LogWarning($"[{prefab.name}] PlacementAnchor 없음. 셀 중앙에 설치합니다.");
                return cellCenterWorld;
            }
            return cellCenterWorld - anchor.transform.localPosition;
        }

        private bool CanPlaceAt(Vector3Int originCell, Vector2Int size)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector3Int cell = new(originCell.x + x, originCell.y + y, originCell.z);
                    Vector3 worldCenter = _referenceTilemap.GetCellCenterWorld(cell);

                    Collider2D hit = Physics2D.OverlapBox(
                        worldCenter,
                        _referenceTilemap.cellSize * 0.9f,
                        0f,
                        _blockingLayerMask
                    );

                    if (hit != null)
                    {
                        Debug.Log($"[Placement] blocked by {hit.name}");
                        return false;
                    }
                }
            }
            return true;
        }
        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RefreshSceneReferences();
        }

        public void RefreshSceneReferences()
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;

            if (SceneTilemapProvider.Instance != null)
                _referenceTilemap = SceneTilemapProvider.Instance.ObjectTilemap;

            Debug.Log($"[SceneRef] Tilemap 갱신: {(_referenceTilemap != null ? _referenceTilemap.name : "null")}");
        }
    }
}