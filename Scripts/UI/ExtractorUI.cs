using MLT.Core;
using MLT.Data.ItemSO;
using MLT.Machine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 수정 핵심:
/// - Update() 폴링 방식 제거
/// - EventBus.OnExtractorCycleCompleted 구독 추가
///   → 사이클 완료 시 pos가 현재 열린 기계와 같으면 즉시 RefreshUI()
///   → 요리UI의 HandleCookingCompleted와 동일한 패턴
/// </summary>
public class ExtractorUI : MonoBehaviour
{
    [SerializeField] private ExtractorSlot _insertSlot;
    [SerializeField] private ExtractorSlot _queueSlot;
    [SerializeField] private ExtractorSlot _resultSlot;
    [SerializeField] private Image _progressImage;

    private Vector3Int _currentMachinePos;
    private bool _isOpen;

    // ── 생명주기 ──────────────────────────────────────────────

    private void OnEnable()
    {
        EventBus.OnCloseAllMachineUI += CloseUI;
        EventBus.OnExtractorCycleCompleted += HandleCycleCompleted; // ← 핵심 추가
        _insertSlot.OnLeftClicked += OnInsertSlotClicked;
        _resultSlot.OnLeftClicked += OnResultSlotClicked;

        if (_isOpen) RefreshUI();
    }

    private void OnDisable()
    {
        EventBus.OnCloseAllMachineUI -= CloseUI;
        EventBus.OnExtractorCycleCompleted -= HandleCycleCompleted;
        _insertSlot.OnLeftClicked -= OnInsertSlotClicked;
        _resultSlot.OnLeftClicked -= OnResultSlotClicked;

        ReturnInsertSlotToCursor();
    }

    private void Update()
    {
        if (!_isOpen) return;
        _progressImage.fillAmount = SeedExtractorManager.Instance.GetProgress(_currentMachinePos);
    }

    // ── 공개 API ──────────────────────────────────────────────

    public void OpenUI(Vector3Int machinePos)
    {
        _currentMachinePos = machinePos;
        _isOpen = true;
        _insertSlot.Clear();
        RefreshUI();
        gameObject.SetActive(true);
    }

    public void CloseUI()
    {
        _isOpen = false;
        gameObject.SetActive(false);
    }

    // ── 이벤트 핸들러 ─────────────────────────────────────────

    /// <summary>
    /// 사이클 완료 시 Manager가 발행 → 열린 기계의 pos와 같을 때만 즉시 갱신.
    /// UI 닫고 열어야 갱신되던 문제 해결.
    /// </summary>
    private void HandleCycleCompleted(Vector3Int completedPos)
    {
        if (!_isOpen) return;
        if (completedPos != _currentMachinePos) return; // 다른 기계 완성은 무시
        RefreshUI();
    }

    // ── 버튼 핸들러 ───────────────────────────────────────────

    public void OnClickExtractButton()
    {
        if (_insertSlot.Item is not ItemCropData crop) return;
        EventBus.RaiseSFX(SFXType.DIALOGUE_OPEN);

        int count = _insertSlot.Amount;
        var list = new List<ItemCropData>(count);
        for (int i = 0; i < count; i++) list.Add(crop);

        SeedExtractorManager.Instance.InsertCrops(_currentMachinePos, list);
        _insertSlot.Clear();
        RefreshUI();
    }

    // ── 슬롯 클릭 ─────────────────────────────────────────────

    private void OnInsertSlotClicked(ExtractorSlot slot)
    {
        var cursor = CursorManager.Instance.CursorSlot;

        if (slot.Item == null)
        {
            if (cursor.IsEmpty) return;
            if (cursor.Item is not ItemCropData) return;
            slot.SetItem(cursor.Item, cursor.Quantity);
            cursor.Clear();
        }
        else
        {
            if (cursor.IsEmpty)
            {
                cursor.SetCursorSlot(slot.Item, slot.Amount);
                slot.Clear();
            }
            else
            {
                if (cursor.Item is not ItemCropData) return;
                var tempItem = slot.Item;
                int tempAmount = slot.Amount;
                slot.SetItem(cursor.Item, cursor.Quantity);
                cursor.SetCursorSlot(tempItem, tempAmount);
            }
        }
    }

    private void OnResultSlotClicked(ExtractorSlot slot)
    {
        if (slot.Item == null) return;

        var cursor = CursorManager.Instance?.CursorSlot;
        if (cursor == null) return;

        if (cursor.IsEmpty)
        {
            cursor.SetCursorSlot(slot.Item, slot.Amount);
            slot.Clear();
            SeedExtractorManager.Instance.CollectOutput(_currentMachinePos);
            RefreshUI(); 
        }
        else if (cursor.Item == slot.Item)
        {
            cursor.SetCursorSlot(cursor.Item, cursor.Quantity + slot.Amount);
            slot.Clear();
            SeedExtractorManager.Instance.CollectOutput(_currentMachinePos);
            RefreshUI(); 
        }
    }

    // ── UI 갱신 ──────────────────────────────────────────────

    private void RefreshUI()
    {
        var state = SeedExtractorManager.Instance.GetState(_currentMachinePos);

        if (state == null)
        {
            _queueSlot.Clear();
            _resultSlot.Clear();
            return;
        }

        if (state.InputQueue.Count > 0)
            _queueSlot.SetItem(state.InputQueue.Peek(), state.InputQueue.Count);
        else
            _queueSlot.Clear();

        int totalAmount = 0;
        ItemSeedData lastSeed = null;
        foreach (var o in state.OutputQueue)
        {
            lastSeed = o.Seed;
            totalAmount += o.Amount;
        }

        if (lastSeed != null) _resultSlot.SetItem(lastSeed, totalAmount);
        else _resultSlot.Clear();
    }

    private void ReturnInsertSlotToCursor()
    {
        if (_insertSlot == null || _insertSlot.Item == null) return;

        var cursor = CursorManager.Instance?.CursorSlot;
        if (cursor != null && cursor.IsEmpty)
            cursor.SetCursorSlot(_insertSlot.Item, _insertSlot.Amount);

        _insertSlot.Clear();
    }
}