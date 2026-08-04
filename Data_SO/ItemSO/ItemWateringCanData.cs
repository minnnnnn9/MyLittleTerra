using UnityEngine;

namespace MLT.Data.ItemSO
{
    [CreateAssetMenu(fileName = "ItemWateringCanData", menuName = "Scriptable Objects/ItemWateringCanData")]
    public class ItemWateringCanData : ItemToolData
    {
        [Header("물뿌리개 정보")]
        public string _uniqueId;
        public int _level;
        public int _maxWateringTime;
        public Vector2Int[] _targetTiles;

        

        // 인스펙터에서 값이 수정될 때마다 호출되는 함수
        private void OnValidate()
        {
            UpdateDescription();
        }

        public new void UpdateDescription()
        {
            string stringBuilder = "";
            stringBuilder += $"레벨 : {_level} \n\n";
            stringBuilder += $"{_maxWateringTime}회분의 물을 담을 수 있다.";

            _description = stringBuilder;
        }
    }
}