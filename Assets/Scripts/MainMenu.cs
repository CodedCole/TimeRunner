using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour, Controls.IMenuActions
{
    public enum EActiveMenu { Title, Main_Menu, Options }

    [Header("Overall")]
    [SerializeField] private string _fullscreenHiddenClass;
    [Header("Main Menu")]
    [SerializeField] private string _mainMenuButtonClass;
    [SerializeField] private string _mainMenuButtonHighlightedClass;
    [Header("Options Menu")]
    [SerializeField] private string _optionsTabHiddenClass;

    private UIDocument _mainMenuDocument;

    private EActiveMenu _activeMenu;
    private VisualElement _titleScreen;
    private VisualElement _mainMenu;
    private VisualElement _optionsMenu;
    private VisualElement _loadingScreen;

    private Controls _controls;

    //title screen
    private IDisposable _titleScreenAnyKeyPressEvent;

    //main menu
    private RaidLoader _raidLoader;
    private bool _loadingRaid;
    private ProgressBar _raidLoadProgress;

    //options menu
    private VisualElement _controlsTabView;

    private VisualElement _audioTabView;
    private bool _setupAudioTab;
    private Slider _masterVolume;
    private Slider _musicVolume;
    private Slider _sfxVolume;

    private VisualElement _graphicsTabView;

    private void Start()
    {
        _mainMenuDocument = GetComponent<UIDocument>();
        _titleScreen = _mainMenuDocument.rootVisualElement.Q<VisualElement>("title-screen");
        _mainMenu = _mainMenuDocument.rootVisualElement.Q<VisualElement>("main-menu");
        _optionsMenu = _mainMenuDocument.rootVisualElement.Q<VisualElement>("options-menu");
        _loadingScreen = _mainMenuDocument.rootVisualElement.Q<VisualElement>("loading-screen");

        _activeMenu = EActiveMenu.Title;
        _titleScreen.RemoveFromClassList(_fullscreenHiddenClass);
        _mainMenu.AddToClassList(_fullscreenHiddenClass);
        _optionsMenu.AddToClassList(_fullscreenHiddenClass);
        _loadingScreen.AddToClassList(_fullscreenHiddenClass);

        _controls = new Controls();
        _controls.Menu.SetCallbacks(this);

        _titleScreenAnyKeyPressEvent = InputSystem.onAnyButtonPress.CallOnce(OnTitleAnyKeyPress);
        SetupMainMenu();

        UniTask.Void(AutoSelectPanelInEventSystem);
    }

    async UniTaskVoid AutoSelectPanelInEventSystem()
    {
        EventSystem es = FindObjectOfType<EventSystem>();
        await UniTask.WaitUntil(() => { return es.transform != null && es.transform.Find("PanelSettings") != null; }, PlayerLoopTiming.PostLateUpdate);
        es.SetSelectedGameObject(es.transform.Find("PanelSettings").gameObject, new BaseEventData(es));
    }

    //----------Title----------
    void OnTitleAnyKeyPress(InputControl ctrl)
    {
        Debug.Log($"pressed {ctrl.displayName} to start");
        _titleScreen.AddToClassList(_fullscreenHiddenClass);
        _mainMenu.RemoveFromClassList(_fullscreenHiddenClass);
        _controls.Menu.Enable();
        _titleScreenAnyKeyPressEvent.Dispose();
        _activeMenu = EActiveMenu.Main_Menu;            
    }
    //--------End-Title--------

    //--------Main-Menu--------
    void SetupMainMenu()
    {
        _mainMenu.Q<Button>("raid-button").clicked += StartLoadingRaid;
        _mainMenu.Q<Button>("options-button").clicked += SwitchToOptionsMenu;
        _mainMenu.Q<Button>("outskirts-button").clicked += () => { Debug.Log("OUTSKIRTS"); };
        _mainMenu.Q<Button>("exit-button").clicked += () => { Debug.Log("EXIT"); };
    }

    void SwitchToOptionsMenu()
    {
        _optionsMenu.RemoveFromClassList(_fullscreenHiddenClass);
        _mainMenu.AddToClassList(_fullscreenHiddenClass);
        _activeMenu = EActiveMenu.Options;
        SetupOptionsMenu();
    }

    void StartLoadingRaid()
    {
        if (_raidLoader == null)
            _raidLoader = FindObjectOfType<RaidLoader>();

        _loadingRaid = true;
        _controls.Menu.Disable();
        _raidLoader.onRaidLoaded += OnRaidLoaded;
        _raidLoader.LoadRaid();
        UniTask.Void(RunLoadingScreen);
    }

    async UniTaskVoid RunLoadingScreen()
    {
        if (_raidLoadProgress == null)
            _raidLoadProgress = _loadingScreen.Q<ProgressBar>("raid-load-progress");

        _mainMenu.AddToClassList(_fullscreenHiddenClass);
        _loadingScreen.RemoveFromClassList(_fullscreenHiddenClass);

        float progress;
        while (_loadingRaid)
        {
            progress = _raidLoader.GetLoadProgress();
            _raidLoadProgress.value = progress;
            _raidLoadProgress.title = $"Loading Raid {Mathf.RoundToInt(progress * 100)}%";
            await UniTask.Yield();
        }
    }

    void OnRaidLoaded()
    {
        _loadingRaid = false;
        _mainMenuDocument.enabled = false;
        _raidLoader.onRaidLoaded -= OnRaidLoaded;
    }
    //------End-Main-Menu------

    //-------Options-Menu------
    void SetupOptionsMenu()
    {
        if (_controlsTabView == null)
        {
            _controlsTabView = _optionsMenu.Q<VisualElement>("controls-options-tab-view");
            _optionsMenu.Q<Button>("controls-options-tab-button").clicked += ShowControlsTab;
            _audioTabView = _optionsMenu.Q<VisualElement>("audio-options-tab-view");
            _optionsMenu.Q<Button>("audio-options-tab-button").clicked += ShowAudioTab;
            _graphicsTabView = _optionsMenu.Q<VisualElement>("graphics-options-tab-view");
            _optionsMenu.Q<Button>("graphics-options-tab-button").clicked += ShowGraphicsTab;

            _optionsMenu.Q<Button>("back-button").clicked += ReturnToMainMenuFromOptions;
        }
        ShowControlsTab();
    }

    void ShowControlsTab()
    {
        _controlsTabView.RemoveFromClassList(_optionsTabHiddenClass);
        _audioTabView.AddToClassList(_optionsTabHiddenClass);
        _graphicsTabView.AddToClassList(_optionsTabHiddenClass);
    }

    void ShowAudioTab()
    {
        if (!_setupAudioTab)
            SetupAudioTab();

        _controlsTabView.AddToClassList(_optionsTabHiddenClass);
        _audioTabView.RemoveFromClassList(_optionsTabHiddenClass);
        _graphicsTabView.AddToClassList(_optionsTabHiddenClass);
    }

    void SetupAudioTab()
    {
        VisualElement volumeOptionsSubGroup = _optionsMenu.Q<VisualElement>("volume-options");

        _masterVolume = volumeOptionsSubGroup.Q<Slider>("master-volume-slider");
        _masterVolume.RegisterValueChangedCallback((ChangeEvent<float> evt) => PlayerOptions.SetMasterVolume(evt.newValue));
        _musicVolume = volumeOptionsSubGroup.Q<Slider>("music-volume-slider");
        _musicVolume.RegisterValueChangedCallback((ChangeEvent<float> evt) => PlayerOptions.SetMusicVolume(evt.newValue));
        _sfxVolume = volumeOptionsSubGroup.Q<Slider>("sfx-volume-slider");
        _sfxVolume.RegisterValueChangedCallback((ChangeEvent<float> evt) => PlayerOptions.SetSFXVolume(evt.newValue));

        volumeOptionsSubGroup.Q<Button>("reset-to-default-button").clicked += () => 
        { 
            PlayerOptions.VolumeResetToDefault();
            _masterVolume.SetValueWithoutNotify(PlayerOptions.GetMasterVolume());
            _musicVolume.SetValueWithoutNotify(PlayerOptions.GetMusicVolume());
            _sfxVolume.SetValueWithoutNotify(PlayerOptions.GetSFXVolume());
        };

        _masterVolume.SetValueWithoutNotify(PlayerOptions.GetMasterVolume());
        _musicVolume.SetValueWithoutNotify(PlayerOptions.GetMusicVolume());
        _sfxVolume.SetValueWithoutNotify(PlayerOptions.GetSFXVolume());

        _setupAudioTab = true;
    }

    void ShowGraphicsTab()
    {
        _controlsTabView.AddToClassList(_optionsTabHiddenClass);
        _audioTabView.AddToClassList(_optionsTabHiddenClass);
        _graphicsTabView.RemoveFromClassList(_optionsTabHiddenClass);
    }

    void ReturnToMainMenuFromOptions()
    {
        _optionsMenu.AddToClassList(_fullscreenHiddenClass);
        _mainMenu.RemoveFromClassList(_fullscreenHiddenClass);
        _activeMenu = EActiveMenu.Main_Menu;
    }
    //-----End-Options-Menu----

    public void OnNavigation(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        switch (_activeMenu)
        {
            case EActiveMenu.Main_Menu:
                break;

            default:
                break;
        }
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        switch (_activeMenu)
        {
            case EActiveMenu.Main_Menu:
                break;

            default:
                break;
        }
    }

    public void OnBack(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        switch (_activeMenu)
        {
            case EActiveMenu.Main_Menu:
                break;

            case EActiveMenu.Options:
                ReturnToMainMenuFromOptions();
                break;
        }
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        //option not available in main menu
        return;
    }
}
