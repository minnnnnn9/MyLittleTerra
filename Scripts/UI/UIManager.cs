using MLT;
using MLT.Core;
using MLT.Player;
using MLT.UI;
using MLT.Audio;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{

    [SerializeField] private GameObject _hotBarUI;
    [SerializeField] private GameObject _topBarUI;
    [SerializeField] private GameObject[] _playerMenuUI;
    [SerializeField] private GameObject _tooltipUI;

    [Header("기계 UI")]
    [SerializeField] private GameObject _cookingUI;    // 요리 냄비 UI
    [SerializeField] private GameObject _extractorUI;  // 씨앗 추출기 UI
    [SerializeField] private GameObject _processingUI; // 식품 가공기 UI


    [SerializeField] private GameObject _shippingBoxUI;

    [SerializeField] private GameObject _cutsceneInvisibleUI;
    [SerializeField] private GameObject _dialogueUI;

    [SerializeField] private GameObject _basicShopUI;

    [SerializeField] private GameObject _UICanvas;

    private bool _isInitialized = false;

    public ShopUI ShopUI { get; private set; }

    public static UIManager Instance { get; private set; }

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
        }

        else
        {
            Destroy(gameObject);

        }
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        CloseInventory();
        
        if (_playerMenuUI.Length != 0)
        {
            if (_playerMenuUI.Length != 0)
            {
                foreach (var UI in _playerMenuUI)
                {
                    if(UI != null)
                    UI.SetActive(false);

                }

            }
        }

        _isInitialized = true;

    }
    
    public GameObject LoadBasicShopUI()
    {


        return _basicShopUI;
    }
    public void RegisterShopUI(ShopUI shopUI)
    {
        ShopUI = shopUI;
    }
    private void OnEnable()
    {
        EventBus.OnOpenInventory += OpenInventory;
        EventBus.OnCloseInventory += CloseInventory;

        // 기계 UI 이벤트 구독
        EventBus.OnOpenCookingUI += OpenCookingUI;
        EventBus.OnOpenExtractorUI += OpenExtractorUI;
        EventBus.OnOpenProcessingUI += OpenProcessingUI;
        EventBus.OnCloseAllMachineUI += CloseAllMachineUI;
       
    }

    private void OnDisable()
    {
        EventBus.OnCloseInventory -= CloseInventory;
        EventBus.OnOpenInventory -= OpenInventory;

        EventBus.OnOpenCookingUI -= OpenCookingUI;
        EventBus.OnOpenExtractorUI -= OpenExtractorUI;
        EventBus.OnOpenProcessingUI -= OpenProcessingUI;
        EventBus.OnCloseAllMachineUI -= CloseAllMachineUI;
       
    }

    // ───────────────────────────────────────────
    // 인벤토리
    // ───────────────────────────────────────────

    public void OpenInventory()
    {
        if (_playerMenuUI != null && _tooltipUI != null)
        {

            _playerMenuUI[0].SetActive(true);
            EventBus.RaiseSFX(SFXType.UI_OPEN);

        }

    }

    public void CloseInventory()
    {
        if (_playerMenuUI != null && _tooltipUI != null)
        {
            if (_playerMenuUI[0].activeSelf == true)
            {
                if (!_isInitialized)
                {
                    return;
                }
                EventBus.RaiseSFX(SFXType.UI_CLOSE);
            }
            _playerMenuUI[0].SetActive(false);

            

        }



    }

    public void OnUICanvas()
    {
        _UICanvas.SetActive(true);

    }

    public void OffUICanvas()
    {
        _UICanvas.SetActive(false);


    }
  

    // ───────────────────────────────────────────
    // 기계 UI 
    // ───────────────────────────────────────────
    public void OpenCookingUI()
    {
        if (_cookingUI != null) _cookingUI.SetActive(true);
    }

    public void OpenExtractorUI(Vector3Int pos)
    {
        if (_extractorUI != null)
        {
            _extractorUI.SetActive(true);
            // ExtractorUI 스크립트의 OpenUI를 호출해서 위치 정보를 전달함
            _extractorUI.GetComponent<ExtractorUI>().OpenUI(pos);
        }
    }

    public void OpenProcessingUI()
    {
        if (_processingUI != null) _processingUI.SetActive(true);
    }

    public void CloseAllMachineUI()
    {
        if (_cookingUI != null) _cookingUI.SetActive(false);
        if (_extractorUI != null) _extractorUI.SetActive(false);
        if (_processingUI != null) _processingUI.SetActive(false);
    }



    public void OpenShippingBox()
    {
        if (Keyboard.current.oKey.wasPressedThisFrame && _shippingBoxUI != null)
        {
            Debug.LogWarning("오키눌림 : " + Keyboard.current.oKey);
            _shippingBoxUI.SetActive(!_shippingBoxUI.activeSelf);
        }

    }

    public void InvisibleUIOnCutscene()
    {
        if (_cutsceneInvisibleUI != null)
            _cutsceneInvisibleUI.SetActive(false);

    }

    public void VisibleUIOnCutscene()
    {
        if (_cutsceneInvisibleUI != null)
            _cutsceneInvisibleUI.SetActive(true);

    }

    public void OnDialogueUI()
    {
        _dialogueUI.SetActive(true);
        EventBus.RaiseSFX(SFXType.DIALOGUE_OPEN);

    }

    public void OffDialogueUI()
    {
        _dialogueUI.SetActive(false);

    }

    private void PlaySound(AudioEventSO sound)
    {

        if (sound != null)
            AudioManager.Instance?.PlaySFX(sound);
    }

}
