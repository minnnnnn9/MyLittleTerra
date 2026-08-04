using MLT.Data.ItemSO;
using UnityEngine;

namespace MLT.Data
{
    /// <summary>
    /// 식품가공 레시피 - 재료 1종 1개 → 결과물 1개.
    /// (스타듀밸리 방식: 오이 1개 → 피클 1개)
    /// </summary>
    [CreateAssetMenu(fileName = "ProcessingRecipe", menuName = "MLT/Recipe/Processing Recipe")]
    public class ProcessingRecipeData : ScriptableObject
    {
        [Header("입력 재료 (1종 1개)")]
        public ItemBaseData InputItem;

        [Header("가공 시간")]
        public int ProcessingMinutes = 60;

        [Header("출력 (1개)")]
        public ItemBaseData OutputItem;
    }
}