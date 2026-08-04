using MLT.Core;
using MLT.Data;
using MLT.Farm;
using MLT.Machine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MLT.World
{
    /// <summary>
    /// 농장 씬 전용 타일 제공자.
    /// 경작 가능(IsAbleToFarm)하고 작물/기계가 없는 타일을 반환.
    ///
    /// 팀원 요청 사항:
    ///   - _baseTilemap에 농장 베이스 타일맵을 Inspector에서 연결해 주세요.
    ///   - TileBaseData.IsAbleToFarm 플래그가 올바르게 세팅되어 있어야 합니다.
    /// </summary>
    public class FarmArtifactTileProvider : MonoBehaviour, IArtifactTileProvider
    {
        [SerializeField] private Tilemap _baseTilemap;

        public ArtifactZoneType ZoneType => ArtifactZoneType.Farm;

        public List<Vector3Int> GetSpawnableCells()
        {
            var result = new List<Vector3Int>();
            if (_baseTilemap == null) return result;

            BoundsInt bounds = _baseTilemap.cellBounds;
            foreach (var pos in bounds.allPositionsWithin)
            {
                // 1. 경작 가능 타일인지
                var tileData = _baseTilemap.GetTile<TileBaseData>(pos);
                if (tileData == null || !tileData.IsAbleToFarm) continue;

                // 2. 이미 작물이 있는 타일인지
                if (FarmingManager.Instance != null &&
                    FarmingManager.Instance.HasActiveFarmTileAt(pos)) continue;

                // 3. 기계가 설치된 타일인지
                if (WorldObjectManager.Instance != null &&
                    WorldObjectManager.Instance.GetObject(pos) != null) continue;

                result.Add(pos);
            }
            return result;
        }
    }
}