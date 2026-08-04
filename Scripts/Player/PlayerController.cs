using MLT.Core;
using MLT.Data;
using MLT.Data.ItemSO;
using MLT.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MLT.Player
{
    [RequireComponent(typeof(PlayerData))]
    [RequireComponent(typeof(PlayerMover))]
    [RequireComponent(typeof(PlayerAnimator))]
    [RequireComponent(typeof(PlayerInputSource))]
    [RequireComponent(typeof(PlayerInteractor))]
    [RequireComponent(typeof(InventoryController))]
    public class PlayerController : MonoBehaviour
    {
        private PlayerData _data;
        private PlayerMover _mover;
        private PlayerAnimator _animator;
        private IInputSource _input;
        private PlayerInteractor _interactor;
        private PlayerPlacement _placement;
        private PlayerStats _stats;
        private InventoryController _invenController;

        //private IInteractable _pendingInteractTarget;
        //private List<Vector3Int> _pendingFarmingCells;
        //private ToolType _pendingToolType = ToolType.NONE;

        private ToolActionContext _pendingContext; // (추가)

        private IPlayerState _currentState;

        public PlayerData Data => _data;
        public PlayerMover Mover => _mover;
        public PlayerAnimator Animator => _animator;
        public IInputSource Input => _input;
        public PlayerInteractor Interactor => _interactor;
        public PlayerPlacement Placement => _placement;
        public ItemBaseData GetEquippedItem() => Data.Inventory.GetSelectedItem();

        public InventoryController InvenController => _invenController;
        public PlayerStats Stats => _stats;
        private int slotIndex = 0;

        
        public bool IsDialogueEnd { get; private set; } = false;

        





        private void Awake()
        {
            _data = GetComponent<PlayerData>();
            _mover = GetComponent<PlayerMover>();
            _animator = GetComponent<PlayerAnimator>();
            _input = GetComponent<PlayerInputSource>();
            _interactor = GetComponent<PlayerInteractor>();
            _placement = GetComponent<PlayerPlacement>();
            _stats = GetComponent<PlayerStats>();
            _invenController = GetComponent<InventoryController>();
        }

        private void OnEnable()
        {
            EventBus.OnSpentAllEnergy += HandleSpentAllEnergy;
            EventBus.OnEnterCutscene += EnterCutscene;
            EventBus.OnExitCutscene += ExitCutscene;
            EventBus.OnDialogueEnd += DialogueEnd;


        }

        private void OnDisable()
        {
            EventBus.OnSpentAllEnergy -= HandleSpentAllEnergy;
            EventBus.OnEnterCutscene -= EnterCutscene;
            EventBus.OnExitCutscene -= ExitCutscene;
            EventBus.OnDialogueEnd -= DialogueEnd;


        }

        private void Start()
        {
            ChangeState(new IdleState());
        }

        private void Update()
        {
            HandleHotbarInput();
            HandleGlobalInput();
            _currentState?.Tick(this);
        }

        public void ChangeState(IPlayerState newState)
        {
            _currentState?.Exit(this);
            _currentState = newState;
            _currentState.Enter(this);
        }



        private void HandleGlobalInput()
        {
            // ESC(Cancel) 키가 눌렸을 때
            if (Input != null && Input.CancelPressed)
            {
                // 1. 모든 기계 UI를 닫으라고 방송
                EventBus.RaiseCloseAllMachineUI();

                // 2. 인벤토리 UI를 닫으라고 방송
                EventBus.RaiseCloseInventory();

                // 3. 만약 액션 맵이 UI였다면 다시 Player로 복구 (필요 시)
                // (PlayerInputSource에 구현된 SetUIInput 활용)
                if (Input is PlayerInputSource source)
                {
                    source.SetUIInput(false);
                }

                Debug.Log("ESC 입력: 모든 UI 닫기 시도");
            }
        }

        // ───────────────────────────────────────────
        // 핫바 입력 처리
        // ───────────────────────────────────────────


        private void HandleHotbarInput()
        {
            if (Data == null || Data.Inventory == null || Input == null) return;

            slotIndex = Data.Inventory.SelectedSlotIndex;



            if (slotIndex < 0) return;

            Data.Inventory.SelectSlot(slotIndex);

            // 선택된 아이템을 ToolManager에 전달 → ToolManager가 타입 분기
            var item = Data.Inventory.GetSelectedItem();
            ToolManager.Instance?.SelectItem(item);

            // 디버그 로그
            if (item == null)
                Debug.Log($"슬롯 선택: {slotIndex + 1} | 아이템 없음");
            
               
        }

        // 대상이 뭐든 이 함수 하나로 처리
        public void BeginToolAction(ToolActionContext context, ItemToolData toolData, Vector2 faceDir)
        {
            if (Data.Energy.CurrentEnergy <= 0) return;
            if (context == null) return;
            if (toolData == null) return;

            // BOW는 TargetCells 비어있어도 통과 (AimState에서 처리)
            if (toolData._toolType != ToolType.BOW)
            {
                // ── 수정 핵심 ──────────────────────────────────────────────
                // 기존: TargetObject도 없고 TargetCells도 없으면 무조건 return
                //        → 도끼/곡괭이가 허공 클릭 시 애니메이션 안 나옴
                //
                // 수정: 비농사 도구(도끼, 곡괭이)는 TargetObject 없어도 애니메이션 실행
                //        → 실제 Execute 시점에 TargetCells가 비어있으면 아무것도 안 일어남
                //        → UI에서 빨간색으로 이미 알려줬으니 무방함
                // ─────────────────────────────────────────────────────────
                bool isNonFarmingTool = toolData._toolType == ToolType.AXE ||
                                        toolData._toolType == ToolType.PICKAXE;

                if (!isNonFarmingTool)
                {
                    // 농사 도구는 기존과 동일 (TargetCells 없으면 실행 안 함)
                    if (context.TargetObject == null &&
                        (context.TargetCells == null || context.TargetCells.Count == 0)) return;
                }
                // 비농사 도구는 여기서 return 안 하고 그냥 통과 → 애니메이션 실행
            }

            _pendingContext = context;

            if (faceDir.sqrMagnitude > 0.001f)
                Animator.FaceDirection(faceDir);

            ChangeState(new ToolUseState(toolData));
        }

        // ───────────────────────────────────────────
        // 상호작용
        // ───────────────────────────────────────────
        public void TryPlaceSelectedItem()
        {
            if (Data == null || Data.Inventory == null || Placement == null) return;
            if (Interactor != null)
            {
                var targetObj = Interactor.FindMouseTargetInPlayerRange();
                if (targetObj != null)
                {
                    targetObj.Interact(this);
                    return; // NPC랑 대화했으면 설치 로직은 여기서 중단
                }
            }
            var selectedItem = Data.Inventory.GetSelectedItem();
            if (selectedItem is not ItemPlaceableData placeableItem) return;

            if (placeableItem.Prefab == null)
            {
                Debug.LogWarning($"[{placeableItem._name}] 설치 프리팹이 없습니다.");
                return;
            }

            // [수정 핵심] Prefab과 Size를 따로 넘기지 않고 placeableItem(데이터) 통째로 넘김!
            bool placed = Placement.TryPlace(placeableItem);

            if (!placed) return;

            Data.Inventory.TryRemoveItem(selectedItem, 1);
        }

        // ───────────────────────────────────────────
        // 기타
        // ───────────────────────────────────────────
        public void OpenInventoryUI()
        {
            EventBus.RaiseOpenInventory();
        }

        //public void ExecutePendingToolUse()
        //{
        //    if (_pendingInteractTarget != null)
        //    {
        //        _pendingInteractTarget.Interact(this);
        //    }
        //    else if (_pendingFarmingCells != null && _pendingFarmingCells.Count > 0)
        //    {
        //        if (ToolManager.Instance == null) return;

        //        foreach (var cell in _pendingFarmingCells)
        //            ToolManager.Instance.ExecuteToolAction(cell);
        //    }

        //    ClearPendingToolUse();
        //}

        public void ExecutePendingToolUse()
        {
            // 명령서야, 네가 알아서 실행해! (내부에서 나무인지 타일인지 알아서 갈라짐)
            _pendingContext?.Execute(this);

            ClearPendingToolUse();
        }

        public void ClearPendingToolUse()
        {
            //_pendingInteractTarget = null;
            //_pendingFarmingCells = null;
            //_pendingToolType = ToolType.NONE;
            _pendingContext = null;
        }

        public void CloseInventoryUI()
        {
            EventBus.RaiseCloseInventory();
        }

        public void BeginDialogue() => ChangeState(new DialogueState());

        public void EndDialogue()
        {
            TimeManager.Instance?.SetTimeSpeed(1f);

            EventBus.RaiseDialogueEnded();
            ChangeState(new IdleState());
        }

        public void BeginMinigame() => ChangeState(new MinigameState());
        public void EndMinigame() => ChangeState(new IdleState());

        private void HandleSpentAllEnergy() { }

        public Vector2 SnapToCardinal(Vector2 dir)
        {
            if (dir.sqrMagnitude <= 0.001f)
                return Vector2.zero;

            dir.Normalize();

            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                return new Vector2(Mathf.Sign(dir.x), 0f);

            return new Vector2(0f, Mathf.Sign(dir.y));
        }

        public void BeginShop(GameObject shopPanel)
        {
            ChangeState(new ShopState(shopPanel));
        }

        public void ScrollHotBarSelection(float value)
        {
            EventBus.RaiseScrollHotBar(value);

        }

        public void EnterCutscene()
        {
            ChangeState(new CutsceneState());

        }

        public void ExitCutscene()
        {
            ChangeState(new IdleState());

        }

        public void SellItem(int index)
        {
            if (_invenController == null) return;
            if (_invenController.InventoryUI == null) return;

           

            

           

            if (index < 0) return;

            EventBus.RaiseOnSellItem(index, Data, Data.Inventory);

        }

        public void BuyItem()
        {
            if (_invenController == null) return;
            if (_invenController.ShopUIS == null) return;

            int hoveredIndex = _invenController.ShopUIS[0].HoveredSlotIndex;

            if (hoveredIndex < 0) return;

            EventBus.RaiseOnBuyItem(hoveredIndex);

        }

        public void UseEdibleItem()
        {                  
            var item = Data.Inventory.GetSelectedItem();
            
            if (item == null) return;
            if (item._isAbleToEat)
            {
                Data.Inventory.TryRemoveItem(item, 1);

            }

            Data.Energy.RecoverEnergy(item.StaminaRecoveryAmount);

        }

        public void StartDialogue()
        {
            UIManager.Instance.OnDialogueUI();


        }

        public void DialogueEnd()
        {
            IsDialogueEnd = false;

        }

        public void SetDialogueEnd(bool value)
        {
            IsDialogueEnd = value;
        }

    }


}