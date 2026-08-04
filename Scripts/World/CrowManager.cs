using MLT.Core;
using MLT.Farm;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MLT.World
{
    public class CrowManager : MonoBehaviour
    {
        public static CrowManager Instance { get; private set; }

        [Header("참조")]
        [SerializeField] private Tilemap _referenceTilemap;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private GameObject _crowPrefab;

        [Header("스폰 설정")]
        [SerializeField] private int _minCrows = 3;
        [SerializeField] private int _maxCrows = 6;
        [SerializeField][Range(0f, 1f)] private float _spawnChance = 0.7f;

        [Header("제한시간 (초)")]
        [SerializeField] private float _huntingDuration = 30f;

        private readonly List<Crow> _activeCrows = new();
        private bool _huntingPhase;
        private Coroutine _timerCoroutine;

        // UI 연결 이벤트
        public event Action<int> OnCrowsSpawned;   // 스폰된 까마귀 수
        public event Action<float> OnTimerUpdated;   // 남은 시간(초)
        public event Action OnHuntingEnded;   // 사냥 종료 (성공 or 타임오버)

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            StartCoroutine(WaitAndSubscribe());
        }

        private IEnumerator WaitAndSubscribe()
        {
            while (MLT.TimeManager.Instance == null) yield return null;
            MLT.TimeManager.Instance.OnDayStarted += HandleDayStarted;
        }

        private void OnDestroy()
        {
            if (MLT.TimeManager.Instance != null)
                MLT.TimeManager.Instance.OnDayStarted -= HandleDayStarted;
        }

        private void HandleDayStarted() => TrySpawnCrows();

        private void TrySpawnCrows()
        {
            if (_crowPrefab == null || _referenceTilemap == null) return;
            if (FarmingManager.Instance == null) return;
            if (UnityEngine.Random.value > _spawnChance) return;

            var cropTiles = FarmingManager.Instance.GetCropTilePositions();
            if (cropTiles.Count == 0) return;

            Shuffle(cropTiles);
            int count = Mathf.Min(UnityEngine.Random.Range(_minCrows, _maxCrows + 1), cropTiles.Count);

            for (int i = 0; i < count; i++)
                SpawnCrow(cropTiles[i]);
            if (count > 0)
            {
                EventBus.RaiseSFX(SFXType.CROW_CRY);
            }

            Debug.Log($"[CrowManager] 까마귀 {count}마리 출몰");

            _huntingPhase = true;
            OnCrowsSpawned?.Invoke(count);

            if (_timerCoroutine != null) StopCoroutine(_timerCoroutine);
            _timerCoroutine = StartCoroutine(HuntingTimer());
        }

        private IEnumerator HuntingTimer()
        {
            float remaining = _huntingDuration;

            while (remaining > 0f)
            {
                remaining -= Time.deltaTime;
                OnTimerUpdated?.Invoke(Mathf.Max(0f, remaining));
                yield return null;
            }

            EndHunting();
        }

        private void EndHunting()
        {
            if (!_huntingPhase) return;
            _huntingPhase = false;
            OnHuntingEnded?.Invoke();

            // 남은 까마귀: 작물 먹고 도주
            var remaining = new List<Crow>(_activeCrows);
            foreach (var crow in remaining)
                crow.EatAndFlee();

            Debug.Log($"[CrowManager] 제한시간 종료. 잔여 {remaining.Count}마리 작물 파괴");
        }

        private void SpawnCrow(Vector3Int targetCell)
        {
            Vector3 worldPos = _referenceTilemap.GetCellCenterWorld(targetCell);
            var obj = UnityEngine.Object.Instantiate(_crowPrefab, worldPos, Quaternion.identity);
            var crow = obj.GetComponent<Crow>();
            if (crow == null) return;

            crow.Initialize(targetCell, _referenceTilemap, _playerTransform);
            _activeCrows.Add(crow);
        }

        public void UnregisterCrow(Crow crow)
        {
            _activeCrows.Remove(crow);

            // 전부 처치 → 즉시 성공 처리
            if (_huntingPhase && _activeCrows.Count == 0)
            {
                if (_timerCoroutine != null) StopCoroutine(_timerCoroutine);
                _huntingPhase = false;
                OnHuntingEnded?.Invoke();
                Debug.Log("[CrowManager] 모든 까마귀 처치 완료!");
            }
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}