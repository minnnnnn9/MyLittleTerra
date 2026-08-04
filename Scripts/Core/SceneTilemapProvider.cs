using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MLT.Core
{
    /// <summary>
    /// 씬마다 배치해서 해당 씬의 타일맵을 제공.
    /// DontDestroy 오브젝트에서 SceneTilemapProvider.Instance.ReferenceTilemap으로 접근.
    /// </summary>
    public class SceneTilemapProvider : MonoBehaviour
    {
        public static SceneTilemapProvider Instance { get; private set; }
        public static event Action OnProviderReady;

        [SerializeField] private Tilemap _baseTilemap;      
        [SerializeField] private Tilemap _floorTilemap;      
        [SerializeField] private Tilemap _objectTilemap;
        [SerializeField] private Grid _grid;

        public Tilemap BaseTilemap => _baseTilemap;
        public Tilemap FloorTilemap => _floorTilemap;
        public Tilemap ObjectTilemap => _objectTilemap;
        public Grid Grid => _grid;

        private void Awake()
        {
            Instance = this;
            OnProviderReady?.Invoke();
        }
    }
}