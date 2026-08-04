using MLT.Core;
using MLT.Data.ItemSO;
using MLT.Farm;
using MLT.Machine;
using MLT.Player;
using MLT.UI;
using MLT.World;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

/// <summary>
/// 도구 범위 하이라이트 UI (v2)
///
/// 노란색 = 실제 상호작용이 일어날 수 있는 타일
/// 빨간색 = 범위 안이지만 지금 상태로 상호작용 불가 / 범위 밖
///
/// 도구별 노란색 조건:
///   HOE          → IsAbleToFarm && State == EMPTY (기계 없음)
///   WATERING_CAN → State != EMPTY (경작된 타일)
///   SCYTHE       → State == HARVESTABLE
///   씨앗          → State == TILLED
///   비료          → State == TILLED || SEEDED
///   도끼/곡괭이   → 범위 안에 IInteractable 또는 기계(InstallableObject) 존재
///
/// 곡괭이는 기계(InstallableObject)도 해제 가능 → WorldObjectManager로 확인
/// </summary>
public class TileHighlightUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController _player;
    [SerializeField] private HotBarUI _hotBarUI;
    [SerializeField] private TileHighlighter _tileHighlighter;
    [SerializeField] private PlayerInteractor _playerInteractor;
    [SerializeField] private Tilemap _referenceTilemap;
    [SerializeField] private PlayerInventory _playerInventory;

    [Header("Highlight Prefabs")]
    [Tooltip("노란색 타일 하이라이트 프리팹")]
    [SerializeField] private GameObject _highlightPrefab;

    [Tooltip("빨간색 타일 하이라이트 프리팹")]
    [SerializeField] private GameObject _redHighlightPrefab;

    [Header("Pool Settings")]
    [SerializeField] private int _poolSize = 30;

    // ── 내부 상태 ────────────────────────────────────────────────
    private ItemBaseData _currentItem;
    private Camera _mainCamera;

    private readonly List<GameObject> _yellowPool = new();
    private readonly List<GameObject> _redPool = new();

    // ═══════════════════════════════════════════════════════════════

    private void Awake()
    {
        _mainCamera = Camera.main;
        BuildPool(_yellowPool, _highlightPrefab, _poolSize);
        BuildPool(_redPool, _redHighlightPrefab, _poolSize); // 빨간색도 범위만큼 필요
    }

    private void OnEnable()
    {
        if (_hotBarUI != null)
            _hotBarUI.OnChangeSelection += UpdateSelectionInfo;
        if (SceneTilemapProvider.Instance != null) BindTilemap();
        SceneTilemapProvider.OnProviderReady += BindTilemap;
    }

    private void OnDisable()
    {
        if (_hotBarUI != null)
            _hotBarUI.OnChangeSelection -= UpdateSelectionInfo;
        SceneTilemapProvider.OnProviderReady -= BindTilemap;
    }

    private void BindTilemap()
    {
        _referenceTilemap = SceneTilemapProvider.Instance.ObjectTilemap;
        if (_mainCamera == null) _mainCamera = Camera.main;
    }

    // ── Update ───────────────────────────────────────────────────

    private void Update()
    {
        if (_referenceTilemap == null) return;

        if (!IsTargetableTool(_currentItem))
        {
            HideAll();
            return;
        }

        Vector3Int mouseCellPos = GetMouseCellPos();
        Vector3Int playerCellPos = GetPlayerCellPos();

        bool isInRange = IsAdjacentOrSelf(playerCellPos, mouseCellPos);

        if (!isInRange)
        {
            HidePool(_yellowPool);
            ShowSingleRed(mouseCellPos);
            return;
        }

        List<Vector3Int> cells = GetTargetCells(mouseCellPos, playerCellPos);

        // ── 수정: 범위 내 셀 중 하나라도 노란색이 없으면 1칸 빨간색만 ──
        bool anyCanInteract = false;
        foreach (var cell in cells)
        {
            if (CanInteractAt(cell)) { anyCanInteract = true; break; }
        }

        if (!anyCanInteract)
        {
            HidePool(_yellowPool);
            ShowSingleRed(mouseCellPos); // 커서 위치 1칸만
            return;
        }

        // 노란색 있으면 기존대로 셀별로 색 나눠서 표시
        int yellowIdx = 0;
        int redIdx = 0;
        foreach (var cell in cells)
        {
            if (CanInteractAt(cell)) PlaceHighlight(_yellowPool, ref yellowIdx, cell);
            else PlaceHighlight(_redPool, ref redIdx, cell);
        }
        for (; yellowIdx < _yellowPool.Count; yellowIdx++) _yellowPool[yellowIdx].SetActive(false);
        for (; redIdx < _redPool.Count; redIdx++) _redPool[redIdx].SetActive(false);
    }

    // ── 선택 아이템 갱신 ─────────────────────────────────────────

    private void UpdateSelectionInfo(int index)
    {
        if (_playerInventory == null || _playerInventory.GetSelectedSlot() == null)
        {
            _currentItem = null;
            return;
        }
        _currentItem = _playerInventory.GetSelectedSlot().Item;
    }

    // ── 핵심: 셀별 상호작용 가능 여부 ────────────────────────────

    private bool CanInteractAt(Vector3Int cell)
    {
        if (_currentItem == null) return false;

        Vector3 worldCenter = _referenceTilemap.GetCellCenterWorld(cell);
        Collider2D hit = Physics2D.OverlapCircle(worldCenter, 0.45f, LayerMask.GetMask("InteractableLayer"));

        if (hit != null)
        {
            var interactable = hit.GetComponent<IInteractable>() ?? hit.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                if (_player != null && interactable.CanInteract(_player))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        if (_currentItem is ItemToolData tool)
        {
            // 곡괭이로 기계 부수는 로직 
            if (tool._toolType == ToolType.PICKAXE)
            {
                if (WorldObjectManager.Instance != null && WorldObjectManager.Instance.GetObject(cell) != null)
                    return true;
            }

            // 도끼 곡괭이는 바닥을 클릭하면 무조건 빨간색
            if (tool._toolType == ToolType.AXE || tool._toolType == ToolType.PICKAXE)
                return false;

            // 농사 도구 로직
            if (FarmingManager.Instance == null) return false;
            FarmState state = FarmingManager.Instance.GetTileState(cell);

            switch (tool._toolType)
            {
                case ToolType.HOE:
                    return state == FarmState.EMPTY
                        && FarmingManager.Instance.IsAbleToFarmPublic(cell)
                        && (WorldObjectManager.Instance == null || WorldObjectManager.Instance.GetObject(cell) == null);
                case ToolType.WATERING_CAN:
                    return state != FarmState.EMPTY;
                case ToolType.SCYTHE:
                    return state == FarmState.HARVESTABLE;
                case ToolType.FERTILIZER:
                case ToolType.SEED:
                    return state == FarmState.TILLED || state == FarmState.SEEDED;
            }
        }

        // 씨앗 비료 데이터 자체일 경우
        if (_currentItem is ItemSeedData)
        {
            if (FarmingManager.Instance == null) return false;
            return FarmingManager.Instance.GetTileState(cell) == FarmState.TILLED;
        }

        // 기계 설치물일 경우
        if (_currentItem is ItemPlaceableData)
        {
            return WorldObjectManager.Instance == null || WorldObjectManager.Instance.GetObject(cell) == null;
        }

        return false;
    }

    // ── 범위 계산 ─────────────────────────────────────────────────

    private List<Vector3Int> GetTargetCells(Vector3Int targetCell, Vector3Int playerCell)
    {
        // 마우스 커서 위치에 NPC나 가축 등 상호작용 대상이 있다면
        //  무조건 1칸만 반환
        if (_referenceTilemap != null)
        {
            Vector3 worldCenter = _referenceTilemap.GetCellCenterWorld(targetCell);
            Collider2D hit = Physics2D.OverlapCircle(worldCenter, 0.45f, LayerMask.GetMask("InteractableLayer"));
            if (hit != null)
            {
                // 대상을 찾았으므로 더 이상 도구 범위를 계산하지 않고 1칸만 줍니다.
                return new List<Vector3Int> { targetCell };
            }
        }

        // 도끼/곡괭이: 마우스 위치 1칸만 
        if (_currentItem is ItemToolData td &&
            (td._toolType == ToolType.AXE || td._toolType == ToolType.PICKAXE))
        {
            return new List<Vector3Int> { targetCell };
        }

        //  농사 상호작용 대상이 허공일 때만 PlayerInteractor의 넓은 범위를 가져온다.
        if (_playerInteractor != null && _currentItem is ItemToolData farmTool)
            return _playerInteractor.GetFarmingTargetCells(farmTool._toolType, farmTool._grade);

        // 씨앗/비료 1칸
        return new List<Vector3Int> { targetCell };
    }

    // ── 하이라이트 배치 헬퍼 ─────────────────────────────────────

    private void PlaceHighlight(List<GameObject> pool, ref int idx, Vector3Int cell)
    {
        if (idx >= pool.Count) return;
        pool[idx].transform.position = _referenceTilemap.GetCellCenterWorld(cell);
        pool[idx].SetActive(true);
        idx++;
    }

    private void ShowSingleRed(Vector3Int cell)
    {
        if (_redPool.Count == 0) return;
        _redPool[0].transform.position = _referenceTilemap.GetCellCenterWorld(cell);
        _redPool[0].SetActive(true);
        for (int i = 1; i < _redPool.Count; i++) _redPool[i].SetActive(false);
        HidePool(_yellowPool);
    }

    // ── 도구 판별 ─────────────────────────────────────────────────

    /// <summary>하이라이터를 표시할 도구인지 (BOW, NONE 제외 전부)</summary>
    private bool IsTargetableTool(ItemBaseData item)
    {
        if (item == null) return false;
        if (item is ItemSeedData || item is ItemPlaceableData) return true;
        if (item is ItemToolData td)
            return td._toolType != ToolType.NONE && td._toolType != ToolType.BOW;
        return false;
    }

    // ── 위치 헬퍼 ─────────────────────────────────────────────────

    private Vector3Int GetMouseCellPos()
    {
        if (_referenceTilemap == null || _mainCamera == null || Mouse.current == null)
            return Vector3Int.zero;
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0f;
        return _referenceTilemap.WorldToCell(mouseWorld);
    }

    private Vector3Int GetPlayerCellPos()
    {
        if (_referenceTilemap == null) return Vector3Int.zero;
        Vector3 playerWorld = _playerInteractor != null
            ? _playerInteractor.transform.position
            : transform.position;
        return _referenceTilemap.WorldToCell(playerWorld);
    }

    private bool IsAdjacentOrSelf(Vector3Int playerCell, Vector3Int targetCell)
    {
        if (_referenceTilemap == null) return false;
        Vector3 playerWorld = _referenceTilemap.GetCellCenterWorld(playerCell);
        Vector3 targetWorld = _referenceTilemap.GetCellCenterWorld(targetCell);
        float dx = Mathf.Abs(targetWorld.x - playerWorld.x);
        float dy = Mathf.Abs(targetWorld.y - playerWorld.y);
        float tileSize = _referenceTilemap.cellSize.x;
        return dx <= tileSize * 1.1f && dy <= tileSize * 1.1f;
    }

    // ── 풀 관리 ───────────────────────────────────────────────────

    private void HideAll()
    {
        HidePool(_yellowPool);
        HidePool(_redPool);
    }

    private static void HidePool(List<GameObject> pool)
    {
        foreach (var obj in pool) obj.SetActive(false);
    }

    private void BuildPool(List<GameObject> pool, GameObject prefab, int count)
    {
        if (prefab == null) return;
        for (int i = 0; i < count; i++)
        {
            var obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }
}