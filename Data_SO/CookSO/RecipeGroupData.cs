using MLT.Data.ItemSO;

using UnityEngine;

/// <summary>
/// 레시피 하나를 정의하는 ScriptableObject
/// 재료 3개의 조합이 매칭되면 결과 아이템 생성
/// </summary>
[CreateAssetMenu(fileName = "NewRecipe", menuName = "Cooking/Recipe")]
public class RecipeGroupData : ScriptableObject
{

    public ItemFoodData[] FoodDatas;

    public int Length => FoodDatas.Length;
    
}