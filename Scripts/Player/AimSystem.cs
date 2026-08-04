using MLT.Data.ItemSO;
using MLT.World;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MLT.Player
{
    public class AimSystem : MonoBehaviour
    {
        public static AimSystem Instance { get; private set; }

        [Header("조준 설정")]
        [SerializeField] private float _swingSpeed = 80f;
        [SerializeField] private float _swingRange = 45f;
        [SerializeField] private float _hitAngleThreshold = 12f;
        [SerializeField] private LayerMask _crowLayer;

        [Header("화살")]
        [SerializeField] private GameObject _arrowPrefab; // Arrow 컴포넌트 붙은 프리팹

        [Header("UI")]
        [SerializeField] private AimUI _aimUI;

        // 발사 애니메이션 끝난 뒤 화살 생성하도록 콜백 방식으로 변경
        private PlayerAnimator _playerAnimator;

        private ItemToolData _bowItem;

        // AimState가 구독. true=명중(화살 발사됨), false=빗나감/취소
        public event Action<bool> OnAimResult;

        private bool _isAiming;
        private float _currentAngle;
        private float _swingDir = 1f;
        private Crow _targetCrow;
        private float _aimStartTime;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void Update()
        {
            if (!_isAiming) return;

            _currentAngle += _swingSpeed * _swingDir * Time.deltaTime;
            if (Mathf.Abs(_currentAngle) >= _swingRange)
                _swingDir *= -1f;

            _aimUI?.UpdateAim(_currentAngle, _swingRange, _hitAngleThreshold);

            if (Time.time - _aimStartTime < 0.2f) return;

            bool shoot = (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                      || (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame);
            bool cancel = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;

            if (shoot) TryShoot();
            else if (cancel) CancelAim();
        }

        public void StartAiming(Crow target, PlayerAnimator animator, ItemToolData bowItem)
        {
            if (_isAiming) return;
            _targetCrow = target;
            _playerAnimator = animator; // ← 추가
            _bowItem = bowItem;
            _currentAngle = UnityEngine.Random.Range(-_swingRange * 0.7f, _swingRange * 0.7f);
            _swingDir = UnityEngine.Random.value > 0.5f ? 1f : -1f;
            _isAiming = true;
            _aimUI?.Show(_hitAngleThreshold, _swingRange);

            _aimStartTime = Time.time;

            _aimUI?.Show(_hitAngleThreshold, _swingRange);
        }

        public void CancelAim()
        {
            _isAiming = false;
            _targetCrow = null;
            _aimUI?.Hide();
            OnAimResult?.Invoke(false);
        }

        /// <summary>
        /// IdleState에서 마우스 위치 까마귀 탐지
        /// </summary>
        public Crow FindCrowAtMouse(Camera cam)
        {
            if (cam == null || Mouse.current == null) return null;
            Vector2 screen = Mouse.current.position.ReadValue();
            Vector3 world = cam.ScreenToWorldPoint(screen);
            world.z = 0f;
            Collider2D hit = Physics2D.OverlapPoint(world, _crowLayer);
            return hit != null ? hit.GetComponent<Crow>() : null;
        }

        private void TryShoot()
        {
            _isAiming = false;
            _aimUI?.Hide();

            float normalizedPos = Mathf.Abs(_currentAngle) / _swingRange;
            float hitRatio = _hitAngleThreshold / _swingRange;
            bool isHit = normalizedPos <= hitRatio;

            if (isHit && _targetCrow != null)
            {
                // 명중 → 애니메이션 후 화살 발사 → 까마귀 맞음
                if (_playerAnimator != null && _bowItem != null)
                {
                    Crow crowRef = _targetCrow;
                    void OnShootAnimFinished()
                    {
                        _playerAnimator.OnToolUseAnimationFinished -= OnShootAnimFinished;
                        LaunchArrow(crowRef, hit: true);
                    }
                    _playerAnimator.OnToolUseAnimationFinished += OnShootAnimFinished;
                    _playerAnimator.PlayToolUse(_bowItem);
                }
                else
                {
                    LaunchArrow(_targetCrow, hit: true);
                }
                Debug.Log($"[활] 발사! 각도={_currentAngle:F1}°");
            }
            else if (_targetCrow != null)
            {
                // 빗나감 → 화살은 날아가지만 까마귀 안 맞음
                if (_playerAnimator != null && _bowItem != null)
                {
                    Crow crowRef = _targetCrow;
                    void OnShootAnimFinished()
                    {
                        _playerAnimator.OnToolUseAnimationFinished -= OnShootAnimFinished;
                        LaunchArrow(crowRef, hit: false);
                    }
                    _playerAnimator.OnToolUseAnimationFinished += OnShootAnimFinished;
                    _playerAnimator.PlayToolUse(_bowItem);
                }
                else
                {
                    LaunchArrow(_targetCrow, hit: false);
                }
                Debug.Log($"[활] 빗나감. 각도={_currentAngle:F1}°");
            }

            _targetCrow = null;
            OnAimResult?.Invoke(isHit);
        }

        private void LaunchArrow(Crow target, bool hit)
        {
            if (_arrowPrefab == null)
            {
                if (hit) target.OnHitByArrow();
                else target.OnMissedArrow();
                return;
            }

            Vector3 spawnPos = transform.position;
            GameObject arrowObj = Instantiate(_arrowPrefab, spawnPos, Quaternion.identity);
            Arrow arrow = arrowObj.GetComponent<Arrow>();
            arrow?.Launch(target, hit); // hit 여부 넘김
        }
    }
}