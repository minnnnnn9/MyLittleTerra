using MLT.Core;
using MLT.Data.ItemSO;
using MLT.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace MLT.Player
{
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private float _cellCheckRadius = 0.45f;
        [SerializeField] private LayerMask _interactableLayer;
        [SerializeField] private Tilemap _referenceTilemap;
        [SerializeField] private Camera _mainCamera;

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

        // OnSceneLoaded 전부 제거

        private void BindTilemaps()
        {
            _referenceTilemap = SceneTilemapProvider.Instance.ObjectTilemap;
            if (_mainCamera == null) _mainCamera = Camera.main;
        }

        public ToolActionContext CreateToolActionContext(ItemToolData toolData)
        {
            if (toolData == null) return null;

            if (toolData._toolType == ToolType.BOW) return null;

            var context = new ToolActionContext
            {
                ToolType = toolData._toolType,
                Grade = toolData._grade,
                ToolData = toolData
            };

            // 도구 종류와 상관없이 무조건 NPC, 가축, 기계를 먼저 찾기
            IInteractable targetObj = FindMouseTargetInPlayerRange();
            if (targetObj != null)
            {
                context.TargetObject = targetObj;
                context.TargetCells = new List<Vector3Int>(); // 오브젝트와 상호작용하므로 바닥 타일은 무시
                Debug.Log($"[명령서 발행] 상호작용 대상 우선 발견: {targetObj.GetType().Name}");
                return context;
            }

            // 상호작용 대상이 허공일 때만 기존의 농사 타일 로직을 실행
            if (IsFarmingTool(toolData._toolType))
            {
                context.TargetCells = GetFarmingTargetCells(toolData._toolType, toolData._grade);
                Debug.Log($"[명령서 발행] 농사 타일 {context.TargetCells.Count}칸");
                return context;
            }

            // 비농사 도구(도끼, 곡괭이)인데 허공을 클릭한 경우
            context.TargetCells = new List<Vector3Int>();
            return context;
        }

        private bool IsFarmingTool(ToolType type) =>
            type == ToolType.HOE ||
            type == ToolType.WATERING_CAN ||
            type == ToolType.SCYTHE ||
            type == ToolType.SEED ||
            type == ToolType.FERTILIZER;

        /// <summary>
        /// 버그7: 농사 도구는 플레이어 기준 동서남북 인접 1칸만 허용.
        /// 등급에 따라 추가 범위 확장은 그 1칸을 기준으로 계산.
        /// </summary>
        public List<Vector3Int> GetFarmingTargetCells(ToolType toolType, ToolGrade grade)
        {
            var cells = new List<Vector3Int>();

            if (_referenceTilemap == null || _mainCamera == null || Mouse.current == null)
                return cells;

            Vector2 mouseScreen = Mouse.current.position.ReadValue();
            Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(mouseScreen);
            mouseWorld.z = 0f;

            Vector3Int targetCell = _referenceTilemap.WorldToCell(mouseWorld);
            Vector3Int playerCell = _referenceTilemap.WorldToCell(transform.position);
            //Debug.Log($"플레이어 월드: {transform.position}, 타일셀: {playerCell}");

            // ── 버그7 핵심: 플레이어 기준 동서남북 1칸(+ 자기 발 밑 0,0)만 허용 ──
            if (!IsAdjacentOrSelf(playerCell, targetCell))
            {
                Debug.Log("[농사] 플레이어 인접 1칸 범위 밖");
                return cells;
            }

            // diff로 dirInt 직접 계산 (대각선 대응)
            Vector3Int diff = targetCell - playerCell;
            Vector3Int dirInt = new Vector3Int(
                Mathf.Clamp(diff.x, -1, 1),
                Mathf.Clamp(diff.y, -1, 1),
                0
            );

            switch (grade)
            {
                case ToolGrade.NONE:
                    cells.Add(targetCell);
                    break;
                case ToolGrade.COPPER:
                    for (int i = 0; i < 3; i++) cells.Add(targetCell + dirInt * i);
                    break;
                case ToolGrade.IRON:
                    for (int i = 0; i < 5; i++) cells.Add(targetCell + dirInt * i);
                    break;
                case ToolGrade.GOLD:
                    for (int x = -1; x <= 1; x++)
                        for (int y = -1; y <= 1; y++)
                            cells.Add(targetCell + new Vector3Int(x, y, 0));
                    break;
                case ToolGrade.IRIDIUM: // 추가
                    for (int x = -2; x <= 2; x++)
                        for (int y = -2; y <= 2; y++)
                            cells.Add(targetCell + new Vector3Int(x, y, 0));
                    break;
            }

            return cells;
        }

        private bool IsAdjacentOrSelf(Vector3Int playerCell, Vector3Int targetCell)
        {
            // 타일 좌표 대신 타일 중심 월드 좌표끼리 비교
            Vector3 playerWorld = _referenceTilemap.GetCellCenterWorld(playerCell);
            Vector3 targetWorld = _referenceTilemap.GetCellCenterWorld(targetCell);

            float dx = Mathf.Abs(targetWorld.x - playerWorld.x);
            float dy = Mathf.Abs(targetWorld.y - playerWorld.y);

            // 타일 1칸 크기 + 약간의 여유(0.1f)
            float tileSize = _referenceTilemap.cellSize.x;
            return dx <= tileSize * 1.1f && dy <= tileSize * 1.1f;
        }

        public IInteractable FindMouseTargetInPlayerRange()
        {
            if (_referenceTilemap == null || _mainCamera == null || Mouse.current == null)
                return null;

            Vector2 mouseScreen = Mouse.current.position.ReadValue();
            Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(mouseScreen);
            mouseWorld.z = 0f;

            Vector3Int targetCell = _referenceTilemap.WorldToCell(mouseWorld);
            Vector3Int playerCell = _referenceTilemap.WorldToCell(transform.position);

            if (!GridUtil.IsWithin3x3(playerCell, targetCell))
            {
                Debug.Log("[Interact] 마우스 위치가 플레이어 상호작용 범위 밖");
                return null;
            }

            Vector3 targetWorldCenter = _referenceTilemap.GetCellCenterWorld(targetCell);
            Collider2D hit = Physics2D.OverlapCircle(
                targetWorldCenter, _cellCheckRadius, _interactableLayer);

            if (hit == null) return null;

            IInteractable interactable = hit.GetComponent<IInteractable>()
                                      ?? hit.GetComponentInParent<IInteractable>();

            if (interactable == null || !interactable.CanInteract(_player)) return null;

            return interactable;
        }

        public void TryInteractByMouse()
        {
            FindMouseTargetInPlayerRange()?.Interact(_player);
        }

        public Vector2 GetMouseCellDirectionFromPlayer()
        {
            if (_referenceTilemap == null || _mainCamera == null || Mouse.current == null)
                return Vector2.zero;

            Vector2 mouseScreen = Mouse.current.position.ReadValue();
            Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(mouseScreen);
            mouseWorld.z = 0f;

            Vector3Int targetCell = _referenceTilemap.WorldToCell(mouseWorld);
            Vector3Int playerCell = _referenceTilemap.WorldToCell(transform.position);
            Vector3Int diff = targetCell - playerCell;

            // 대각선 포함 정규화
            float x = Mathf.Clamp(diff.x, -1, 1);
            float y = Mathf.Clamp(diff.y, -1, 1);

            if (x == 0 && y == 0) return Vector2.zero;
            return new Vector2(x, y).normalized; // (0.707, 0.707) 형태
        }
    }
}