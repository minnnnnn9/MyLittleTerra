using MLT.Data.ItemSO;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 레시피 하나를 정의하는 ScriptableObject
/// 재료 3개의 조합이 매칭되면 결과 아이템 생성
/// </summary>
[CreateAssetMenu(fileName = "NewRecipe", menuName = "Cooking/Recipe")]
public class RecipeData : ScriptableObject
{
    [Header("재료 (순서 무관)")]
    public ItemBaseData[] Ingredients = new ItemBaseData[3];

    [Header("결과물")]
    public ItemFoodData ResultItem;
    public int ResultAmount = 1;

    [Header("설명 (내부용, 플레이어 비공개)")]
    [TextArea] public string _devNote;


    public bool HasRecipeDiscovered = false;
    /// <summary>
    /// 입력된 재료 배열이 이 레시피와 매칭되는지 확인 (순서 무관)
    /// </summary>
    public bool TryMatch(ItemBaseData[] inputIngredients)
    {
        if (inputIngredients == null) return false;

        // 1. 레시피에 설정된 실제 재료만 추출 (null 제거)
        List<ItemBaseData> recipeNeeds = new List<ItemBaseData>();
        foreach (var ing in Ingredients)
        {
            if (ing != null) recipeNeeds.Add(ing);
        }

        // 2. 플레이어가 입력한 실제 재료만 추출 (null 제거)
        List<ItemBaseData> inputItems = new List<ItemBaseData>();
        foreach (var input in inputIngredients)
        {
            if (input != null) inputItems.Add(input);
        }

        // 재료의 종류 개수가 다르면 매칭 실패
        if (recipeNeeds.Count != inputItems.Count) return false;

        // 3. 순서 상관없이 모든 재료가 일치하는지 확인
        foreach (var input in inputItems)
        {
            int idx = recipeNeeds.FindIndex(r => r == input);
            if (idx < 0) return false;
            recipeNeeds.RemoveAt(idx);
        }

        return recipeNeeds.Count == 0;
    }

    /// <summary>
    /// Dictionary 형태로 입력된 재료들과 매칭되는지 확인합니다.
    /// </summary>
    public bool TryMatchDictionary(Dictionary<ItemBaseData, int> inputs)
    {
        if (inputs == null || inputs.Count == 0) return false;

        List<ItemBaseData> recipeNeeds = new List<ItemBaseData>();
        foreach (var ing in Ingredients)
        {
            if (ing != null) recipeNeeds.Add(ing);
        }

        if (recipeNeeds.Count != inputs.Count) return false;

        foreach (var ing in recipeNeeds)
        {
            if (!inputs.TryGetValue(ing, out int count) || count < 1) return false;
        }

        return true;
    }

    public void RecipeDiscovered()
    {
        HasRecipeDiscovered = true;

    }
}