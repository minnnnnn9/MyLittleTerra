using MLT.Core;
using MLT.Data;
using MLT.Data.ItemSO;
using MLT.Player;
using MLT.World;
using System.Collections.Generic;
using UnityEngine;

namespace MLT.Machine
{
    public class ProcessingMachine : InstallableObject
    {
        [SerializeField] private ItemPlaceableData _data;
        [SerializeField] private List<ProcessingRecipeData> _recipes;

        private Animator _animator;
        private static readonly int AnimIsWorking = Animator.StringToHash("IsWorking");

        // ── 💰 금고(SceneDataManager) 데이터 실시간 직결 ──
        private ProcessingState _fallbackState = new();
        private ProcessingState _state
        {
            get
            {
                if (SceneDataManager.Instance == null) return _fallbackState;
                if (!SceneDataManager.Instance.ProcessingStates.TryGetValue(CellPos, out var state))
                {
                    state = new ProcessingState();
                    SceneDataManager.Instance.ProcessingStates[CellPos] = state;
                }
                return state;
            }
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            // ── 🔄 씬 복귀 처리: 흘러간 시간 계산 ──
            if (_state.IsProcessing)
            {
                if (TimeManager.Instance != null && _state.Recipe != null)
                {
                    int elapsed = TimeManager.Instance.TotalMinutes - _state.StartMinute;

                    if (elapsed >= _state.Recipe.ProcessingMinutes)
                    {
                        CompleteCycle(); // 가공이 이미 끝났다면 완성 상태로 전환
                    }
                    else
                    {
                        SetAnimation(true); // 아직 가공 중이면 애니메이션 재생
                    }
                }
            }
            else if (_state.PendingOutput != null)
            {
                SetAnimation(false); // 완성품 대기 중이면 애니메이션 정지 유지
            }
            else
            {
                // 완전히 빈 상태일 때 애니메이션이 돌아가는 버그 방지 (전원 리셋)
                ForceResetAnimation();
            }
        }

        public override void OnInteract(PlayerController player)
        {
            string machineName = _data != null ? _data._name : "식품가공기";

            // 1. 완성품 대기 중일 때 클릭하면 아이템을 드롭하고 종료
            if (_state.PendingOutput != null)
            {
                DropOutput(machineName);
                return;
            }

            // 2. 가공 중 → 남은 시간 알림
            if (_state.IsProcessing)
            {
                if (TimeManager.Instance == null) return;
                int elapsed = TimeManager.Instance.TotalMinutes - _state.StartMinute;
                int remaining = _state.Recipe.ProcessingMinutes - elapsed;
                Debug.Log($"[{machineName}] 가공 중... 남은 시간: {remaining}분");
                return;
            }

            if (player == null) return;

            // 3. 핫바 아이템 및 레시피 확인
            ItemBaseData heldItem = player.GetEquippedItem();
            if (heldItem == null)
            {
                Debug.Log($"[{machineName}] 아이템을 들고 상호작용하세요.");
                return;
            }

            ProcessingRecipeData recipe = FindMatchingRecipe(heldItem);
            if (recipe == null)
            {
                Debug.Log($"[{machineName}] '{heldItem._name}'에 맞는 레시피가 없습니다.");
                return;
            }

            if (player.Data == null || player.Data.Inventory == null) return;

            if (!player.Data.Inventory.HasItem(heldItem, 1))
            {
                Debug.Log($"[{machineName}] {heldItem._name}이 부족합니다.");
                return;
            }

            // 새로운 가공 시작
            EventBus.RaiseItemConsumed(heldItem, 1);
            EventBus.RaiseSFX(SFXType.MACHINE_INSERT);

            _state.Recipe = recipe;
            _state.StartMinute = TimeManager.Instance.TotalMinutes;
            _state.IsProcessing = true;

            SetAnimation(true);
            Debug.Log($"[{machineName}] {heldItem._name} 가공 시작! {recipe.ProcessingMinutes}분 후 완성.");
        }

        public override void OnUninstall()
        {
            if (_state.IsProcessing && _state.Recipe != null && _dropPrefab != null)
            {
                Vector3 dropPos = transform.position + new Vector3(0f, -0.3f, 0f);
                DropSpawner.Spawn(_state.Recipe.InputItem, 1, dropPos, _dropPrefab);
                _state.Clear();
            }

            if (_state.PendingOutput != null && _dropPrefab != null)
            {
                DropSpawner.Spawn(_state.PendingOutput, 1, transform.position, _dropPrefab);
                _state.Clear();
            }

            if (_data != null && _dropPrefab != null)
                DropSpawner.Spawn(_data, 1, transform.position, _dropPrefab);

            if (SceneDataManager.Instance != null)
                SceneDataManager.Instance.ProcessingStates.Remove(CellPos);
        }

        private void Update()
        {
            if (!_state.IsProcessing || _state.Recipe == null || TimeManager.Instance == null) return;

            int elapsed = TimeManager.Instance.TotalMinutes - _state.StartMinute;
            if (elapsed >= _state.Recipe.ProcessingMinutes)
                CompleteCycle();
        }

        private void CompleteCycle()
        {
            _state.PendingOutput = _state.Recipe.OutputItem;
            _state.IsProcessing = false;
            _state.Recipe = null;

            SetAnimation(false); // 가공이 끝나면 애니메이션 정지

            string machineName = _data != null ? _data._name : "식품가공기";
            Debug.Log($"[{machineName}] 가공 완료! 기계를 클릭해 수령하세요.");
        }

        private void DropOutput(string machineName)
        {
            if (_state.PendingOutput == null) return;

            if (_dropPrefab == null)
            {
                Debug.LogError($"[{machineName}] Drop Prefab이 Inspector에 연결되지 않았습니다.");
                return;
            }

            Vector3 dropPos = transform.position + new Vector3(0f, -0.3f, 0f);
            DropSpawner.Spawn(_state.PendingOutput, 1, dropPos, _dropPrefab);
            Debug.Log($"[{machineName}] {_state.PendingOutput._name} 수확 완료!");

            // 데이터 장부 완전 초기화
            _state.Clear();

            // ── 🛠️ 해결의 초강수: 애니메이션 상태 자체를 완전히 껐다 켜서 초기화합니다 ──
            ForceResetAnimation();
        }

        private ProcessingRecipeData FindMatchingRecipe(ItemBaseData item)
            => _recipes.Find(r => r != null && r.InputItem != null && r.InputItem == item);

        private void SetAnimation(bool isWorking)
        {
            if (_animator != null && _animator.enabled) // enabled 체크 추가
                _animator.SetBool(AnimIsWorking, isWorking);
        }

        // ── 🚫 애니메이션 컨트롤러를 강제로 리셋해 '동작 중' 상태를 죽여버립니다 ──
        private void ForceResetAnimation()
        {
            if (_animator != null)
            {
                SetAnimation(false); // 일단 false로 세팅

                // 전원을 잠시 끄는 마법의 주문
                _animator.enabled = false;
                _animator.enabled = true; // 다시 켜지면 bool이 false라 자연스레 Idle 상태가 됨
            }
        }

        public override void OnDayPassed() { }
    }

    public class ProcessingState
    {
        public ProcessingRecipeData Recipe;
        public bool IsProcessing;
        public int StartMinute;
        public ItemBaseData PendingOutput;

        public void Clear()
        {
            Recipe = null;
            IsProcessing = false;
            StartMinute = 0;
            PendingOutput = null;
        }
    }
}