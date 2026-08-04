using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeSlot : MonoBehaviour
{

    [SerializeField] private Image _foodImage;
    [SerializeField] private TextMeshProUGUI _foodName;
    [SerializeField] private TextMeshProUGUI _needIngredientNames;
    [SerializeField] private Image _ingredientImage_1;
    [SerializeField] private Image _ingredientImage_2;
    [SerializeField] private Image _ingredientImage_3;
    [SerializeField] private Image[] _notDiscoveredImages;
    [SerializeField] private Image _questionMark;

    //public Image FoodImage => _foodImage;
    //public TextMeshProUGUI FoodName => _foodName;
    //public TextMeshProUGUI NeedIngredientNames => _needIngredientNames;

    //public Image IngredientName_1 => _ingredientName_1;
    //public Image IngredientName_2 => _ingredientName_2;
    //public Image IngredientName_3 => _ingredientName_3;

    public void SetRecipeSlot(Sprite foodImage, string foodName,
        string needIngredientName, Sprite ingredientImage_1,
        Sprite ingredientImage_2, Sprite ingredientImage_3)
    {
        _foodImage.sprite = foodImage;
        _foodName.text = foodName;
        _needIngredientNames.text = needIngredientName;
        _ingredientImage_1.sprite = ingredientImage_1;
        _ingredientImage_2.sprite = ingredientImage_2;
        _ingredientImage_3.sprite = ingredientImage_3;

        for (int i = 0; i < _notDiscoveredImages.Length; i++)
        {
            _notDiscoveredImages[i].sprite = _questionMark.sprite;
        }

        _foodImage.enabled = false;
       
        _ingredientImage_1.enabled = false;
        _ingredientImage_2.enabled = false;
        _ingredientImage_3.enabled = false;
    }

    public void DiscoverRecipeImage()
    {
        for (int i = 0; i < _notDiscoveredImages.Length; i++)
        {
            _notDiscoveredImages[i].enabled = false;
        }
          

        _foodImage.enabled = true;
       
        _ingredientImage_1.enabled = true;
        _ingredientImage_2.enabled = true;
        _ingredientImage_3.enabled = true;

    }




}
