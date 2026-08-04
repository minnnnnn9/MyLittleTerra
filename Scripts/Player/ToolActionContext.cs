using MLT.Core;
using MLT.Data.ItemSO;
using MLT.Machine;
using MLT.World;
using System.Collections.Generic;
using UnityEngine;

namespace MLT.Player
{
    public class ToolActionContext
    {
        public ToolType ToolType;
        public ToolGrade Grade;
        public ItemToolData ToolData;
        public IInteractable TargetObject;
        public List<Vector3Int> TargetCells;

        /// <summary>
        /// 수정 핵심:
        /// 모든 도구가 ToolManager.ExecuteToolAction(cell, ToolType)을 거침.
        /// 낫만 FarmingManager 직접 호출하던 불일관성 제거.
        /// ToolManager가 유일한 실행 진입점.
        /// </summary>
        public void Execute(PlayerController player)
        {
            // 오브젝트 상호작용 (채광, 벌목, 기계 등)
            if (TargetObject != null)
            {
                Debug.Log($"[ToolActionContext] TargetObject={TargetObject.GetType().Name}, ToolType={ToolType}");

                if (ToolType == ToolType.PICKAXE && TargetObject is InstallableObject installable)
                {
                    Debug.Log($"[ToolActionContext] 기계 설치 해제 {installable.CellPos}");
                    EventBus.RaiseSFX(SFXType.BREAK_PLACE);
                    WorldObjectManager.Instance?.RemoveObject(installable.CellPos);
                    return;
                }
                TargetObject.Interact(player);
                return;
            }

            // 타일 농사 액션 - 모두 ToolManager 경유
            if (TargetCells == null || TargetCells.Count == 0) return;
            if (ToolManager.Instance == null) return;

            foreach (var cell in TargetCells)
            {
                // ArtifactSpot이 있으면 우선 처리하고 땅갈기 스킵
                var spot = ArtifactSpotManager.Instance?.GetSpotAt(cell);
                Debug.Log($"[ArtifactCheck] cell={cell} spot={spot} manager={ArtifactSpotManager.Instance}");

                if (spot != null)
                {
                    spot.Interact(player);
                    return;
                }
                ToolManager.Instance.ExecuteToolAction(cell, ToolType);
            }
        }
    }
}