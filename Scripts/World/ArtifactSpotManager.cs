using MLT.Core;
using MLT.Data;
using MLT.Data.ItemSO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MLT.World
{
    public class ArtifactSpotManager : MonoBehaviour
    {
        public static ArtifactSpotManager Instance { get; private set; }

        [Header("프리팹")]
        [SerializeField] private GameObject _artifactSpotPrefab;
        [SerializeField] private GameObject _droppedItemPrefab;

        [Header("SO 설정")]
        [SerializeField] private ArtifactSpotConfig _config;
        [SerializeField] private ArtifactRewardTable _rewardTable;

        [Header("참조")]
        [SerializeField] private Tilemap _referenceTilemap;
        [SerializeField] private Transform _spotParent;

        private readonly Dictionary<Vector3Int, ArtifactSpot> _activeSpots = new();
        private readonly Dictionary<ArtifactZoneType, int> _zoneCounts = new()
        {
            { ArtifactZoneType.Farm,  0 },
            { ArtifactZoneType.Beach, 0 }
        };

        private readonly List<IArtifactTileProvider> _providers = new();

        // ── 💰 [핵심 수정] 구역(Zone)별로 쪼개진 금고 장부를 정확하게 꺼내오는 함수 ──
        private Dictionary<Vector3Int, ArtifactSpotState> GetVaultForZone(ArtifactZoneType zone)
        {
            if (SceneDataManager.Instance != null && SceneDataManager.Instance.ArtifactSpots.ContainsKey(zone))
                return SceneDataManager.Instance.ArtifactSpots[zone];

            return new Dictionary<Vector3Int, ArtifactSpotState>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            // 현재 씬에 있는 프로바이더(농장이면 Farm, 마을이면 Beach)만 긁어옵니다.
            foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (mb is IArtifactTileProvider p)
                    _providers.Add(p);
            }
        }

        private void Start()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnDayStarted += HandleDayStarted;
                int today = TimeManager.Instance.Day;

                RestoreFromVault(today);

                if (today >= _config.SpawnStartDay)
                {
                    SpawnAll(today);
                }
            }
        }

        private void OnDestroy()
        {
            if (TimeManager.Instance != null)
                TimeManager.Instance.OnDayStarted -= HandleDayStarted;
        }

        private void RestoreFromVault(int today)
        {
            _activeSpots.Clear();
            _zoneCounts[ArtifactZoneType.Farm] = 0;
            _zoneCounts[ArtifactZoneType.Beach] = 0;

            foreach (var provider in _providers)
            {
                var zone = provider.ZoneType;
                var vaultSpots = GetVaultForZone(zone); // 여기서 자기 구역 장부만 쏙 빼옵니다
                var toRemove = new List<Vector3Int>();

                foreach (var kvp in vaultSpots)
                {
                    var pos = kvp.Key;
                    var state = kvp.Value;

                    if (today - state.SpawnDay >= state.DespawnAfterDays)
                    {
                        toRemove.Add(pos);
                        continue;
                    }

                    RestoreSpotPrefab(pos, state);
                }

                foreach (var pos in toRemove)
                {
                    vaultSpots.Remove(pos);
                }
            }
        }

        private void RestoreSpotPrefab(Vector3Int cellPos, ArtifactSpotState state)
        {
            if (_artifactSpotPrefab == null || _referenceTilemap == null) return;

            Vector3 worldPos = _referenceTilemap.GetCellCenterWorld(cellPos);
            worldPos.z = 0f;

            var parent = _spotParent != null ? _spotParent : transform;
            var go = Instantiate(_artifactSpotPrefab, worldPos, Quaternion.identity, parent);

            var spot = go.GetComponent<ArtifactSpot>();
            if (spot == null) { Destroy(go); return; }

            spot.Setup(cellPos, state.ZoneType, state.SpawnDay, state.DespawnAfterDays);

            _activeSpots[cellPos] = spot;
            _zoneCounts[state.ZoneType]++;
        }

        private void HandleDayStarted()
        {
            int today = TimeManager.Instance.Day;
            DespawnExpired(today);
            SpawnAll(today);
        }

        private void DespawnExpired(int today)
        {
            foreach (var provider in _providers)
            {
                var zone = provider.ZoneType;
                var vaultSpots = GetVaultForZone(zone);
                var toRemove = new List<Vector3Int>();

                foreach (var kvp in vaultSpots)
                {
                    if (today - kvp.Value.SpawnDay >= kvp.Value.DespawnAfterDays)
                        toRemove.Add(kvp.Key);
                }

                foreach (var pos in toRemove)
                {
                    RemoveSpot(pos, zone, destroyGO: true);
                }
            }
        }

        private void SpawnAll(int today)
        {
            foreach (var provider in _providers)
                SpawnForProvider(provider, today);
        }

        private void SpawnForProvider(IArtifactTileProvider provider, int today)
        {
            var zone = provider.ZoneType;
            int maxSpots = zone == ArtifactZoneType.Farm ? _config.MaxFarmSpots : _config.MaxBeachSpots;
            float chance = zone == ArtifactZoneType.Farm ? _config.FarmSpawnChancePerTile : _config.BeachSpawnChancePerTile;

            if (_zoneCounts[zone] >= maxSpots) return;

            var vaultSpots = GetVaultForZone(zone);
            var candidates = provider.GetSpawnableCells();
            Shuffle(candidates);

            foreach (var pos in candidates)
            {
                if (_zoneCounts[zone] >= maxSpots) break;
                if (vaultSpots.ContainsKey(pos)) continue;
                if (Random.value > chance) continue;

                SpawnSpot(pos, zone, today, vaultSpots);
            }
        }

        private void SpawnSpot(Vector3Int cellPos, ArtifactZoneType zone, int today, Dictionary<Vector3Int, ArtifactSpotState> vaultSpots)
        {
            var state = new ArtifactSpotState
            {
                ZoneType = zone,
                SpawnDay = today,
                DespawnAfterDays = _config.DespawnAfterDays
            };
            vaultSpots[cellPos] = state;

            RestoreSpotPrefab(cellPos, state);
            Debug.Log($"[ArtifactSpot] 스폰: {cellPos} / {zone}");
        }

        public ArtifactSpot GetSpotAt(Vector3Int pos)
        {
            _activeSpots.TryGetValue(pos, out var spot);
            return spot;
        }

        public void OnArtifactDug(Vector3Int cellPos, Vector3 worldPos, ArtifactZoneType zone)
        {
            var vaultSpots = GetVaultForZone(zone);
            if (!vaultSpots.ContainsKey(cellPos)) return;

            var (item, amount) = _rewardTable.Roll(zone);
            if (item != null)
            {
                SpawnDroppedItem(item, amount, worldPos);
                EventBus.RaiseSFX(SFXType.ARTIFACT_DIG);
            }

            RemoveSpot(cellPos, zone, destroyGO: true);
        }

        private void SpawnDroppedItem(ItemBaseData item, int amount, Vector3 spawnPos)
        {
            if (_droppedItemPrefab == null) return;

            var go = Instantiate(_droppedItemPrefab, spawnPos, Quaternion.identity);
            var dropped = go.GetComponent<ItemDrop>();
            dropped?.Initialize(item, amount, _config.DropArcHeight, _config.DropDuration);
        }

        private void RemoveSpot(Vector3Int pos, ArtifactZoneType zone, bool destroyGO)
        {
            if (_activeSpots.TryGetValue(pos, out var spot))
            {
                _activeSpots.Remove(pos);
                if (destroyGO && spot != null) Destroy(spot.gameObject);
            }

            var vaultSpots = GetVaultForZone(zone);
            if (vaultSpots.ContainsKey(pos))
            {
                _zoneCounts[zone] = Mathf.Max(0, _zoneCounts[zone] - 1);
                vaultSpots.Remove(pos);
            }
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }

    public class ArtifactSpotState
    {
        public ArtifactZoneType ZoneType;
        public int SpawnDay;
        public int DespawnAfterDays;
    }
}