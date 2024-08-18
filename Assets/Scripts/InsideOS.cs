using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MyBox;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InsideOS : MonoBehaviour
{
    [SerializeField] private Interactive _button;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private AnimationCurve _bootWidthCurve, _bootHeightCurve;
    [SerializeField] private float _bootScaleTime = 1.5f;
    [SerializeField] private Material _onLightMaterial;
    [SerializeField] private Renderer _onLightRenderer;
    [SerializeField] private Image _bootLogo;
    [SerializeField] private Transform _cameraPosition;
    [SerializeField] private InsideOSAppData[] _appDatas = null;
    
    [Separator("Desktop")] 
    [SerializeField] private GameObject _desktopContainer;
    [SerializeField] private RectTransform _cursor;
    [SerializeField] private float _mouseSensitivity = 1;
    [SerializeField] private float _doubleClickTime = 0.5f;
    [SerializeField] private GraphicRaycaster _graphicRaycaster;
    
    [Separator("Apps")]
    [SerializeField] private RectTransform _appWindowContainer = null;
    [SerializeField] private Vector3 _firstAppWindowPosition = default;
    [SerializeField] private Vector3 _nextAppWindowOffset = default;

    [Separator("Printer")] 
    [SerializeField] private Transform _paperSpawnPosition;
    [SerializeField] private GameObject _paperPrefab;
    
    [Header("Debug")] 
    [SerializeField] private bool _fastStart = true;
    [SerializeField] private bool _startOn = true;

    public Camera _camera;
    /*
    private Camera Camera
    {
        get
        {
            _camera ??= Player.LocalPlayer.Camera;
            return _camera;
        }
    }
    */

    
    private bool _booting = false;
    private bool _isOn = false;
    private Material _orgOnLightMaterial = null;
    private Material _onLight = null;
    private bool _booted = false;
    private RectTransform _canvasRect = null;
    private float _doubleClickTimer = 0f;
    private InsideOSButton _selectedInsideOSButton = null;
    private bool _doubleClicking = false;
    private readonly Dictionary<AppType, GameObject> _appData = new();
    private readonly Dictionary<AppType, InsideOSApp> _runningApps = new();
    private List<InsideOSApp> _runningAppList = new();
    
    void Awake()
    {
        _canvasRect = _canvas.GetComponent<RectTransform>();
        ResetScreens();
        _button.OnUsed += PowerButtonPressed;
        _orgOnLightMaterial = _onLightRenderer.material;
        _onLight = Instantiate(_onLightMaterial);
        for (var i = 0; i < _appDatas.Length; i++)
        {
            _appData.Add(_appDatas[i].App, _appDatas[i].WindowPrefab);
        }
        
        if (_startOn)
            TurnOn();
    }

    private void OnDestroy()
    {
        Destroy(_onLight);
    }

    void Update()
    {
        if (_doubleClickTimer > 0f)
            _doubleClickTimer -= Time.deltaTime;
        for (var i = 0; i < _runningAppList.Count; i++)
        {
            _runningAppList[i].UpdateApp(_cursor.anchoredPosition);
        }
    }

    private void PowerButtonPressed(Interactive button, PlayerInteraction playerInteraction)
    {
        if (_booting) return;
        if (_isOn)
        {
            TurnOff();
        }
        else
        {
            TurnOn();
        }

        _isOn = !_isOn;
    }
    
    private async void BootOS()
    {
        _onLightRenderer.material = _onLight;
        if (!_fastStart)
            await UniTask.Delay(750);
        _bootLogo.transform.localScale = Vector3.zero;
        _bootLogo.enabled = true;
        if (!_fastStart)
        {
            LeanTween.scale(_bootLogo.gameObject, Vector3.one, _bootScaleTime).setEaseOutBack().setOvershoot(1.1f);
            await UniTask.Delay(4000);
        }
        _bootLogo.enabled = false;
        _desktopContainer.SetActive(true);
        _cursor.gameObject.SetActive(true);
        _booted = true;
        _booting = false;
    }

    private void ResetScreens()
    {
        _booting = false;
        _desktopContainer.SetActive(false);
        _cursor.gameObject.SetActive(false);
        _canvas.enabled = false;
        _canvas.transform.localScale = new Vector3(0, 0, 1);
        _bootLogo.enabled = false;
        _onLightRenderer.material = _orgOnLightMaterial;
    }

    private void TurnOn()
    {
        _booting = true;
        _canvas.transform.localScale = new Vector3(0, 0, 1);
        _canvas.enabled = true;
        LeanTween.scaleX(_canvas.gameObject, 0.1f, _bootScaleTime).setEase(_bootWidthCurve).setOnComplete(BootOS);
        LeanTween.scaleY(_canvas.gameObject, 0.1f, _bootScaleTime).setEase(_bootHeightCurve);
    }

    private void TurnOff()
    {
        _booting = true;
        LeanTween.scaleX(_canvas.gameObject, 0, _bootScaleTime).setEase(_bootHeightCurve).setOnComplete(ResetScreens);
        LeanTween.scaleY(_canvas.gameObject, 0, _bootScaleTime).setEase(_bootWidthCurve);
        _booted = false;
    }

    public void MouseCursorUpdate(Vector2 mouseDelta)
    {
        if (!_booted) return;
        mouseDelta.y *= -1;
        var pos = _cursor.anchoredPosition + mouseDelta * _mouseSensitivity;
        pos.y = Mathf.Clamp(pos.y, -_canvasRect.sizeDelta.y / 2f, _canvasRect.sizeDelta.y / 2f);
        pos.x = Mathf.Clamp(pos.x, -_canvasRect.sizeDelta.x / 2f, _canvasRect.sizeDelta.x / 2f);
        _cursor.anchoredPosition = pos;
    }

    public void MouseActionUpdate(PlayerInteraction.MouseAction mouseAction)
    {
        switch (mouseAction)
        {
            case PlayerInteraction.MouseAction.Pressed:
                var clickedButton = RayCastMouseClick();
                if (clickedButton is not null)
                {
                    clickedButton.Pressed();
                    if (_doubleClickTimer > 0f && clickedButton == _selectedInsideOSButton)
                    {
                        //Double click
                        _selectedInsideOSButton.DoubleClicked();
                        _doubleClickTimer = 0f;
                    }
                    else
                    {
                        //Single click
                        _doubleClickTimer = _doubleClickTime;
                        if (_selectedInsideOSButton is not null && _selectedInsideOSButton != clickedButton)
                        {
                            _selectedInsideOSButton.Deselect();
                        }

                        _selectedInsideOSButton = clickedButton;
                        clickedButton.Select();
                    }
                
                }
                else
                {
                    if (_selectedInsideOSButton is not null)
                        _selectedInsideOSButton.Deselect();
                    _selectedInsideOSButton = null;
                }
                break;
            case PlayerInteraction.MouseAction.Released:
                if (_selectedInsideOSButton is not null)
                {
                    _selectedInsideOSButton.Released();
                }
                break;
            case PlayerInteraction.MouseAction.Drag:
                break;
        }
    }

    private InsideOSButton RayCastMouseClick()
    {
        var pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = _camera.WorldToScreenPoint(_cursor.transform.position);
        
        var results = new List<RaycastResult>();
        _graphicRaycaster.Raycast(pointerData, results);
        
        for (var i = 0; i < results.Count; i++)
        {
            var insideOSButton = results[i].gameObject.GetComponent<InsideOSButton>();
            if (insideOSButton is null || _runningApps.Count > 0 && insideOSButton is InsideOSIcon) continue;
            return insideOSButton;
        }

        return null;
    }

    public void OpenApp(string appName)
    {
        AppType appType = (AppType)Enum.Parse(typeof(AppType), appName);
        if (_runningApps.ContainsKey(appType) || !_appData.TryGetValue(appType, out var prefab)) return;
        var openPos = _firstAppWindowPosition + _runningApps.Count * _nextAppWindowOffset;
        var newApp = Instantiate(prefab, _appWindowContainer).GetComponent<InsideOSApp>();
        newApp.GetComponent<RectTransform>().anchoredPosition = openPos;
        _runningApps.Add(appType, newApp);
        newApp.Open(this);
        _runningAppList = _runningApps.Values.ToList();
    }

    private void BringAppToFront(AppType appType)
    {
        
    }

    public void CloseApp(AppType appType)
    {
        if (_runningApps.TryGetValue(appType, out var app))
        {
            Destroy(app.gameObject);
            _runningApps.Remove(appType);
        }
    }

    [Serializable]
    public struct InsideOSAppData
    {
        public AppType App;
        public GameObject WindowPrefab;
    }
}
