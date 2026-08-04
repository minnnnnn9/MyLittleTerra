using MLT.Core;
using MLT.Data;
using MLT.Data.ItemSO;
using MLT.Player;
using MLT.World;
using System.Collections.Generic;
using UnityEngine;

namespace MLT.Machine
{
    public class AgingMachine : InstallableObject
    {
        [SerializeField] private ItemPlaceableData _data;
        [SerializeField] private List<AgingRecipeData> _recipes;

        private Animator _animator;
        private static readonly int AnimIsWorking = Animator.StringToHash("IsWorking");

        private AgingSlot _fallbackSlot = new();
        private AgingSlot _slot
        {
            get
            {
                if (SceneDataManager.Instance == null) return _fallbackSlot;
                if (!SceneDataManager.Instance.AgingStates.TryGetValue(CellPos, out var slot))
                {
                    slot = new AgingSlot();
                    SceneDataManager.Instance.AgingStates[CellPos] = slot;
                }
                return slot;
            }
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            if (_slot != null && !_slot.IsEmpty)
            {
                // [핵심] 씬 진입 시: 이미 다 익었으면 애니메이션 정지, 덜 익었으면 계속 재생!
                if (_slot.IsReady())
                {
                    SetAnimation(false);
                }
                else
                {
                    SetAnimation(true);
                }
            }
        }

        public override void OnInteract(PlayerController player)
        {
            if (player == null || player.Data == null || player.Data.Inventory == null) return;
            string machineName = _data != null ? _data._name : "숙성통";

            // 1. 완료 상태일 때 클릭하면 뱉어내기
            if (!_slot.IsEmpty && _slot.IsReady())
            {
                DropOutput(machineName);
                return;
            }

            // 2. 덜 익었을 때 클릭하면 남은 일수 표시
            if (!_slot.IsEmpty)
            {
                int elapsed = TimeManager.Instance.Day - _slot.InsertedDay;
                int remaining = _slot.Recipe.AgingDays - elapsed;
                Debug.Log($"[{machineName}] 숙성 중... 남은 일수: {remaining}일");
                return;
            }

            // 3. 비어있을 때 재료 넣기
            ItemBaseData heldItem = player.GetEquippedItem();
            if (heldItem == null) return;

            AgingRecipeData recipe = _recipes.Find(r => r.InputItem == heldItem);
            if (recipe == null) return;

            if (!player.Data.Inventory.HasItem(heldItem, 1)) return;

            EventBus.RaiseItemConsumed(heldItem, 1);
            EventBus.RaiseSFX(SFXType.MACHINE_INSERT);
            _slot.Insert(heldItem, recipe, TimeManager.Instance.Day);

            SetAnimation(true); // 재료 넣고 숙성 시작하면 보글보글!
            Debug.Log($"[{machineName}] {heldItem._name} 숙성 시작! {recipe.AgingDays}일 후 완성.");
        }

        public override void OnDayPassed()
        {
            if (_slot.IsEmpty) return;

            // [핵심] 날짜가 바뀌었을 때 다 익었다면 그 즉시 애니메이션 멈춤!
            if (_slot.IsReady())
            {
                SetAnimation(false);
                string machineName = _data != null ? _data._name : "숙성통";
                Debug.Log($"[{machineName}] {_slot.Recipe.OutputItem._name} 숙성 완료! 통을 클릭해 수령하세요.");
            }
        }

        public override void OnUninstall()
        {
            if (!_slot.IsEmpty && _dropPrefab != null)
            {
                Vector3 dropPos = transform.position + new Vector3(0f, -0.3f, 0f);
                DropSpawner.Spawn(_slot.InputItem, 1, dropPos, _dropPrefab);
                _slot.Clear();
            }
            if (_data != null && _dropPrefab != null)
                DropSpawner.Spawn(_data, 1, transform.position, _dropPrefab);

            if (SceneDataManager.Instance != null)
                SceneDataManager.Instance.AgingStates.Remove(CellPos);
        }

        private void DropOutput(string machineName)
        {
            if (_dropPrefab == null)
            {
                Debug.LogError($"🚨 [{machineName}] _dropPrefab이 비어있어서 아이템을 뱉을 수 없습니다! 인스펙터를 확인하세요.");
                return;
            }

            var item = _slot.Recipe.OutputItem;
            int amount = _slot.Recipe.OutputAmount;
            Vector3 dropPos = transform.position + new Vector3(0f, -0.3f, 0f);

            DropSpawner.Spawn(item, amount, dropPos, _dropPrefab);
            _slot.Clear();
            SetAnimation(false); // 수확 후 다시 멈춤 상태 유지
            Debug.Log($"[{machineName}] {item._name} x{amount} 드랍 완료!");
        }

        private void SetAnimation(bool isWorking)
        {
            if (_animator != null)
                _animator.SetBool(AnimIsWorking, isWorking);
        }
    }

    public class AgingSlot
    {
        public ItemBaseData InputItem { get; private set; }
        public AgingRecipeData Recipe { get; private set; }
        public int InsertedDay { get; private set; }
        public bool IsEmpty => InputItem == null;

        public void Insert(ItemBaseData item, AgingRecipeData recipe, int currentDay)
        {
            InputItem = item;
            Recipe = recipe;
            InsertedDay = currentDay;
        }

        public bool IsReady() =>
            !IsEmpty && TimeManager.Instance != null &&
            (TimeManager.Instance.Day - InsertedDay >= Recipe.AgingDays);

        public void Clear()
        {
            InputItem = null;
            Recipe = null;
            InsertedDay = 0;
        }
    }
}