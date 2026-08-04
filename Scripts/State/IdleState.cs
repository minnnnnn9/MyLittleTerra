using MLT.Core;
using MLT.Data.ItemSO;
using MLT.World;
using UnityEngine;

namespace MLT.Player
{    /// <summary>
     /// InteractPressed 처리 흐름 (일관성 정리):
     ///
     ///   InteractPressed
     ///     ├─ BOW 장비 중          → TryHandleBowInput()  → AimState
     ///     ├─ Placeable 아이템     → TryPlaceSelectedItem()
     ///     ├─ 농사 도구 (ItemToolData) → CreateToolActionContext() → BeginToolAction() → ToolUseState
     ///     ├─ 씨앗/비료            → GetFarmingTargetCells() → context.Execute()
     ///     └─ 나머지               → TryInteractByMouse()
     /// </summary>
    public class IdleState : IPlayerState
    {
        private bool _isSowingDrag = false;

        public void Enter(PlayerController player)
        {
            player.Animator.SetIdle();
            _isSowingDrag = false;
        }

        public void Tick(PlayerController player)
        {
            if (player.Input.SubInteractPressed)
            {
                player.UseEdibleItem();
            }

            if (player.Input.InventoryPressed)
            {
                player.ChangeState(new InventoryState());
                return;
            }

            if (player.Input.MouseScrolled != 0)
            {
                player.ScrollHotBarSelection(player.Input.MouseScrolled);
            }

            if (player.Input.SlotPressed)
            {
                EventBus.RaiseOnHotBarPressed(player.Input.SlotPressedIndex);
            }

            // 마우스 클릭 시 무조건 NPC/기계가 눈앞에 있는지부터 확인!
            if (player.Input.InteractPressed)
            {
                var selectedItem = player.Data.Inventory.GetSelectedItem();
                bool isTool = selectedItem is ItemToolData;

                // 도구가 아닐 때(씨앗, 비료, 설치물, 빈손)만 즉시 상호작용
                if (!isTool)
                {
                    var targetObj = player.Interactor.FindMouseTargetInPlayerRange();
                    if (targetObj != null)
                    {
                        targetObj.Interact(player);
                        return; // NPC 대화했으면 여기서 틱(Tick)을 끊어버림! (씨앗 파종으로 안 넘어감)
                    }
                }
            }

            ToolType currentTool = ToolManager.Instance?.CurrentTool ?? ToolType.NONE;

            // 1. NPC가 없을 때 비로소 씨앗 파종 로직으로 진입
            if (currentTool == ToolType.SEED)
            {
                HandleSeedDrag(player);
                return;
            }

            // 2. 나머지 일반 행동들 
            if (player.Input.InteractPressed)
            {
                HandleInteract(player);
                return;
            }

            if (player.Input.Move.sqrMagnitude > 0.01f)
            {
                player.ChangeState(new MoveState());
                return;
            }

            player.Animator.SetIdle();
        }

        private void HandleInteract(PlayerController player)
        {
            ToolType currentTool = ToolManager.Instance?.CurrentTool ?? ToolType.NONE;
            var selectedItem = player.Data.Inventory.GetSelectedItem();
            bool isTool = selectedItem is ItemToolData;

            // 1. 활 처리
            if (currentTool == ToolType.BOW)
            {
                TryHandleBowInput(player);
                return;
            }

            // 2. 기계 설치
            if (selectedItem is ItemPlaceableData)
            {
                player.TryPlaceSelectedItem();
                player.ChangeState(new IdleState());
                return;
            }

            // 3. 도구 사용 
            if (isTool)
            {
                var tool = selectedItem as ItemToolData;
                var context = player.Interactor.CreateToolActionContext(tool);
                Vector2 dir = player.Interactor.GetMouseCellDirectionFromPlayer();
                player.BeginToolAction(context, tool, dir);
                return;
            }

            // 4. 비료 
            if (currentTool == ToolType.FERTILIZER)
            {
                var cells = player.Interactor.GetFarmingTargetCells(currentTool, ToolGrade.NONE);
                var context = new ToolActionContext
                {
                    ToolType = currentTool,
                    Grade = ToolGrade.NONE,
                    ToolData = null,
                    TargetCells = cells
                };
                Vector2 dir = player.Interactor.GetMouseCellDirectionFromPlayer();
                if (dir.sqrMagnitude > 0.001f)
                    player.Animator.FaceDirection(dir);
                context.Execute(player);
                return;
            }

            // 5. 나머지 
            player.Interactor.TryInteractByMouse();
        }

        public void Exit(PlayerController player) 
        {
            _isSowingDrag = false;
        }

        private void HandleSeedDrag(PlayerController player)
        {
            // 클릭 시작
            if (player.Input.InteractPressed)
            {
                _isSowingDrag = true;
                TrySowAtMouse(player);
            }

            // 홀드 중 - 매 프레임 현재 마우스 위치 타일에 파종 시도
            if (_isSowingDrag && player.Input.InteractHeld)
            {
                TrySowAtMouse(player);
            }

            // 뗄 때 종료
            if (player.Input.InteractReleased)
            {
                _isSowingDrag = false;
            }

            // 이동 허용 (드래그 중에도 이동 가능)
            if (player.Input.Move.sqrMagnitude > 0.01f)
            {
                player.ChangeState(new MoveState());
                return;
            }

            player.Animator.SetIdle();
        }

        private void TrySowAtMouse(PlayerController player)
        {
            var cells = player.Interactor.GetFarmingTargetCells(ToolType.SEED, ToolGrade.NONE);
            if (cells == null || cells.Count == 0) return;

            var context = new ToolActionContext
            {
                ToolType = ToolType.SEED,
                Grade = ToolGrade.NONE,
                ToolData = null,
                TargetCells = cells
            };

            Vector2 dir = player.Interactor.GetMouseCellDirectionFromPlayer();
            if (dir.sqrMagnitude > 0.001f)
                player.Animator.FaceDirection(dir);

            context.Execute(player);
        }


        // ── BOW 처리 ─────────────────────────────────────────────

        /// <summary>
        /// 활 장비 중 마우스 좌클릭 → 까마귀 탐지 → AimState 전환
        /// </summary>
        /// <returns>BOW 입력을 소비했으면 true (이후 처리 스킵)</returns>
        private void TryHandleBowInput(PlayerController player)
        {
            if (AimSystem.Instance == null) return;

            Crow crow = AimSystem.Instance.FindCrowAtMouse(Camera.main);
            if (crow == null)
            {
                Debug.Log("[활] 클릭 위치에 까마귀 없음");
                return;
            }

            // 까마귀 방향으로 플레이어 회전만
            Vector2 dirToCrow = (crow.transform.position - player.transform.position).normalized;
            player.Animator.FaceDirection(dirToCrow);

            // 바로 AimState로 (애니메이션 없이)
            player.ChangeState(new AimState(crow));
        }
    }
}


