using MLT.Core;
using MLT.Data.ItemSO;
using MLT.Player;
using UnityEngine;

namespace MLT.Machine
{
    public class CookingStove : InstallableObject
    {
        private Animator _animator;
        private static readonly int AnimIsWorking = Animator.StringToHash("IsWorking");

        // 설치 직후 OnInteract가 즉시 호출되는 버그 방지용 플래그
        private bool _justPlaced = true;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            _justPlaced = false;
            EventBus.OnCookingStarted += HandleCookingStarted;
            EventBus.OnCookingCompleted += HandleCookingCompleted;
            EventBus.OnCookingStopped += HandleCookingStopped;

            
            // 씬에 들어왔을 때, 매니저가 이미 요리 중이라면 애니메이션을 스스로 켭니다.
            if (CookingManager.Instance != null && CookingManager.Instance.IsProcessing)
            {
                SetAnimation(true);
            }
        }

        private void OnDestroy()
        {
            EventBus.OnCookingStarted -= HandleCookingStarted;
            EventBus.OnCookingCompleted -= HandleCookingCompleted;
            EventBus.OnCookingStopped -= HandleCookingStopped; // ← 추가
        }

        private void HandleCookingStarted() => SetAnimation(true);

        public override void OnInteract(PlayerController player)
        {
            // 설치 직후 프레임 클릭 무시
            if (_justPlaced)
            {
                _justPlaced = false;
                return;
            }
            EventBus.RaiseSFX(SFXType.DIALOGUE_OPEN);
            EventBus.RaiseOpenCookingUI();
        }

        public override void OnDayPassed() { }

        // ── 애니메이션 ─────────────────────────────────────────────
        private void HandleCookingCompleted(ItemBaseData item, int amount)
        {
            // 완료 시에는 건드리지 않음 - Stopped에서 처리
        }

        private void HandleCookingStopped() => SetAnimation(false);

        // CookingUI의 OnClickCookButton에서 호출 가능하도록 public
        public void SetAnimation(bool isWorking)
        {
            Debug.Log($"[CookingStove] SetAnimation({isWorking}) - animator null: {_animator == null}");
            if (_animator != null)
                _animator.SetBool(AnimIsWorking, isWorking);
        }
    }
}