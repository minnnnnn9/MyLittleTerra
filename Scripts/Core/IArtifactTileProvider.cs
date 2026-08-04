using System.Collections.Generic;
using MLT.Data;
using UnityEngine;

namespace MLT.Core
{
    /// <summary>
    /// 씬마다 유물 지점을 스폰할 수 있는 타일 목록을 제공하는 인터페이스.
    ///
    /// 팀원 요청 사항:
    ///   - 농장 씬  → FarmArtifactTileProvider 구현체 작성 필요
    ///     (FarmingManager.GetFarmableTiles() 같은 형태로 경작 가능 + 비어있는 타일 목록 반환)
    ///   - 마을/해변 씬 → BeachArtifactTileProvider 구현체 작성 필요
    ///     (모래사장 타일맵에서 비어있는 타일 목록 반환)
    ///
    /// ArtifactSpotManager는 이 인터페이스에만 의존하므로, 씬이 달라져도 Manager 수정 불필요.
    /// </summary>
    public interface IArtifactTileProvider
    {
        ArtifactZoneType ZoneType { get; }

        /// <summary>유물 지점을 스폰 가능한 타일 좌표 목록을 반환.</summary>
        List<Vector3Int> GetSpawnableCells();
    }
}