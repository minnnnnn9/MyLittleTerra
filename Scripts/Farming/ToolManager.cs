using MLT.Core;
using MLT.Data;
using MLT.Data.ItemSO;
using MLT.Farm;
using UnityEngine;

namespace MLT.Core
{
    public class ToolManager : MonoBehaviour
    {
        public static ToolManager Instance { get; private set; }

        public ToolType CurrentTool { get; private set; }
        public ItemSeedData CurrentSeed { get; private set; }
        public FertilizerData CurrentFertilizer { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public ToolType SelectItem(ItemBaseData item)
        {
            if (item == null)
            {
                CurrentTool = ToolType.NONE;
                CurrentSeed = null;
                CurrentFertilizer = null;
                return ToolType.NONE;
            }

            if (item is ItemToolData toolData)
            {
                CurrentTool = toolData._toolType;
                CurrentSeed = null;
                CurrentFertilizer = null;
            }
            else if (item is ItemSeedData seed)
            {
                CurrentTool = ToolType.SEED;
                CurrentSeed = seed;
                CurrentFertilizer = null;
            }
            else if (item is FertilizerData fertilizer)
            {
                CurrentTool = ToolType.FERTILIZER;
                CurrentSeed = null;
                CurrentFertilizer = fertilizer;
            }
            else
            {
                CurrentTool = ToolType.NONE;
                CurrentSeed = null;
                CurrentFertilizer = null;
            }

            return CurrentTool;
        }

        public void SetTool(ToolType tool)
        {
            CurrentTool = tool;
            CurrentSeed = null;
            CurrentFertilizer = null;
        }

        public void SetSeed(ItemSeedData seed)
        {
            CurrentTool = ToolType.SEED;
            CurrentSeed = seed;
        }

        public void SetFertilizer(FertilizerData fertilizer)
        {
            CurrentTool = ToolType.FERTILIZER;
            CurrentFertilizer = fertilizer;
        }

        /// <summary>
        /// 수정 핵심:
        /// 기존: CurrentTool을 읽어서 분기 → context.ToolType과 불일치 가능
        /// 수정: toolType을 파라미터로 받아서 실행 → context가 결정한 타입 그대로 사용
        ///
        /// 모든 도구가 이 메서드를 거침 (낫 포함). 일관된 진입점 유지.
        /// </summary>
        public void ExecuteToolAction(Vector3Int cellPos, ToolType toolType)
        {
            if (FarmingManager.Instance == null) return;

            switch (toolType)
            {
                case ToolType.SCYTHE:
                    FarmingManager.Instance.HarvestTile(cellPos);
                    break;

                case ToolType.HOE:
                    FarmingManager.Instance.TillOrRestoreTile(cellPos);
                    EventBus.RaiseSFX(SFXType.TILL_SOIL);
                    break;

                case ToolType.WATERING_CAN:
                    FarmingManager.Instance.ExecuteAction(ToolType.WATERING_CAN, cellPos);
                    break;

                case ToolType.SEED:
                    FarmingManager.Instance.ExecuteAction(ToolType.SEED, cellPos, CurrentSeed);
                    break;

                case ToolType.FERTILIZER:
                    FarmingManager.Instance.ExecuteAction(ToolType.FERTILIZER, cellPos,
                        fertilizer: CurrentFertilizer);
                    break;

                case ToolType.BOW:
                    // BOW는 IdleState에서 AimState로 직접 처리.
                    // ToolManager까지 내려올 일 없음. 안전장치용 빈 케이스.
                    break;

                default:
                    Debug.LogWarning($"[ToolManager] 처리되지 않은 ToolType: {toolType}");
                    break;
            }
        }

        /// <summary>
        /// 하위 호환용 오버로드. CurrentTool 기반으로 실행.
        /// 신규 코드는 toolType 명시 버전 사용 권장.
        /// </summary>
        public void ExecuteToolAction(Vector3Int cellPos)
            => ExecuteToolAction(cellPos, CurrentTool);
    }
}