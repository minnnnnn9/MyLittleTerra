using MLT;
using MLT.Data.ItemSO;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ExtractorSlotType
{
    INSERT,
    RESULT
   
}



public class ExtractorSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    [SerializeField] private Image _icon;
    [SerializeField] private GameObject _iconObject;
    [SerializeField] private TextMeshProUGUI _amountText;
    [SerializeField] private ExtractorSlotType _slotType;

    private ItemBaseData _item;
    private int _amount;
    private int _index;

    public bool IsEmpty { get; private set; }
    public int Index => _index;
    public ItemBaseData Item => _item;
    public int Amount => _amount;


    public event Action<ExtractorSlot> OnLeftClicked;
    public event Action<ExtractorSlot> OnRightClicked;



    public void Init(int index)
    {
        IsEmpty = true;
        _index = index;
        Clear();
    }


    public void SetItem(ItemBaseData item, int amount)
    {
        _item = item;
        _amount = amount;

        if (_item != null && _amount > 0)
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

        _icon.sprite = null;
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



        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClicked?.Invoke(this);

        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClicked?.Invoke(this);
        }

    }
}