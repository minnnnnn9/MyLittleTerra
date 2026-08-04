using MLT.Core;
using MLT.Data.ItemSO;
using MLT.Machine;
using MLT.Player;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CookingUI : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private List<CookingSlot> _inputSlots = new List<CookingSlot>();
    [SerializeField] private CookingSlot _resultSlot;

    [Header("References")]
    [SerializeField] private PlayerController _player;
    [SerializeField] private CookingStove _stove;
    [SerializeField] private Image _progressImage;


    // ── 생명주기 ──────────────────────────────────────────────

    private void Start()
    {
        // 슬롯 초기화
        for (int i = 0; i < _inputSlots.Count; i++)
        {
            if (_inputSlots[i] != null) _inputSlots[i].Init(i);
        }
        if (_resultSlot != null) _resultSlot.Init(100);

        // UI 꺼져있어도 항상 이벤트 수신 (씨앗추출기와 동일한 방식)
        EventBus.OnCookingCompleted += HandleCookingCompleted;
        EventBus.OnCookingStarted += HandleCookingStarted;
    }

    private void OnDestroy()
    {
        EventBus.OnCookingCompleted -= HandleCookingCompleted;
        EventBus.OnCookingStarted -= HandleCookingStarted;
    }

    private void OnEnable()
    {
        // 슬롯 클릭은 UI 열릴 때만
        foreach (var slot in _inputSlots)
            slot.OnClicked += OnSlotClicked;
        if (_resultSlot != null) _resultSlot.OnClicked += OnSlotClicked;

        // UI 열릴 때 현재 상태 동기화 (씨앗추출기의 OpenUI → RefreshUI와 동일)
        SyncUIWithCurrentState();
    }

    private void OnDisable()
    {
        foreach (var slot in _inputSlots)
            slot.OnClicked -= OnSlotClicked;
        if (_resultSlot != null) _resultSlot.OnClicked -= OnSlotClicked;

        // UI 닫을 때 재료 슬롯 커서로 반환
        ReturnInputSlotsToCursor();
    }

    private void Update()
    {
        VisualProgress();
    }

    // ── 이벤트 핸들러 ─────────────────────────────────────────

    private void HandleCookingStarted()
    {
        
    }

    private void HandleCookingCompleted(ItemBaseData item, int amount)
    {
        RefreshInputSlots();

        if (_resultSlot.Item == item)
            _resultSlot.AddAmount(amount);
        else
            _resultSlot.SetItem(item, amount);
    }

    // ── UI 동기화 ─────────────────────────────────────────────

    /// <summary>
    /// UI 열릴 때 현재 CookingManager 상태로 즉시 갱신.
    /// 씨앗추출기의 OpenUI → RefreshUI와 동일한 패턴.
    /// </summary>
    private void SyncUIWithCurrentState()
    {
        if (CookingManager.Instance == null) return;

        RefreshInputSlots();

        var pending = CookingManager.Instance.GetPendingOutput();
        if (pending != null)
            _resultSlot.SetItem(pending, CookingManager.Instance.GetPendingOutputAmount());
        else
            _resultSlot.Clear();
    }

    private void RefreshInputSlots()
    {
        foreach (var slot in _inputSlots) slot.Clear();

        var currentIngredients = CookingManager.Instance.GetInputInventory();
        int index = 0;
        foreach (var kvp in currentIngredients)
        {
            if (index < _inputSlots.Count && kvp.Key != null)
            {
                _inputSlots[index].SetItem(kvp.Key, kvp.Value);
                index++;
            }
        }
    }

    public void RefreshResult(ItemBaseData item, int amount)
    {
        _resultSlot.SetItem(item, amount);
    }

    // ── Cook 버튼 ─────────────────────────────────────────────

    public void OnClickCookButton()
    {
        if (CookingManager.Instance == null)
        {
            Debug.LogError("[CookingUI] CookingManager.Instance가 null입니다.");
            return;
        }

        var ingredients = new Dictionary<ItemBaseData, int>();
        bool hasItem = false;

        foreach (var slot in _inputSlots)
        {
            if (slot.Item != null && slot.Amount > 0)
            {
                if (ingredients.ContainsKey(slot.Item))
                    ingredients[slot.Item] += slot.Amount;
                else
                    ingredients.Add(slot.Item, slot.Amount);
                hasItem = true;
            }
        }

        if (!hasItem)
        {
            Debug.Log("[CookingUI] 재료 슬롯이 비어있습니다.");
            return;
        }

        if (CookingManager.Instance.StartCooking(ingredients))
        {
            Debug.Log("[CookingUI] 요리 시작!");
        }
        else
        {
            Debug.LogWarning("[CookingUI] 요리 시작 실패 - 레시피 없음 또는 이미 요리 중");
        }
    }

    // ── 슬롯 클릭 ─────────────────────────────────────────────

    private void OnSlotClicked(CookingSlot slot)
    {
        if (CursorManager.Instance == null)
        {
            Debug.LogError("[CookingUI] CursorManager.Instance null");
            return;
        }

        // 결과 슬롯 클릭 - 수령 처리
        if (slot.Index == 100)
        {
            if (slot.Item == null) return;
            var cursor = CursorManager.Instance.CursorSlot;
            if (cursor.IsEmpty)
            {
                cursor.SetCursorSlot(slot.Item, slot.Amount);
                slot.Clear();
                CookingManager.Instance.ClearPendingOutput(); // pending 초기화
            }
            return;
        }

        // 재료 슬롯 - 요리 중에는 조작 불가
        if (CookingManager.Instance.IsProcessing)
        {
            Debug.Log("[요리 중] 재료를 꺼낼 수 없습니다.");
            return;
        }

        var cursorSlot = CursorManager.Instance.CursorSlot;

        if (slot.Item == null)
        {
            if (cursorSlot.IsEmpty) return;
            slot.SetItem(cursorSlot.Item, cursorSlot.Quantity);
            cursorSlot.Clear();
        }
        else
        {
            if (cursorSlot.IsEmpty)
            {
                cursorSlot.SetCursorSlot(slot.Item, slot.Amount);
                slot.Clear();
            }
            else
            {
                ItemBaseData tempItem = slot.Item;
                int tempAmount = slot.Amount;
                slot.SetItem(cursorSlot.Item, cursorSlot.Quantity);
                cursorSlot.SetCursorSlot(tempItem, tempAmount);
            }
        }
    }

    private void ReturnInputSlotsToCursor()
    {
        if (CookingManager.Instance == null) return;
        if (CookingManager.Instance.IsProcessing) return; // 요리 중이면 반환 안 함

        var cursor = CursorManager.Instance?.CursorSlot;
        if (cursor == null) return;

        foreach (var slot in _inputSlots)
        {
            if (slot.Item == null) continue;
            // 커서가 비어있으면 첫 번째 아이템만 커서로
            // 나머지는 인벤토리로 직접 반환
            if (cursor.IsEmpty)
            {
                cursor.SetCursorSlot(slot.Item, slot.Amount);
            }
            else
            {
                // 인벤토리로 반환
                if (_player != null)
                    _player.Data.Inventory.TryAddItem(slot.Item, slot.Amount);
            }
            slot.Clear();
        }
    }

    // ── 프로그레스 ────────────────────────────────────────────

    public void VisualProgress()
    {
        if (CookingManager.Instance == null || _progressImage == null) return;
        _progressImage.fillAmount = CookingManager.Instance.GetProgress();
    }
}