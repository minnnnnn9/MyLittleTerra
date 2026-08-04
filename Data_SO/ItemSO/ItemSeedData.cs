using UnityEngine;
using MLT.Core;
using MLT.Utils;

namespace MLT.Data.ItemSO
{
    [CreateAssetMenu(fileName = "ItemSeedData",
                     menuName = "Scriptable Objects/ItemSeedData")]
    public class ItemSeedData : ItemBaseData
    {
        [Header("씨앗 정보")]
        public Season[] _seasons;

        [Tooltip("수확까지 걸리는 게임 내 시간(Hour). 예) 24 = 하루")]
        public int _growthDurationHours;

        [Header("재수확 설정")]
        public bool _canRegrow;
        [Tooltip("재성장에 걸리는 게임 내 시간(Hour)")]
        public int _regrowDurationHours;

        public int _regrowSpriteIndex; 

        [Header("성장 단계별 스프라이트")]
        public Sprite[] growthSprites;

        [Header("수확물 데이터")]
        public ItemCropData _cropData;

        private void OnValidate() => UpdateDescription();

        public void UpdateDescription()
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < _seasons.Length; i++)
            {
                sb.Append(Util.EnumChangeKOR(_seasons[i].ToString()));
                if (i != _seasons.Length - 1) sb.Append(", ");
            }
            sb.Append("에 재배 가능\n\n");
            sb.Append($"성장까지 {_growthDurationHours}시간 소요");
            _description = sb.ToString();
        }

        /// <summary>
        /// elapsedMinutes: 심은 후 경과한 게임 내 분(TotalMinutes 기준)
        /// </summary>
        public Sprite GetGrowthSprite(int elapsedMinutes)
        {
            if (growthSprites == null || growthSprites.Length == 0) return null;

            int totalMinutes = _growthDurationHours * 60;
            if (totalMinutes <= 0) return growthSprites[growthSprites.Length - 1];

            // 경과 분 → 스프라이트 인덱스
            float progress = Mathf.Clamp01((float)elapsedMinutes / totalMinutes);
            int index = Mathf.Min(
                Mathf.FloorToInt(progress * growthSprites.Length),
                growthSprites.Length - 1
            );
            return growthSprites[index];
        }
    }
}