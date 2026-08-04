using UnityEngine;
using MLT.Core;
using MLT.Utils;

namespace MLT.Data.ItemSO
{
    [CreateAssetMenu(fileName = "ItemCropData", menuName = "Scriptable Objects/ItemCropData")]
    public class ItemCropData : ItemBaseData
    {
        [Header("작물 정보")]
        public Season[] _seasons;
        [SerializeField] private ItemSeedData _itemSeedData;

        public override int SellPrice => _itemSeedData.BuyPrice * 2;



        private void OnValidate() => UpdateDescription();

        public void UpdateDescription()
        {
           
           
        string stringBuilder = "";

            if (_seasons != null && _seasons.Length > 0)
            {
                for (int i = 0; i < _seasons.Length; i++)
                {
                    stringBuilder += Util.EnumChangeKOR(_seasons[i].ToString());
                    if (i != _seasons.Length - 1) stringBuilder += ", ";
                }
                stringBuilder += "에 재배 가능한 작물\n\n";
            }

            if (_isAbleToEat)
            {
                stringBuilder += $"섭취 시 스태미나 {StaminaRecoveryAmount} 회복\n" + "핫바에서 선택 후 마우스 우클릭";
            }

            _description = stringBuilder;
        }
    }
}
