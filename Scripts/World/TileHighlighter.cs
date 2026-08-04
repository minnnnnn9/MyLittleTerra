using MLT.Core;
using MLT.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;


public class TileHighlighter : MonoBehaviour

{
    [Header("References")]
    [SerializeField] private Tilemap _tilemap;


    private Camera _mainCamera;

    public Vector3 HighlighterPos;

    private void Awake()
    {
        _mainCamera = Camera.main;
        if (SceneTilemapProvider.Instance != null) BindTilemaps();
    }

    private void OnEnable()
    {
        SceneTilemapProvider.OnProviderReady += BindTilemaps;
    }

    private void OnDisable()
    {
        SceneTilemapProvider.OnProviderReady -= BindTilemaps;
    }

    // OnSceneLoaded 전부 제거

    private void BindTilemaps()
    {
        _tilemap = SceneTilemapProvider.Instance.ObjectTilemap;
        if (_mainCamera == null) _mainCamera = Camera.main;
    }

    private void Update()
    {
        Vector2 mouseScreenPos =
            Mouse.current.position.ReadValue();

        Vector3 mouseWorldPos =
            _mainCamera.ScreenToWorldPoint(mouseScreenPos);

        mouseWorldPos.z = 0f;

        if(_tilemap != null)
        {
            Vector3Int cellPos =
            _tilemap.WorldToCell(mouseWorldPos);

            Vector3 cellWorldPos =
                _tilemap.GetCellCenterWorld(cellPos);

            HighlighterPos = cellWorldPos;

          

        }
    }
}