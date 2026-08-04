using MLT.Core;
using MLT.Utils;
using System;
using UnityEngine;

namespace MLT.Data.ItemSO
{
    [CreateAssetMenu(fileName = "ItemPlaceableData", menuName = "MLT/Item/Placeable Data")]

    public class ItemPlaceableData : ItemBaseData
    {
        [Header("설치 프리팹")]
        public GameObject Prefab;

        [Header("설치 크기")]
        public Vector2Int Size = Vector2Int.one;

        [Header("필요 재료")]

        public ItemMaterialDataGroup[] _needMaterials;


        private void OnValidate()
        {
            UpdateDescription();
        }

        public void UpdateDescription()
        {
            string stringBuilder = "";

            if (_name == "숙성통")
            {

                stringBuilder = "작물을 숙성시키는 통입니다.\n아이템을 드래그해서 기계를 클릭하면 숙성 시작\n 제작 시 ";

                string tempText = "";
                for(int i = 0; i < _needMaterials.Length; i++)
                {
                    if(i >= _needMaterials.Length)
                    {
                        tempText += $"{_needMaterials[i].Item._name} { _needMaterials[i].Count}개";

                    }
                    else
                    {
                        tempText += $"\n{_needMaterials[i].Item._name} {_needMaterials[i].Count}개 필요";

                    }
                        

                }
                stringBuilder += tempText;

            }

            else if (_name == "식품가공기")
            {
                stringBuilder = "작물을 가공하는 통입니다.\n아이템을 드래그해서 기계를 클릭하면 가공 시작\n\n";

                string tempText = "";
                for (int i = 0; i < _needMaterials.Length; i++)
                {
                    if (i >= _needMaterials.Length)
                    {
                        tempText += $"{_needMaterials[i].Item._name} {_needMaterials[i].Count}개";

                    }
                    else
                    {
                        tempText += $"\n{_needMaterials[i].Item._name} {_needMaterials[i].Count}개 필요";

                    }


                }
                stringBuilder += tempText;
            }

            else if (_name == "씨앗 추출기")
            {
                stringBuilder = "작물을 넣으면 해당 작물의 씨앗이 추출되는 기계입니다.\n낮은 확률로 특별한 씨앗을 획득할 수 있습니다.\n\n";

                string tempText = "";
                for (int i = 0; i < _needMaterials.Length; i++)
                {
                    if (i >= _needMaterials.Length)
                    {
                        tempText += $"{_needMaterials[i].Item._name} {_needMaterials[i].Count}개";

                    }
                    else
                    {
                        tempText += $"\n{_needMaterials[i].Item._name} {_needMaterials[i].Count}개 필요";

                    }


                }
                stringBuilder += tempText;

            }

            else if (_name == "요리용 냄비")
            {
                stringBuilder = "레시피에 맞는 재료를 넣으면 요리가 만들어지는 기계입니다.\n\n";

                string tempText = "";
                for (int i = 0; i < _needMaterials.Length; i++)
                {
                    if (i >= _needMaterials.Length)
                    {
                        tempText += $"{_needMaterials[i].Item._name} {_needMaterials[i].Count}개";

                    }
                    else
                    {
                        tempText += $"\n{_needMaterials[i].Item._name} {_needMaterials[i].Count}개 필요";

                    }


                }
                stringBuilder += tempText;

            }

            _description = stringBuilder;
        }
    }

    [Serializable]
    public class ItemMaterialDataGroup
    {
        public ItemBaseData Item;
        public int Count;
    }
}