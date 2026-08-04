using MLT.Data.ItemSO;
using UnityEngine;

/// <summary>
/// 요리 실패 시 반환될 쓰레기 음식 데이터
/// CookingManager에서 참조
/// </summary>
[CreateAssetMenu(fileName = "CookingConfig", menuName = "Cooking/Config")]
public class CookingConfig : ScriptableObject
{
    [Header("실패 결과물")]
    public ItemBaseData FailItem;
    public int FailAmount = 1;

    [Header("가공 설정")]
    public int CookingMinutes = 10; 

    [Header("레시피 목록")]
    public RecipeData[] Recipes;
}