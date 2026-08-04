using System;
using MLT.Core;
using MLT.Data.ItemSO;
using MLT.Data;
using UnityEngine;

namespace MLT.Farm
{
    [Serializable]
    public class FarmTile
    {
        public FarmState State { get; private set; } = FarmState.EMPTY;
        public ItemSeedData CropData { get; private set; }
        public bool IsWatered { get; private set; }
        public FertilizerData Fertilizer { get; private set; }

        // 물 준 시간만 누적 (성장 판정 기준)
        public int WateredMinutes { get; private set; }

        // 이전 호출 시점 저장 (실제 경과 분 계산용)
        private int _lastProcessedMinutes = -1;

        public event Action OnTileChanged;

        private bool _isRegrowing = false;

        public void ClearEvents() => OnTileChanged = null;

        // ── 경작 ───────────────────────────────────────────────

        public void Till()
        {
            if (State != FarmState.EMPTY) return;
            ResetToTilled();
            NotifyChange();
        }

        // ── 비료 ───────────────────────────────────────────────

        public void Fertilize(FertilizerData fertilizer)
        {
            if (State != FarmState.TILLED && State != FarmState.SEEDED) return;
            if (Fertilizer != null) return;
            Fertilizer = fertilizer;
            NotifyChange();
        }

        // ── 씨앗 ───────────────────────────────────────────────

        public void Sow(ItemSeedData seed, int currentTotalMinutes)
        {
            if (State != FarmState.TILLED) return;
            if (!System.Array.Exists(seed._seasons, s => s == TimeManager.Instance.Season))
            {
                Debug.Log($"[{seed._name}]은 현재 계절({TimeManager.Instance.Season})에 심을 수 없습니다.");
                return;
            }
            CropData = seed;
            WateredMinutes = 0;
            _lastProcessedMinutes = currentTotalMinutes;
            State = FarmState.SEEDED;
            NotifyChange();
        }

        // ── 물 주기 ────────────────────────────────────────────

        public void Water()
        {
            if (State == FarmState.EMPTY || IsWatered) return;
            IsWatered = true;
            NotifyChange();
        }

        // ── 매 시간 성장 판정 ──────────────────────────────────

        public void ProcessTimePass(int currentTotalMinutes, bool isRaining)
        {
            // 비 오면 자동으로 물 준 상태
            if (isRaining && !IsWatered)
            {
                IsWatered = true;
                NotifyChange();
            }

            if (State != FarmState.SEEDED && State != FarmState.GROWING) return;

            bool moistProtected = Fertilizer?.Type == FertilizerType.MoistSoil;

            // 물 줬을 때만 실제 경과 분 누적
            if (IsWatered || moistProtected)
            {
                if (_lastProcessedMinutes >= 0)
                    WateredMinutes += currentTotalMinutes - _lastProcessedMinutes;
            }
            // 물 없으면 성장 멈춤 (누적 안 함)

            _lastProcessedMinutes = currentTotalMinutes;

            int growthMinutes = GetActualGrowthMinutes();
            FarmState next = WateredMinutes >= growthMinutes
                ? FarmState.HARVESTABLE
                : FarmState.GROWING;

            if (next == State) return;
            State = next;
            NotifyChange();
        }

        // ── 하루 마무리 ────────────────────────────────────────

        public void ProcessDayEnd(bool isRaining)
        {
            if (State == FarmState.EMPTY || State == FarmState.TILLED) return;

            bool moistProtected = Fertilizer?.Type == FertilizerType.MoistSoil;

            if (moistProtected)
            {
                Fertilizer = null; // 보습 토양 1회 소모
                return;
            }

            // 하루 끝에 물 한 번도 안 줬으면 즉시 소멸
            if (!isRaining && !IsWatered)
            {
                if (State == FarmState.SEEDED || State == FarmState.GROWING)
                {
                    ResetToTilled();
                    NotifyChange();
                    return;
                }
            }

            // 다음날을 위해 물 상태 리셋
            if (!isRaining && IsWatered)
            {
                IsWatered = false;
                NotifyChange();
            }
        }

        // ── 수확 ───────────────────────────────────────────────

        public ItemCropData Harvest()
        {
            if (State != FarmState.HARVESTABLE) return null;

            var result = CropData._cropData;

            if (CropData._canRegrow)
            {
                State = FarmState.GROWING;
                _isRegrowing = true;
                int totalMinutes = CropData._growthDurationHours * 60;
                float targetProgress = (CropData._regrowSpriteIndex + 0.5f) / CropData.growthSprites.Length;
                WateredMinutes = Mathf.RoundToInt(targetProgress * totalMinutes);

                _lastProcessedMinutes = TimeManager.Instance.TotalMinutes;
            }
            else
            {
                ResetToTilled();
            }

            NotifyChange();
            return result;
        }

        // ── 작물 강제 제거 ─────────────────────────────────────

        public void KillCrop()
        {
            if (State == FarmState.EMPTY || State == FarmState.TILLED) return;
            ResetToTilled();
            NotifyChange();
        }

        // ── 내부 유틸 ──────────────────────────────────────────

        private int GetActualGrowthMinutes()
        {
            if (CropData == null) return int.MaxValue;
            int targetHours = _isRegrowing ? CropData._regrowDurationHours : CropData._growthDurationHours;
            int baseMinutes = CropData._growthDurationHours * 60;
            if (Fertilizer?.Type == FertilizerType.GrowthBooster)
                baseMinutes = Mathf.RoundToInt(baseMinutes * (1f - Fertilizer.GrowthSpeedMultiplier));
            return Mathf.Max(1, baseMinutes);
        }

        private void ResetToTilled()
        {
            State = FarmState.TILLED;
            CropData = null;
            Fertilizer = null;
            IsWatered = false;
            WateredMinutes = 0;
            _lastProcessedMinutes = -1;
            _isRegrowing = false;
        }

        private void NotifyChange() => OnTileChanged?.Invoke();
    }
}