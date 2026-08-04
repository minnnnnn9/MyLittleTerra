using UnityEngine;
using MLT.Core;
using MLT.Utils;

namespace MLT.Data.ItemSO
{
    [CreateAssetMenu(fileName = "ItemToolData", menuName = "Scriptable Objects/ItemToolData")]
    public class ItemToolData : ItemBaseData
    {
        [Header("도구 정보")]
        public ToolType _toolType;
        public ToolGrade _grade;

      

        [Header("애니메이션 세트")]
        public ToolAnimationSet _animationSet;

        public Color GetGradeColor() => _grade switch
        {
            ToolGrade.NONE => new Color(0.55f, 0.55f, 0.55f), // 돌 어두운 회색
            ToolGrade.COPPER => new Color(0.72f, 0.45f, 0.20f), // 구리 갈색
            ToolGrade.IRON => Color.white,                     // 철 기본 스프라이트
            ToolGrade.GOLD => new Color(1.00f, 0.80f, 0.00f), // 금 노란색
            ToolGrade.IRIDIUM => new Color(0.60f, 0.40f, 0.90f), // 이리듐 보라색
            _ => Color.white
        };

        // 인스펙터에서 값이 수정될 때마다 호출되는 함수
        private void OnValidate()
        {
            UpdateDescription();
        }

        public void UpdateDescription()
        {
            string stringBuilder = "";
            stringBuilder += $"등급 : {_grade}\n\n";
            stringBuilder += $"{Util.EnumChangeKOR(_toolType.ToString())}.";

            _description = stringBuilder;
        }
    }
}