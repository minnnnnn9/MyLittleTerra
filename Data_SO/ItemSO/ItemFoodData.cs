using UnityEngine;
using MLT.Data.ItemSO;

namespace MLT.Data.ItemSO
{
    [CreateAssetMenu(fileName = "ItemFoodData", menuName = "Scriptable Objects/ItemFoodData")]
    public class ItemFoodData : ItemBaseData // 요리 음식 SO
    {
        [Header("음식 정보")]
      
        public bool _isFailFood;            // 실패 음식 여부 (쓰레기 음식)
        [SerializeField] private RecipeData _recipeData;

        public RecipeData RecipeData => _recipeData;

        public override int StaminaRecoveryAmount => _recipeData.Ingredients[0].StaminaRecoveryAmount +
            _recipeData.Ingredients[1].StaminaRecoveryAmount + _recipeData.Ingredients[2].StaminaRecoveryAmount;

        public override int SellPrice => _recipeData.Ingredients[0].SellPrice +
            _recipeData.Ingredients[1].SellPrice + _recipeData.Ingredients[2].SellPrice;
        public override int BuyPrice => _recipeData.Ingredients[0].BuyPrice +
            _recipeData.Ingredients[1].BuyPrice + _recipeData.Ingredients[2].BuyPrice;

        private void OnValidate()
        {
            UpdateDescription();
        }

        public void UpdateDescription()
        {
            string tempstr = "";

            if (_recipeData == null)
            {
              

            }
            else
            {
                if (_recipeData.Ingredients.Length > 0)
                {
                    for (int i = 0; i < _recipeData.Ingredients.Length; i++)
                    {


                        if (i > 0)
                        {
                            tempstr += $", {_recipeData.Ingredients[i]._name}";
                        }
                        else
                        {
                            tempstr += $"{_recipeData.Ingredients[i]._name}";
                        }
                    }


                    tempstr += "로 만든 음식\n\n";
                }


            }



            string str = "";
            str += _isFailFood
                ? "뭔가 이상한 냄새가 난다..."
                : tempstr + $"섭취 시 스태미나 {StaminaRecoveryAmount} 회복\n" + "핫바에서 선택 후 마우스 우클릭";
            _description = str;



        }
    }
}