using System.Collections;
using UnityEngine;
using MLT.Player;
using MLT.Data.ItemSO;
using MLT.Core;

namespace MLT.World
{
    public class ItemDrop : MonoBehaviour
    {
        [Header("표현")]
        [SerializeField] private SpriteRenderer _spriteRenderer;

        [Header("자석 효과")]
        [SerializeField] private float _magnetRadius = 1.5f;
        [SerializeField] private float _pickupRadius = 0.2f;
        [SerializeField] private float _magnetSpeed = 5f;

        [Header("획득 지연")]
        [SerializeField] private float _pickupDelay = 1f;

        [Header("포물선 연출")]
        [SerializeField] private float _arcHeight = 1.2f; // 호 높이
        [SerializeField] private float _arcDuration = 0.45f; // 비행 시간(초)

        private ItemBaseData _item;
        private int _amount;
        private PlayerController _player;
        private float _spawnTime;
        private bool _isFlying; // 포물선 중에는 자석/획득 차단

        private bool CanPickup => !_isFlying && Time.time >= _spawnTime + _pickupDelay;

        // ── 기존 Initialize (포물선 없이 그냥 스폰) ──────────────────────
        public void Initialize(ItemBaseData item, int amount)
        {
            _item = item;
            _amount = amount;
            _spawnTime = Time.time;

            if (_spriteRenderer != null && _item != null)
                _spriteRenderer.sprite = _item._icon;
        }

        // ── 포물선 연출 포함 Initialize (유물 지점 전용) ─────────────────
        public void Initialize(ItemBaseData item, int amount, float arcHeight, float arcDuration)
        {
            _arcHeight = arcHeight;
            _arcDuration = arcDuration;
            Initialize(item, amount);
            StartCoroutine(ArcRoutine());
        }

        private IEnumerator ArcRoutine()
        {
            _isFlying = true;

            Vector3 startPos = transform.position;
            Vector3 landPos = startPos + new Vector3(
                Random.Range(-0.4f, 0.4f),
                Random.Range(-0.2f, 0.2f), 0f);

            float elapsed = 0f;
            while (elapsed < _arcDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _arcDuration;
                float arc = Mathf.Sin(t * Mathf.PI) * _arcHeight;
                transform.position = Vector3.Lerp(startPos, landPos, t) + new Vector3(0f, arc, 0f);
                yield return null;
            }

            transform.position = landPos;
            _isFlying = false;
        }

        // ── 기존 로직 유지 ────────────────────────────────────────────────
        private void Start()
        {
            _player = FindFirstObjectByType<PlayerController>();
        }

        private void Update()
        {
            if (_player == null || _item == null || !CanPickup) return;

            float distance = Vector2.Distance(transform.position, _player.transform.position);

            if (distance <= _magnetRadius)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    _player.transform.position,
                    _magnetSpeed * Time.deltaTime);
            }

            if (distance <= _pickupRadius)
            {
                if (_player.Data.Inventory.TryAddItem(_item, _amount))
                {
                    EventBus.RaiseSFX(SFXType.ITEM_PICKUP);
                    Destroy(gameObject);
                }
            }
        }
    }
}