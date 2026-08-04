using MLT.Data.ItemSO;
using MLT.Player;
using MLT.UI;
using MLT.World;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace MLT.Core
{
    public static class EventBus
    {
        // ───────────────────────────────────────────
        // 플레이어
        // ───────────────────────────────────────────
        public static event Action<int> OnEnergyChanged;
        public static void RaiseEnergyChanged(int currentEnergy)
        {
            OnEnergyChanged?.Invoke(currentEnergy);
        }

        public static event Action OnSpentAllEnergy;
        public static void RaiseSpentAllEnergy()
        {
            OnSpentAllEnergy?.Invoke();
        }

        public static event Action<int> OnGoldChanged;
        public static void RaiseGoldChanged(int gold)
        {
            OnGoldChanged?.Invoke(gold);
        }

        public static event Action<int> OnDebtChanged;
        public static void RaiseDebtChanged(int gold)
        {
            OnDebtChanged?.Invoke(gold);
        }

        public static event Action OnOpenInventory;
        public static void RaiseOpenInventory()
        {
            OnOpenInventory?.Invoke();
        }

        public static event Action OnCloseInventory;
        public static void RaiseCloseInventory()
        {

            OnCloseInventory?.Invoke();
        }

        public static event Action<float> OnScrollHotBar;

        public static void RaiseScrollHotBar(float value)
        {
            OnScrollHotBar?.Invoke(value);

        }

        public static event Action<int> OnHotBarPressed;
        public static void RaiseOnHotBarPressed(int numberKey)
        {

            OnHotBarPressed?.Invoke(numberKey);
        }

        // ───────────────────────────────────────────
        // 다이얼로그
        // ───────────────────────────────────────────

        public static event Action<NPCInteractable, PlayerController> OnDialogueStarted;
        public static void RaiseDialogueStarted(NPCInteractable npc, PlayerController player)
        {
            OnDialogueStarted?.Invoke(npc, player);
        }

        public static event Action OnDialogueEnded;
        public static void RaiseDialogueEnded()
        {
            OnDialogueEnded?.Invoke();
        }

        public static UnityEvent<int> CustomBtnEvent;
        public static void RaiseDialogueEnded(int val)
        {
            OnDialogueEnded?.Invoke();
        }

        // ───────────────────────────────────────────
        // 채광
        // ───────────────────────────────────────────

        public static event Action<RockInteractable, PlayerController> OnMiningMinigameRequested;
        public static void RaiseMiningMinigameRequested(RockInteractable rock, PlayerController player)
        {
            OnMiningMinigameRequested?.Invoke(rock, player);
        }

        public static event Action<RockInteractable, bool, bool> OnMiningMinigameResolved;
        public static void RaiseMiningMinigameResolved(RockInteractable rock, bool success, bool greatSuccess)
        {
            OnMiningMinigameResolved?.Invoke(rock, success, greatSuccess);
        }

        // ───────────────────────────────────────────
        // 농사
        // ───────────────────────────────────────────

        public static event Action<ItemCropData> OnHarvested;
        public static void RaiseHarvested(ItemCropData crop)
        {
            OnHarvested?.Invoke(crop);
        }

        public static event Action<ItemCropData, Vector3> OnHarvestedDrop;
        public static void RaiseHarvestedDrop(ItemCropData crop, Vector3 worldPos)
        {
            OnHarvestedDrop?.Invoke(crop, worldPos);
        }

        public static event Action<ItemSeedData> OnSeedUsed;
        public static void RaiseSeedUsed(ItemSeedData seed)
        {
            OnSeedUsed?.Invoke(seed);
        }

        // ───────────────────────────────────────────
        // 씨앗 추출기
        // ───────────────────────────────────────────

        public static event Action<ItemCropData> OnCropConsumed;
        public static void RaiseCropConsumed(ItemCropData crop)
        {
            OnCropConsumed?.Invoke(crop);
        }

        public static event Action<ItemSeedData, int> OnSeedCollected;
        public static void RaiseSeedCollected(ItemSeedData seed, int amount)
        {
            OnSeedCollected?.Invoke(seed, amount);
        }

       
        public static event Action<Vector3Int> OnOpenExtractorUI;
        public static void RaiseOpenExtractorUI(Vector3Int pos)
        {
            OnOpenExtractorUI?.Invoke(pos);
        }

        /// <summary>
        /// 씨앗추출기 1사이클 완료 시 발행.
        /// ExtractorUI가 구독해서 열려있는 기계의 UI를 즉시 갱신.
        /// pos = 완성된 기계의 셀 좌표 → UI가 자신의 기계인지 판별.
        /// </summary>
        public static event Action<Vector3Int> OnExtractorCycleCompleted;
        public static void RaiseExtractorCycleCompleted(Vector3Int pos)
        {
            OnExtractorCycleCompleted?.Invoke(pos);
        }


        // ───────────────────────────────────────────
        // 요리
        // ───────────────────────────────────────────
        public static event Action OnCookingStarted;
        public static void RaiseCookingStarted()
        {
            OnCookingStarted?.Invoke();
        }

        public static event Action<ItemBaseData, int> OnCookingCompleted;
        public static void RaiseCookingCompleted(ItemBaseData item, int amount)
        {
            OnCookingCompleted?.Invoke(item, amount);
        }

        public static event Action OnCookingStopped;
        public static void RaiseCookingStopped()
        {
            OnCookingStopped?.Invoke();
        }

        public static event Action OnOpenCookingUI;
        public static void RaiseOpenCookingUI()
        {
            OnOpenCookingUI?.Invoke();
        }

        // ───────────────────────────────────────────
        // 공통 아이템 소모 (숙성/가공 재료 차감)
        // ───────────────────────────────────────────

        public static event Action<ItemBaseData, int> OnItemConsumed;
        public static void RaiseItemConsumed(ItemBaseData item, int amount)
        {
            OnItemConsumed?.Invoke(item, amount);
        }

        // ───────────────────────────────────────────
        // 숙성 기계
        // ───────────────────────────────────────────

        public static event Action<ItemBaseData, int> OnAgingCompleted;
        public static void RaiseAgingCompleted(ItemBaseData item, int amount)
        {
            OnAgingCompleted?.Invoke(item, amount);
        }

        // ───────────────────────────────────────────
        // 가공 기계
        // ───────────────────────────────────────────

        public static event Action<ItemBaseData, int> OnProcessingCompleted;
        public static void RaiseProcessingCompleted(ItemBaseData item, int amount)
        {
            OnProcessingCompleted?.Invoke(item, amount);
        }

        public static event Action OnOpenProcessingUI;
        public static void RaiseOpenProcessingUI()
        {
            OnOpenProcessingUI?.Invoke();
        }

        // ───────────────────────────────────────────
        // 모든 기계UI 닫기
        // ───────────────────────────────────────────

        public static event Action OnCloseAllMachineUI; 
        public static void RaiseCloseAllMachineUI()
        {
            OnCloseAllMachineUI?.Invoke();
        }

        // ───────────────────────────────────────────
        // 시간
        // ───────────────────────────────────────────
        public static event Action OnDayStarted;
        public static void RaiseDayStarted()
        {
            OnDayStarted?.Invoke();
        }


        // 컷씬

        public static event Action OnEnterCutscene;
        public static void RaiseEnterCutscene()
        {
            OnEnterCutscene?.Invoke();
        }


        public static event Action OnExitCutscene;
        public static void RaiseExitCutscene()
        {
            OnExitCutscene?.Invoke();
        }

        // 상점

        public static event Action<int> OnOpenShop;
        public static void RaiseOnOpenShop(int index)
        {
            OnOpenShop?.Invoke(index);
        }


        public static event Action<int> OnCloseShop;
        public static void RaiseOnCloseShop(int index)
        {
            OnCloseShop?.Invoke(index);
        }

        public static event Action<int, PlayerData, IInventory> OnSellItem;

        public static void RaiseOnSellItem(int index, PlayerData player, IInventory inventory)
        {
            OnSellItem?.Invoke(index, player, inventory);

        }

        public static event Action<int> OnBuyItem;

        public static void RaiseOnBuyItem(int index)
        {
            OnBuyItem?.Invoke(index);

        }


        //대화

        public static event Action OnDialogueEnd;

        public static void RaiseDialogueEnd()
        {
            OnDialogueEnd?.Invoke();

        }

        //사운드 이펙트

        public static event System.Action<SFXType> OnSFXRequested;

        public static void RaiseSFX(SFXType type)
        {
            OnSFXRequested?.Invoke(type);
        }
    }
}