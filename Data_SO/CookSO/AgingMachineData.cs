using UnityEngine;

namespace MLT.Data
{
    [CreateAssetMenu(fileName = "AgingMachineData", menuName = "MLT/Machine/Aging Machine Data")]
    public class AgingMachineData : ScriptableObject
    {
        [Header("기계 정보")]
        public string MachineName = "숙성고";
        public int MaxSlots = 1; // 동시에 숙성 가능한 슬롯 수
    }
}