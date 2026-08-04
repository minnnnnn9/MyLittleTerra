using UnityEngine;

namespace MLT.Data
{
    [CreateAssetMenu(fileName = "ProcessingMachineData", menuName = "MLT/Machine/Processing Machine Data")]
    public class ProcessingMachineData : ScriptableObject
    {
        [Header("기계 정보")]
        public string MachineName = "가공기";
        public int MaxSlots = 1; // 동시에 가공 가능한 슬롯 수
    }
}