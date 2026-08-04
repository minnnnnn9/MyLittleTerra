using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MLT.Data.ItemSO;
using TMPro;
using MLT;

public enum CookingSlotType
{
    INGREDIENT,
    RESULT
}

public class CookingSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    [SerializeField] private Image _icon;
    [SerializeField] private GameObject _iconObject;
    [SerializeField] private TextMeshProUGUI _amountText;
    [SerializeField] private CookingSlotType _slotType;

    private ItemBaseData _item;
    private int _amount;
    private int _index;

    public int Index => _index;
    public ItemBaseData Item => _item;
    public int Amount => _amount;


    public event Action<CookingSlot> OnClicked;


    public void Init(int index)
    {
        _index = index;
        Clear();
    }


    public void SetItem(ItemBaseData item, int amount)
    {
        _item = item;
        _amount = amount;

        if (_item != null && amount > 0)
        {
            _icon.sprite = _item._icon;
            _iconObject.SetActive(true);
            UpdateAmountUI();
        }
        else
        {
            Clear();
        }
    }


    public void AddAmount(int value)
    {
        if (_item == null) return;

        _amount += value;
        UpdateAmountUI();
    }


    public void RemoveAmount(int value)
    {
        if (_item == null) return;

        _amount -= value;

        if (_amount <= 0)
        {
            Clear();
        }
        else
        {
            UpdateAmountUI();
        }
    }

    public void Clear()
    {
        _item = null;
        _amount = 0;
        if (_icon.sprite != null)
        {
            _icon.sprite = null;
        }
        _iconObject.SetActive(false);
        _amountText.text = "";
    }


    private void UpdateAmountUI()
    {
        if (_amount > 1)
            _amountText.text = _amount.ToString();
        else
            _amountText.text = "";
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        OnClicked?.Invoke(this);
    }
}