using MLT.World;

namespace MLT.Player
{
    /// <summary>
    /// 활 조준 스테이트.
    /// ToolUseState와 동일한 Enter/Tick/Exit 패턴.
    /// Enter → AimSystem 시작
    /// Tick  → 결과 대기 후 IdleState 복귀
    /// Exit  → 애니메이션 정리
    /// </summary>
    public class AimState : IPlayerState
    {
        private readonly Crow _targetCrow;
        private bool _finished;
        private PlayerController _player;

        public AimState(Crow targetCrow)
        {
            _targetCrow = targetCrow;
        }

        public void Enter(PlayerController player)
        {
            _player = player;
            _finished = false;

            if (AimSystem.Instance == null)
            {
                _finished = true;
                return;
            }

            AimSystem.Instance.OnAimResult += HandleAimResult;
            AimSystem.Instance.StartAiming(_targetCrow, player.Animator, player.Data.Inventory.GetCurrentToolData());
        }

        public void Tick(PlayerController player)
        {
            if (_finished)
                player.ChangeState(new IdleState());
        }

        public void Exit(PlayerController player)
        {
            if (AimSystem.Instance != null)
                AimSystem.Instance.OnAimResult -= HandleAimResult;
        }

        private void HandleAimResult(bool isHit)
        {
            if (_finished) return;
            _finished = true;
        }
    }
}