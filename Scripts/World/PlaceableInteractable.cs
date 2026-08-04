using UnityEngine;
using MLT.Core;
using MLT.Player;
using MLT.Data.ItemSO;

namespace MLT.World
{
    public class PlaceableInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemBaseData _requiredPlaceableItem;

        public bool CanInteract(PlayerController player)
        {
            if (_requiredPlaceableItem == null)
                return false;

            return player.Data.Inventory.HasItem(_requiredPlaceableItem, 1);
        }

        public void Interact(PlayerController player)
        {
            if (_requiredPlaceableItem == null)
                return;

            bool hasItem = player.Data.Inventory.HasItem(_requiredPlaceableItem, 1);
            if (!hasItem)
                return;

            player.Data.Inventory.TryRemoveItem(_requiredPlaceableItem, 1);


        }
    }
}