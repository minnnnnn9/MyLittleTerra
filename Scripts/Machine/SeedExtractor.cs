using MLT.Core;
using MLT.Data.ItemSO;
using MLT.Player;
using MLT.World;
using UnityEngine;

namespace MLT.Machine
{
    public class SeedExtractor : InstallableObject
    {
        [SerializeField] private ExtractorUI _extractorUI;
        [SerializeField] private ItemPlaceableData _data;

        private Animator _animator;
        private static readonly int AnimIsWorking = Animator.StringToHash("IsWorking");


        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            if (_extractorUI == null)
                _extractorUI = FindFirstObjectByType<ExtractorUI>(FindObjectsInactive.Include);

            // 사이클 완료 이벤트 구독 → UI 갱신 + 애니메이션 상태 갱신
            EventBus.OnExtractorCycleCompleted += HandleCycleCompleted;
        }

        private void OnDestroy()
        {
            EventBus.OnExtractorCycleCompleted -= HandleCycleCompleted;
        }

        public override void OnInteract(PlayerController player)
        {
            Debug.Log("[SeedExtractor] OnInteract 호출됨");
            if (_extractorUI == null)
            {
                Debug.LogWarning("[SeedExtractor] ExtractorUI 참조 없음");
                return;
            }
            EventBus.RaiseSFX(SFXType.DIALOGUE_OPEN);
            _extractorUI.OpenUI(this.CellPos);
        }

        public override void OnDayPassed() { }

        // ── 애니메이션 ─────────────────────────────────────────────
        // SeedExtractorManager에서 상태 변화 시 호출할 수 있도록 public으로 열어둠
        public void SetWorking(bool isWorking)
        {
            if (_animator != null)
                _animator.SetBool(AnimIsWorking, isWorking);
        }

        public override void OnUninstall()
        {
            var state = SeedExtractorManager.Instance?.GetState(this.CellPos);
            if (state != null && _dropPrefab != null)
            {
                // InputQueue에 남은 작물 드롭
                while (state.InputQueue.Count > 0)
                {
                    var crop = state.InputQueue.Dequeue();
                    DropSpawner.Spawn(crop, 1, transform.position, _dropPrefab);
                }

                // OutputQueue에 완성된 씨앗 드롭
                while (state.OutputQueue.Count > 0)
                {
                    var output = state.OutputQueue.Dequeue();
                    DropSpawner.Spawn(output.Seed, output.Amount, transform.position, _dropPrefab);
                }
            }

            // 기계 아이템 드롭
            if (_data != null && _dropPrefab != null)
                DropSpawner.Spawn(_data, 1, transform.position, _dropPrefab);
        }

        private void HandleCycleCompleted(Vector3Int pos)
        {
            if (pos != this.CellPos) return;

            var state = SeedExtractorManager.Instance.GetState(pos);
            if (state == null) return;

            // 큐가 비면 idle, 남아있으면 계속 working
            SetWorking(state.IsProcessing);
        }

        private void Update()
        {
            // 작동 시작 감지 - InsertCrops 호출 후 IsProcessing이 true가 되는 순간 애니메이션 ON
            var state = SeedExtractorManager.Instance?.GetState(this.CellPos);
            if (state == null) return;

            bool shouldWork = state.IsProcessing;
            if (_animator != null && _animator.GetBool(AnimIsWorking) != shouldWork)
                _animator.SetBool(AnimIsWorking, shouldWork);
        }
    }
}