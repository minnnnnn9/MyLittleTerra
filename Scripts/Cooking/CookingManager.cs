using MLT;
using MLT.Core;
using MLT.Data.ItemSO;
using MLT.Player;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 요리 로직 처리 (재료 조합 → 레시피 매칭 → 결과물 반환)
/// UI와 분리된 순수 로직 레이어 (SceneDataManager 금고 연동 완)
/// </summary>
public class CookingManager : MonoBehaviour
{
    public static CookingManager Instance { get; private set; }

    [SerializeField] private CookingConfig _config;

    // 중앙 금고 데이터 실시간 직결 프로퍼티
    private Dictionary<ItemBaseData, int> _inputInventory => MLT.Core.SceneDataManager.Instance.CookingInputInventory;
    private Queue<ItemBaseData> _outputQueue => MLT.Core.SceneDataManager.Instance.CookingOutputQueue;

    private bool _isProcessing
    {
        get => MLT.Core.SceneDataManager.Instance.IsCookingProcessing;
        set => MLT.Core.SceneDataManager.Instance.IsCookingProcessing = value;
    }
    private RecipeData _currentRecipe
    {
        get => MLT.Core.SceneDataManager.Instance.CookingCurrentRecipe;
        set => MLT.Core.SceneDataManager.Instance.CookingCurrentRecipe = value;
    }
    private int _lastCycleMinute
    {
        get => MLT.Core.SceneDataManager.Instance.CookingLastCycleMinute;
        set => MLT.Core.SceneDataManager.Instance.CookingLastCycleMinute = value;
    }
    private float _progressValue
    {
        get => MLT.Core.SceneDataManager.Instance.CookingProgress;
        set => MLT.Core.SceneDataManager.Instance.CookingProgress = value;
    }
    private ItemBaseData _pendingOutputItem
    {
        get => MLT.Core.SceneDataManager.Instance.CookingPendingItem;
        set => MLT.Core.SceneDataManager.Instance.CookingPendingItem = value;
    }
    private int _pendingOutputAmount
    {
        get => MLT.Core.SceneDataManager.Instance.CookingPendingAmount;
        set => MLT.Core.SceneDataManager.Instance.CookingPendingAmount = value;
    }

    // 외부 출력용 Public Getter 메서드 
    public Dictionary<ItemBaseData, int> GetInputInventory() => _inputInventory;
    public bool IsProcessing => _isProcessing;
    public ItemBaseData GetPendingOutput() => _pendingOutputItem;
    public int GetPendingOutputAmount() => _pendingOutputAmount;
    public Dictionary<ItemBaseData, int> GetIngredientDictionary() => _inputInventory;
    public float GetProgress() => _progressValue;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        //  맵에 돌아왔을 때 여전히 요리 중이라면 애니메이션 즉시 재생
        if (_isProcessing)
        {
            EventBus.RaiseCookingStarted();
        }
    }

    private void Update()
    {
        // _currentRecipe == null 조건 제거 
        if (!_isProcessing || TimeManager.Instance == null) return;

        float elapsed = TimeManager.Instance.TotalMinutes - _lastCycleMinute;
        _progressValue = Mathf.Clamp01(elapsed / _config.CookingMinutes);

        if (elapsed >= _config.CookingMinutes)
        {
            ItemBaseData resultItem;
            int resultAmount;

            if (_currentRecipe != null)
            {
                // 성공 시 레시피에 적힌 재료만 차감
                foreach (var ing in _currentRecipe.Ingredients)
                {
                    if (ing == null) continue;
                    _inputInventory[ing]--;
                    if (_inputInventory[ing] <= 0) _inputInventory.Remove(ing);
                }
                resultItem = _currentRecipe.ResultItem;
                resultAmount = 1;
            }
            else
            {
                // 실패 시 냄비에 든 재료 종류별로 1개씩 차감
                var keys = new System.Collections.Generic.List<ItemBaseData>(_inputInventory.Keys);
                foreach (var key in keys)
                {
                    if (key == null) continue;
                    _inputInventory[key]--;
                    if (_inputInventory[key] <= 0) _inputInventory.Remove(key);
                }
                resultItem = _config.FailItem;
                resultAmount = _config.FailAmount;
            }

            _outputQueue.Enqueue(resultItem);

            _pendingOutputAmount = (_pendingOutputItem == resultItem)
                ? _pendingOutputAmount + resultAmount : resultAmount;
            _pendingOutputItem = resultItem;

            _lastCycleMinute += _config.CookingMinutes;
            _progressValue = 0f;

            StartNextCycle();

            EventBus.RaiseCookingCompleted(resultItem, resultAmount);
        }
    }

    // UI의 요리 시작 버튼 클릭 시 호출
    public bool StartCooking(Dictionary<ItemBaseData, int> ingredientsToInsert)
    {
        if (ingredientsToInsert == null || ingredientsToInsert.Count == 0 || _isProcessing) return false;

        RecipeData matched = null;
        foreach (var recipe in _config.Recipes)
        {
            if (MatchRecipe(recipe, ingredientsToInsert)) { matched = recipe; break; }
        }


        foreach (var kvp in ingredientsToInsert)
        {
            if (!_inputInventory.ContainsKey(kvp.Key)) _inputInventory[kvp.Key] = 0;
            _inputInventory[kvp.Key] += kvp.Value;
        }

        _currentRecipe = matched;
        _lastCycleMinute = TimeManager.Instance.TotalMinutes;
        _isProcessing = true;
        _progressValue = 0f;

        EventBus.RaiseCookingStarted(); // 애니메이션 즉시 시작
        return true;
    }

    private void StartNextCycle()
    {
        if (!HasEnoughIngredients())
        {
            _isProcessing = false;
            _currentRecipe = null;
            EventBus.RaiseCookingStopped();
            return;
        }

        _isProcessing = true;
    }

    // 완성품 수령
    public bool CollectOutput(PlayerController player)
    {
        if (_outputQueue.Count == 0) return false;
        while (_outputQueue.Count > 0)
        {
            var item = _outputQueue.Dequeue();
        }
        _pendingOutputItem = null;
        _pendingOutputAmount = 0;
        return true;
    }

    // 남은 재료 반환
    public void EjectRemainingIngredients(PlayerController player)
    {
        _isProcessing = false;
        _currentRecipe = null;
        foreach (var kvp in _inputInventory)
            player.Data.Inventory.TryAddItem(kvp.Key, kvp.Value);
        _inputInventory.Clear();
    }

    private bool HasEnoughIngredients()
    {
        // 레시피가 없는 상태라면, 재료가 1개라도 남아있으면 계속 진행
        if (_currentRecipe == null)
        {
            return _inputInventory.Count > 0;
        }

        foreach (var ing in _currentRecipe.Ingredients)
        {
            if (ing == null) continue;
            if (!_inputInventory.TryGetValue(ing, out int count) || count < 1) return false;
        }
        return true;
    }

    private bool MatchRecipe(RecipeData recipe, Dictionary<ItemBaseData, int> inputs)
    {
        foreach (var ing in recipe.Ingredients)
        {
            if (ing == null) continue;
            if (!inputs.TryGetValue(ing, out int count) || count < 1) return false;
        }
        return true;
    }

    public void ClearPendingOutput()
    {
        _pendingOutputItem = null;
        _pendingOutputAmount = 0;
        _outputQueue.Clear();
    }
}