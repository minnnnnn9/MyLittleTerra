
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeUI : MonoBehaviour
{
    [SerializeField] private RecipeSlot _recipePrefab;
    [SerializeField] private Transform _parent;
    [SerializeField] private RecipeGroupData _recipeGroupData;
    


    private List<RecipeSlot> _recipeList = new();

  
    private void Start()
    {
        Init();
    }
    private void Init()
    {
        for (int i = 0; i < _recipeGroupData.Length; i++)
        {

            RecipeSlot slot = Instantiate(_recipePrefab, _parent);



            Sprite foodImage = _recipeGroupData.FoodDatas[i]._icon;
            string foodName = _recipeGroupData.FoodDatas[i]._name;
            string needIngredients = "필요한 재료 : ";

            if (_recipeGroupData.FoodDatas[i].RecipeData.Ingredients.Length != 3) continue;

            Sprite ingredientImage_1 = _recipeGroupData.FoodDatas[i].RecipeData.Ingredients[0]._icon;
            Sprite ingredientImage_2 = _recipeGroupData.FoodDatas[i].RecipeData.Ingredients[1]._icon;
            Sprite ingredientImage_3 = _recipeGroupData.FoodDatas[i].RecipeData.Ingredients[2]._icon;

            slot.SetRecipeSlot(foodImage, foodName, needIngredients,
                ingredientImage_1, ingredientImage_2, ingredientImage_3);

            _recipeList.Add(slot);

        }

    }

    private void Refresh()
    {

        //for (int i = 0; i < _recipeGroupData.Length; i++)
        //{


        //    Sprite foodImage = _recipeGroupData.FoodDatas[i]._icon;
        //    string foodName = _recipeGroupData.FoodDatas[i]._name;
        //    string needIngredients = "필요한 재료 : ";

        //    if (_recipeGroupData.FoodDatas[i].RecipeData.Ingredients.Length != 3) continue;

        //    Sprite ingredientImage_1 = _recipeGroupData.FoodDatas[i].RecipeData.Ingredients[0]._icon;
        //    Sprite ingredientImage_2 = _recipeGroupData.FoodDatas[i].RecipeData.Ingredients[1]._icon;
        //    Sprite ingredientImage_3 = _recipeGroupData.FoodDatas[i].RecipeData.Ingredients[2]._icon;

        //    _recipeList[i].SetRecipeSlot(foodImage, foodName, needIngredients,
        //        ingredientImage_1, ingredientImage_2, ingredientImage_3);

            
        //}


    }

    public void FindRecipeIndex(string foodname)
    {
        int discoveredRecipeIndex = -1;

        for (int i = 0; i < _recipeGroupData.Length; i++)
        {
            if (foodname == _recipeGroupData.FoodDatas[i]._name)
            {
                discoveredRecipeIndex = i;
                return;

            }

        }

        _recipeList[discoveredRecipeIndex].DiscoverRecipeImage();


    }

    private void Update()
    {
        Debug.LogWarning("레시피리스트 카운트 : " + _recipeList.Count);
    }
}
