using MLT;
using MLT.Core;
using MLT.Data;
using MLT.Data.ItemSO;
using System.Collections.Generic;
using UnityEngine;

namespace MLT.Machine
{
    public struct SeedOutput
    {
        public ItemSeedData Seed;
        public int Amount;
    }

    public class SeedExtractorManager : MonoBehaviour
    {
        public static SeedExtractorManager Instance { get; private set; }

        [SerializeField] private SeedExtractorConfigData _config;

        private Dictionary<Vector3Int, ExtractorState> _states => SceneDataManager.Instance.ExtractorStates;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Update()
        {
            if (TimeManager.Instance == null) return;

            foreach (var kvp in _states)
            {
                var state = kvp.Value;
                if (!state.IsProcessing) continue;

                float elapsed = TimeManager.Instance.TotalMinutes - state.LastCycleMinute;
                state.Progress = Mathf.Clamp01(elapsed / _config.ProcessingMinutes);

                if (elapsed >= _config.ProcessingMinutes)
                    ProcessOneCycle(kvp.Key, state);  // pos 전달
            }
        }

        private void ProcessOneCycle(Vector3Int pos, ExtractorState state)
        {
            if (state.InputQueue.Count == 0) { state.IsProcessing = false; return; }

            var crop = state.InputQueue.Dequeue();

            ItemSeedData seed = (Random.value < _config.AncientSeedChance)
                ? _config.AncientSeed
                : FindSeed(crop);

            if (seed != null)
            {
                int amount = Random.Range(_config.MinOutput, _config.MaxOutput + 1);
                state.OutputQueue.Enqueue(new SeedOutput { Seed = seed, Amount = amount });
            }

            state.LastCycleMinute += _config.ProcessingMinutes;
            state.Progress = 0f;
            state.IsProcessing = state.InputQueue.Count > 0;

            // ── 핵심 수정 ──────────────────────────────────────────────
            // 사이클 완료 시점에 이벤트 발행 → ExtractorUI가 즉시 RefreshUI() 호출
            // 요리UI의 OnCookingCompleted와 동일한 패턴
            // ─────────────────────────────────────────────────────────────
            EventBus.RaiseExtractorCycleCompleted(pos);
        }

        /// <summary>플레이어 결과 슬롯 클릭 시 호출. 여기서만 인벤토리 추가.</summary>
        public void CollectOutput(Vector3Int pos)
        {
            var state = GetState(pos);
            if (state == null || state.OutputQueue.Count == 0) return;
            state.OutputQueue.Clear(); // 이벤트 발행 없이 큐만 비움
        }

        public void InsertCrops(Vector3Int pos, List<ItemCropData> crops)
        {
            if (!_states.ContainsKey(pos)) _states[pos] = new ExtractorState();
            var state = _states[pos];

            foreach (var crop in crops) state.InputQueue.Enqueue(crop);

            if (!state.IsProcessing)
            {
                state.LastCycleMinute = TimeManager.Instance.TotalMinutes;
                state.IsProcessing = true;
                state.Progress = 0f;
            }
        }

        public ExtractorState GetState(Vector3Int pos)
            => _states.TryGetValue(pos, out var s) ? s : null;

        public float GetProgress(Vector3Int pos)
        {
            var state = GetState(pos);
            return state?.Progress ?? 0f;
        }

        private ItemSeedData FindSeed(ItemCropData crop)
        {
            var recipe = _config.Recipes.Find(r => r.Crop == crop);
            return recipe.Seed;
        }
    }
}