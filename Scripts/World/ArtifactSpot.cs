using UnityEngine;
using MLT.Core;
using MLT.Data;
using MLT.Player;

namespace MLT.World
{
    /// <summary>
    /// 유물 지점 오브젝트 (지렁이).
    /// 타일 위에 배치되며, 플레이어가 괭이(HOE)로 상호작용 시 보상 드랍.
    /// 일정 일수가 지나면 자동 소멸(디스폰).
    /// </summary>
    public class ArtifactSpot : MonoBehaviour, IInteractable
    {
        // ── 설정 ─────────────────────────────────────────────────────────
        private Vector3Int _cellPos;
        private ArtifactZoneType _zoneType; // 어느 구역에서 스폰됐는지 (농장 / 해변)
        private int _spawnDay;              // 스폰된 날짜
        private int _despawnAfterDays = 3;  // 이 일수가 지나면 자동 소멸

        // ── 애니메이션 ────────────────────────────────────────────────────
        private Animator _animator;
        private static readonly int AnimWiggle = Animator.StringToHash("Wiggle");

        // ── 초기화 ────────────────────────────────────────────────────────
        public void Setup(Vector3Int cellPos, ArtifactZoneType zoneType, int currentDay, int despawnAfterDays)
        {
            _cellPos = cellPos;
            _zoneType = zoneType;
            _spawnDay = currentDay;
            _despawnAfterDays = despawnAfterDays;

            _animator = GetComponent<Animator>();
        }

        // ── IInteractable ─────────────────────────────────────────────────

        public bool CanInteract(PlayerController player)
        {
            // 괭이를 들고 있어야만 상호작용 가능
            return ToolManager.Instance != null &&
                   ToolManager.Instance.CurrentTool == ToolType.HOE;
        }

        public void Interact(PlayerController player)
        {
            // ArtifactSpotManager에 처리 위임 (보상 드랍 + 제거)
            ArtifactSpotManager.Instance?.OnArtifactDug(_cellPos, transform.position, _zoneType);
        }

        // ── 날짜 경과 체크 ───────────────────────────────────────────────

        /// <summary>ArtifactSpotManager가 매일 아침 호출. true 반환 시 제거.</summary>
        public bool ShouldDespawn(int currentDay)
        {
            return (currentDay - _spawnDay) >= _despawnAfterDays;
        }

        public Vector3Int CellPos => _cellPos;
    }
}