using MLT.World;
using System.Collections;
using UnityEngine;

namespace MLT.Player
{
    /// <summary>
    /// 화살 오브젝트.
    /// AimSystem에서 isHit=true일 때 Instantiate되어
    /// 타겟 까마귀를 향해 날아감 → 도착 시 OnHitByArrow() 호출.
    /// </summary>
    public class Arrow : MonoBehaviour
    {
        [SerializeField] private float _speed = 15f;

        private Crow _target;
        private bool _arrived;
        private bool _isHit;
        private Vector3 _missTargetPos; // 빗나갈 목표 위치

        public void Launch(Crow target, bool isHit)
        {
            _target = target;
            _isHit = isHit;

            if (!isHit && target != null)
            {
                // 까마귀 위치 기준으로 살짝 빗나간 위치 계산
                Vector2 offset = UnityEngine.Random.insideUnitCircle.normalized * 1.5f;
                _missTargetPos = target.transform.position + new Vector3(offset.x, offset.y, 0f);
            }

            UpdateRotation();
            StartCoroutine(FlyRoutine());
        }

        private IEnumerator FlyRoutine()
        {
            while (!_arrived)
            {
                if (_target == null && _isHit) { Destroy(gameObject); yield break; }

                Vector3 targetPos = _isHit
                    ? _target.transform.position  // 명중: 까마귀 위치 추적
                    : _missTargetPos;             // 빗나감: 고정된 빗나간 위치로

                transform.position = Vector3.MoveTowards(
                    transform.position, targetPos, _speed * Time.deltaTime);

                UpdateRotation(targetPos);

                if (Vector3.Distance(transform.position, targetPos) < 0.1f)
                {
                    _arrived = true;
                    if (_isHit)
                        _target.OnHitByArrow();
                    else
                        _target?.OnMissedArrow(); // 근처 지나갈 때 놀라는 연출
                    Destroy(gameObject);
                }

                yield return null;
            }
        }

        private void UpdateRotation(Vector3? targetPos = null)
        {
            Vector3 pos = targetPos ?? (_isHit && _target != null
                ? _target.transform.position
                : _missTargetPos);

            Vector2 dir = (pos - transform.position).normalized;
            if (dir.sqrMagnitude > 0.001f)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle - 45f);
            }
        }
    }
}