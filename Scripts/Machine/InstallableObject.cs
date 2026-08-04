using MLT.Core;
using MLT.Player;
using UnityEngine;

namespace MLT.Machine
{
    
    public abstract class InstallableObject : MonoBehaviour, IInteractable
    {
       
        public Vector3Int CellPos { get; protected set; }
        [SerializeField] protected GameObject _dropPrefab;

        public virtual void Setup(Vector3Int pos)
        {
            CellPos = pos;
        }

        // --- IInteractable 구현 부 ---
        // 기계와 상호작용 가능한지 여부 (일단 무조건 true)
        public virtual bool CanInteract(PlayerController player) => true;

        // 플레이어가 클릭하면 호출됨 -> 자식 클래스의 OnInteract로 전달!
        public void Interact(PlayerController player)
        {
            OnInteract(player);
            
        }
        // -----------------------------

        // 플레이어가 클릭했을 때
        public abstract void OnInteract(PlayerController player);

        // 날짜가 지났을 때 
        public abstract void OnDayPassed();

        public virtual void OnUninstall()
        {
            // 기본: 아무것도 안 함. 자식에서 override
        }
    }
}