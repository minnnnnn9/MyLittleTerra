using MLT.Data.ItemSO;
using UnityEngine;

namespace MLT.Data
{
    [CreateAssetMenu(fileName = "AgingRecipe", menuName = "MLT/Recipe/Aging Recipe")]
    public class AgingRecipeData : ScriptableObject
    {
        [Header("입력")]
        public ItemBaseData InputItem;

        [Header("숙성 설정")]
        public int AgingDays = 3; // 숙성에 걸리는 일수

        [Header("출력")]
        public ItemBaseData OutputItem;
        public int OutputAmount = 1;
    }
}