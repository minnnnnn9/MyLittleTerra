using MLT.Core;
using MLT.Data;
using MLT.Machine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MLT.World
{
    /// <summary>
    /// 마을/해변 씬 전용 타일 제공자.
    /// 모래사장 타일맵에서 비어있는 타일을 반환.
    ///
    /// 팀원 요청 사항:
    ///   - _beachTilemap에 해변 모래사장 전용 타일맵을 Inspector에서 연결해 주세요.
    ///   - 모래사장 타일에 쓰이는 TileBaseData의 IsBeachTile 플래그를 true로 설정하거나,
    ///     별도 태그/레이어로 구분하는 방식으로 팀 내 규칙을 맞춰 주세요.
    ///     (현재는 타일맵에 있는 모든 타일을 후보로 사용)
    /// </summary>
    public class BeachArtifactTileProvider : MonoBehaviour, IArtifactTileProvider
    {
        [SerializeField] private Tilemap _beachTilemap;

        public ArtifactZoneType ZoneType => ArtifactZoneType.Beach;

        public List<Vector3Int> GetSpawnableCells()
        {
            var result = new List<Vector3Int>();
            if (_beachTilemap == null) return result;

            BoundsInt bounds = _beachTilemap.cellBounds;
            foreach (var pos in bounds.allPositionsWithin)
            {
                if (_beachTilemap.GetTile(pos) == null) continue;

                var tileData = _beachTilemap.GetTile<TileBaseData>(pos);
                if (tileData == null || !tileData.IsAbleToSpawnArtifact) continue;

                // 기계가 있는 타일 제외 (해변에 기계를 설치하는 경우 대비)
                if (WorldObjectManager.Instance != null &&
                    WorldObjectManager.Instance.GetObject(pos) != null) continue;

                result.Add(pos);
            }
            return result;
        }
    }
}