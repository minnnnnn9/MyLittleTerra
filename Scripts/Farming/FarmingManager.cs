using MLT.Core;
using MLT.Data;
using MLT.Data.ItemSO;
using MLT.Machine;
using MLT.World;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace MLT.Farm
{
    public class FarmingManager : MonoBehaviour
    {
        public static FarmingManager Instance { get; private set; }

        [Header("타일맵")]
        [SerializeField] private Tilemap _baseTilemap;
        [SerializeField] private Tilemap _cropTilemap;

        [Header("RuleTile (오토타일)")]
        [SerializeField] private RuleTile _tilledRuleTile;
        [SerializeField] private RuleTile _wateredRuleTile;

        [Header("드롭")]
        [SerializeField] private GameObject _dropPrefab;

        private Dictionary<Vector3Int, FarmTile> _tiles => MLT.Core.SceneDataManager.Instance.FarmTiles;
        private Dictionary<Sprite, Tile> _tileCache = new();
        private HashSet<Vector3Int> _activeTiles = new();
        private int _lastCheckedHour = -1;

        [Header(" 실시간 자료구조 데이터 모니터링")]
        [SerializeField] private List<string> _debugTilesLiveView = new();
        [SerializeField] private List<string> _debugActiveTilesLiveView = new();

        private void Update()
        {
            // 매 프레임 딕셔너리와 HashSet의 내부를 글자로 변환하여 리스트에 동기화
            if (_tiles != null)
            {
                _debugTilesLiveView.Clear();
                foreach (var kvp in _tiles)
                {
                    _debugTilesLiveView.Add($"[타일 좌표: {kvp.Key}] 상태: {kvp.Value.State} | 물 공급: {kvp.Value.IsWatered}");
                }
            }

            if (_activeTiles != null)
            {
                _debugActiveTilesLiveView.Clear();
                foreach (var pos in _activeTiles)
                {
                    _debugActiveTilesLiveView.Add($"[활성 연산중] {pos}");
                }
            }
        }

        private Func<ItemSeedData, bool> _checkCanRemoveSeed;
        public void SetSeedChecker(Func<ItemSeedData, bool> checker) => _checkCanRemoveSeed = checker;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (SceneTilemapProvider.Instance != null) BindTilemaps();
        }

        private void OnEnable()
        {
            SceneTilemapProvider.OnProviderReady += BindTilemaps;
        }

        private void OnDisable()
        {
            SceneTilemapProvider.OnProviderReady -= BindTilemaps;
        }

        private void BindTilemaps()
        {
            _baseTilemap = SceneTilemapProvider.Instance.BaseTilemap;
            _cropTilemap = SceneTilemapProvider.Instance.ObjectTilemap;
        }

        private void Start()
        {
            if (TimeManager.Instance == null) return;
            TimeManager.Instance.OnMinuteChanged += HandleMinuteChanged;
            TimeManager.Instance.OnDayStarted += HandleDayStarted;
            TimeManager.Instance.OnSeasonChanged += HandleSeasonChanged;

            RestoreFarmTiles();
        }

        private void OnDestroy()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnMinuteChanged -= HandleMinuteChanged;
                TimeManager.Instance.OnDayStarted -= HandleDayStarted;
                TimeManager.Instance.OnSeasonChanged -= HandleSeasonChanged;
            }
            foreach (var tile in _tileCache.Values)
                if (tile != null) Destroy(tile);
            _tileCache.Clear();
        }

        private bool HasMachineAt(Vector3Int cellPos)
        {
            if (WorldObjectManager.Instance == null) return false;
            return WorldObjectManager.Instance.GetObject(cellPos) != null;
        }

        public bool HasActiveFarmTileAt(Vector3Int cellPos)
        {
            if (!_tiles.TryGetValue(cellPos, out var tile)) return false;
            return tile.State != FarmState.EMPTY;
        }

        public void TillOrRestoreTile(Vector3Int cellPos)
        {
            if (HasMachineAt(cellPos)) return;
            if (!IsAbleToFarm(cellPos)) return;

            FarmTile tile = GetOrCreateTile(cellPos);
            tile.Till();
            if (TimeManager.Instance != null && TimeManager.Instance.IsRainyDay)
                tile.Water();
        }

        public void ExecuteAction(ToolType tool, Vector3Int cellPos,
                                  ItemSeedData seed = null, FertilizerData fertilizer = null)
        {
            if (HasMachineAt(cellPos)) return;

            if (tool == ToolType.HOE)
            {
                TillOrRestoreTile(cellPos);
                Debug.Log($"[흙 갈기] {cellPos}");
                return;
            }

            if (!_tiles.TryGetValue(cellPos, out var tile)) return;

            switch (tool)
            {
                case ToolType.WATERING_CAN:
                    tile.Water();
                    EventBus.RaiseSFX(SFXType.WATERING);
                    break;
                case ToolType.SEED:
                    TrySow(tile, seed);
                    break;
                case ToolType.FERTILIZER:
                    if (fertilizer != null) tile.Fertilize(fertilizer);
                    break;
            }
        }

        public ItemCropData HarvestTile(Vector3Int cellPos)
        {
            if (!_tiles.TryGetValue(cellPos, out var tile))
            {
                Debug.Log($"[수확] 타일 없음 {cellPos}");
                return null;
            }

            var item = tile.Harvest();
            if (item != null)
            {
                Debug.Log($"[수확 성공] {item._name}");
                Vector3 worldPos = _baseTilemap.GetCellCenterWorld(cellPos);
                DropSpawner.Spawn(item, 1, worldPos, _dropPrefab);
                EventBus.RaiseSFX(SFXType.HARVEST);
            }
            else
            {
                Debug.Log($"[수확 실패] 현재 상태: {tile.State}");
            }
            return item;
        }

        private void TrySow(FarmTile tile, ItemSeedData seed)
        {
            if (seed == null) return;
            if (tile.State != FarmState.TILLED) return;
            if (_checkCanRemoveSeed != null && !_checkCanRemoveSeed(seed))
            {
                Debug.Log($"[씨앗 부족] {seed._name}");
                return;
            }
            tile.Sow(seed, TimeManager.Instance.TotalMinutes);
            if (tile.CropData != null)
            {
                EventBus.RaiseSeedUsed(seed);
                EventBus.RaiseSFX(SFXType.SEED_PLANT);
            }
        }

        private bool IsAbleToFarm(Vector3Int cellPos)
        {
            TileBaseData tileData = _baseTilemap.GetTile<TileBaseData>(cellPos);
            return tileData != null && tileData.IsAbleToFarm;
        }

        private void HandleMinuteChanged()
        {
            int currentHour = TimeManager.Instance.Hour;
            if (currentHour == _lastCheckedHour) return;
            _lastCheckedHour = currentHour;

            bool isRaining = TimeManager.Instance.IsRainyDay;
            int now = TimeManager.Instance.TotalMinutes;

            foreach (var pos in _activeTiles)
            {
                if (!_tiles.TryGetValue(pos, out var tile)) continue;
                tile.ProcessTimePass(now, isRaining);

                // 크롭 있으면 매 시간 스프라이트 갱신
                if (tile.CropData != null)
                    RefreshVisual(pos, tile);
            }
        }

        private void HandleDayStarted()
        {
            _lastCheckedHour = -1;
            bool isRaining = TimeManager.Instance.IsRainyDay;

            foreach (var pos in _activeTiles.ToList()) // ToList() 유지
            {
                if (_tiles.TryGetValue(pos, out var tile))
                    tile.ProcessDayEnd(isRaining);
            }
        }

        private void HandleSeasonChanged(Season newSeason)
        {
            foreach (var (pos, tile) in _tiles)
            {
                tile.KillCrop();
                RefreshVisual(pos, tile);
            }
        }

        private FarmTile GetOrCreateTile(Vector3Int pos)
        {
            if (!_tiles.TryGetValue(pos, out var tile))
            {
                tile = new FarmTile();
                _tiles[pos] = tile;
                tile.OnTileChanged += () => RefreshVisual(pos, tile);
            }
            _activeTiles.Add(pos);
            return tile;
        }

        private void RefreshVisual(Vector3Int pos, FarmTile tile)
        {
            if (tile.State == FarmState.EMPTY)
                _activeTiles.Remove(pos);
            else
                _activeTiles.Add(pos);

            switch (tile.State)
            {
                case FarmState.EMPTY:
                    _baseTilemap.SetTile(pos, null);
                    _cropTilemap.SetTile(pos, null);
                    break;

                default:
                    TileBase soilTile = tile.IsWatered ? _wateredRuleTile : _tilledRuleTile;
                    _baseTilemap.SetTile(pos, soilTile);

                    if (tile.CropData != null)
                    {
                        // WateredMinutes 기준으로 스프라이트 결정
                        Sprite sprite = tile.CropData.GetGrowthSprite(tile.WateredMinutes);
                        _cropTilemap.SetTile(pos, GetOrCreateCropTile(sprite));
                    }
                    else
                    {
                        _cropTilemap.SetTile(pos, null);
                    }
                    break;
            }
        }

        private Tile GetOrCreateCropTile(Sprite sprite)
        {
            if (sprite == null) return null;
            if (!_tileCache.TryGetValue(sprite, out var tile))
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprite;
                _tileCache[sprite] = tile;
            }
            return tile;
        }

        public int GetCropTileCount() => GetCropTilePositions().Count;

        public List<Vector3Int> GetCropTilePositions()
        {
            var result = new List<Vector3Int>();
            foreach (var (pos, tile) in _tiles)
                if (tile.State == FarmState.SEEDED ||
                    tile.State == FarmState.GROWING ||
                    tile.State == FarmState.HARVESTABLE)
                    result.Add(pos);
            return result;
        }

        public void KillCropAt(Vector3Int cellPos)
        {
            if (!_tiles.TryGetValue(cellPos, out var tile)) return;
            tile.KillCrop();
            RefreshVisual(cellPos, tile);
        }

        /// <summary>
        /// 타일 상태 조회 (없으면 EMPTY 반환)
        /// TileHighlightUI_New에서 도구별 상호작용 가능 여부 판단에 사용
        /// </summary>
        public FarmState GetTileState(Vector3Int cellPos)
        {
            if (_tiles.TryGetValue(cellPos, out var tile))
                return tile.State;
            return FarmState.EMPTY;
        }

        /// <summary>
        /// 해당 위치가 농사 가능한 땅인지 (TileBaseData.IsAbleToFarm)
        /// TileHighlightUI_New에서 HOE 노란색 판단에 사용
        /// </summary>
        public bool IsAbleToFarmPublic(Vector3Int cellPos)
        {
            // 기존 private IsAbleToFarm을 public으로 노출
            TileBaseData tileData = _baseTilemap.GetTile<TileBaseData>(cellPos);
            return tileData != null && tileData.IsAbleToFarm;
        }

        private void RestoreFarmTiles()
        {
            _activeTiles.Clear(); // 활성화된 타일 목록 초기화

            if (MLT.Core.SceneDataManager.Instance == null) return;

            foreach (var kvp in _tiles)
            {
                Vector3Int pos = kvp.Key;
                FarmTile tile = kvp.Value;

                if (tile.State != FarmState.EMPTY)
                {
                    _activeTiles.Add(pos);

                    // 핵심: 이전 씬에서 연결됐던 유령 이벤트를 자르고, 새로 태어난 매니저와 연결!
                    tile.ClearEvents();
                    tile.OnTileChanged += () => RefreshVisual(pos, tile);

                    // 타일맵에 즉시 작물/흙 스프라이트 그리기
                    RefreshVisual(pos, tile);
                }
            }
        }
    }
}