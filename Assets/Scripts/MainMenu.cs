using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
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

    private Controls _controls;

    //title screen
    private IDisposable _titleScreenAnyKeyPressEvent;

    //main menu

    //options menu
    private VisualElement _controlsTabView;
    private VisualElement _audioTabView;
    private VisualElement _graphicsTabView;

    private void Start()
    {
        _mainMenuDocument = GetComponent<UIDocument>();
        _titleScreen = _mainMenuDocument.rootVisualElement.Q<VisualElement>("title-screen");
        _mainMenu = _mainMenuDocument.rootVisualElement.Q<VisualElement>("main-menu");
        _optionsMenu = _mainMenuDocument.rootVisualElement.Q<VisualElement>("options-menu");

        _activeMenu = EActiveMenu.Title;
        _titleScreen.RemoveFromClassList(_fullscreenHiddenClass);
        _mainMenu.AddToClassList(_fullscreenHiddenClass);
        _optionsMenu.AddToClassList(_fullscreenHiddenClass);

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
        _mainMenu.Q<Button>("raid-button").clicked += () => { Debug.Log("RAID"); };
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
        _controlsTabView.AddToClassList(_optionsTabHiddenClass);
        _audioTabView.RemoveFromClassList(_optionsTabHiddenClass);
        _graphicsTabView.AddToClassList(_optionsTabHiddenClass);
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
                _titleScreenAnyKeyPressEvent = InputSystem.onAnyButtonPress.CallOnce(OnTitleAnyKeyPress);
                _mainMenu.AddToClassList(_fullscreenHiddenClass);
                _titleScreen.RemoveFromClassList(_fullscreenHiddenClass);
                _controls.Disable();
                _activeMenu = EActiveMenu.Title;
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
