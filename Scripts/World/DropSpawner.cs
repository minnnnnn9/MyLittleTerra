using UnityEngine;
using MLT.Data.ItemSO;

namespace MLT.World
{
    public static class DropSpawner
    {
        public static void Spawn(ItemBaseData item, int amount, Vector3 centerPos, GameObject dropPrefab, float arcHeight = 0.8f, float arcDuration = 0.35f)
        {
            if (item == null || dropPrefab == null || amount <= 0)
                return;

            for (int i = 0; i < amount; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * 0.35f;
                Vector3 spawnPos = centerPos + (Vector3)randomOffset;
                spawnPos.z = 0f;

                GameObject obj = Object.Instantiate(dropPrefab, spawnPos, Quaternion.identity);

                ItemDrop itemDrop = obj.GetComponent<ItemDrop>();
                if (itemDrop != null)
                {
                    itemDrop.Initialize(item, 1, arcHeight, arcDuration);
                }
            }
        }
    }
}