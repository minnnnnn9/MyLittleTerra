using UnityEngine;
using MLT.Farm;
using MLT.Core;
using MLT.Player;

namespace MLT.Machine
{
    public class SprinklerObject : InstallableObject
    {
        public override void OnInteract(PlayerController player)
        {
            // 클릭 시 "작동 중인 스프링클러입니다" 같은 메시지를 띄울 수 있음
        }

        public override void OnDayPassed()
        {
            // 매일 아침 WorldObjectManager에 의해 호출됨
            ExecuteSprinkler();
        }

        private void ExecuteSprinkler()
        {
            if (FarmingManager.Instance == null) return;

            // 3x3 범위 (중간 범위)
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    // 자기 자신 칸은 물 줄 필요 없음
                    if (x == 0 && y == 0) continue;

                    Vector3Int targetPos = CellPos + new Vector3Int(x, y, 0);

                    // FarmingManager의 물 주기 기능 실행
                    FarmingManager.Instance.ExecuteAction(ToolType.WATERING_CAN, targetPos);
                }
            }
            Debug.Log($"[Sprinkler] {CellPos} 주변에 물을 뿌렸습니다.");
        }
    }
}