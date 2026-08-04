using UnityEngine;

namespace MLT.World
{
    public class PlacedObject : MonoBehaviour
    {
        public Vector3Int OriginCell { get; private set; }
        public Vector2Int Size { get; private set; }

        public void Initialize(Vector3Int originCell, Vector2Int size)
        {
            OriginCell = originCell;
            Size = size;
        }
    }
}