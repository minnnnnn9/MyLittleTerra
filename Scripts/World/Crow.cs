using MLT.Core;
using MLT.Farm;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MLT.World
{
    public class Crow : MonoBehaviour
    {
        [SerializeField] private float _fleeSpeed = 6f;
        [SerializeField] private float _playerTooCloseRadius = 0f;

        private Vector3Int _targetCell;
        private bool _isFleeing;
        private Tilemap _tilemap;
        private Transform _player;
        private Animator _animator;
        private Collider2D _hitCollider;

        private static readonly int AnimFlying = Animator.StringToHash("IsFlying");
        private static readonly int AnimEating = Animator.StringToHash("IsEating");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _hitCollider = GetComponent<Collider2D>();
        }

        private void Update()
        {
            if (_isFleeing)
            {
                Flee();
                return;
            }

            if (IsPlayerTooClose())
                StartFlee(eatCrop: false);
        }

        public void Initialize(Vector3Int targetCell, Tilemap tilemap, Transform player)
        {
            _targetCell = targetCell;
            _tilemap = tilemap;
            _player = player;

            transform.position = tilemap.GetCellCenterWorld(targetCell);

            SetAnim(AnimFlying, false);
            SetAnim(AnimEating, true);
        }

        // ── 외부 호출 ──────────────────────────────────────────

        /// <summary>화살 명중 시 AimSystem → Crow 호출</summary>
        public void OnHitByArrow()
        {
            if (_isFleeing) return;
            CrowManager.Instance?.UnregisterCrow(this);
            EventBus.RaiseSFX(SFXType.CROW_CRY_FAST);
            Destroy(gameObject);
        }

        /// <summary>화살 빗나감 시 호출 - 놀라는 연출</summary>
        public void OnMissedArrow()
        {
            if (_isFleeing) return;
            StartCoroutine(StarttledRoutine());
        }

        /// <summary>제한시간 종료 시 CrowManager 호출 - 작물 먹고 도주</summary>
        public void EatAndFlee()
        {
            if (_isFleeing) return;
            StartCoroutine(EatThenFleeRoutine());
        }

        // ── 내부 ───────────────────────────────────────────────

        private IEnumerator EatThenFleeRoutine()
        {
            yield return new WaitForSeconds(0.5f);
            FarmingManager.Instance?.KillCropAt(_targetCell);
            Debug.Log($"[까마귀] {_targetCell} 작물 파괴 후 도주");
            StartFlee(eatCrop: true);
        }

        private IEnumerator StarttledRoutine()
        {
            Vector3 origin = transform.position;
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                transform.position = origin + Vector3.up * Mathf.Sin(t * Mathf.PI / 0.3f) * 0.3f;
                yield return null;
            }
            transform.position = origin;
        }

        private void StartFlee(bool eatCrop)
        {
            if (_isFleeing) return;
            _isFleeing = true;
            StopAllCoroutines();
            SetAnim(AnimEating, false);
            SetAnim(AnimFlying, true);

            // 도주 중 피격 불가
            if (_hitCollider != null) _hitCollider.enabled = false;
        }

        private void Flee()
        {
            transform.position += Vector3.left * _fleeSpeed * Time.deltaTime;

            float dist = _player != null
                ? Vector3.Distance(transform.position, _player.position)
                : float.MaxValue;

            if (dist > 20f)
            {
                CrowManager.Instance?.UnregisterCrow(this);
                Destroy(gameObject);
            }
        }

        private bool IsPlayerTooClose()
        {
            if (_player == null) return false;
            return Vector3.Distance(transform.position, _player.position) < _playerTooCloseRadius;
        }

        private void SetAnim(int hash, bool value)
            => _animator?.SetBool(hash, value);
    }
}