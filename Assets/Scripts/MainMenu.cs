using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour, Controls.IMenuActions
{
    public enum EActiveMenu { Title, Main_Menu, Options }

    [SerializeField] private string _fullscreenHiddenClass;
    [SerializeField] private string _mainMenuButtonClass;
    [SerializeField] private string _mainMenuButtonHighlightedClass;

    private UIDocument _mainMenuDocument;

    private EActiveMenu _activeMenu;
    private VisualElement _titleScreen;
    private VisualElement _mainMenu;
    private VisualElement _optionsMenu;

    private Controls _controls;

    private IDisposable _titleScreenAnyKeyPressEvent;
    private MenuNavigator _mainMenuNavigator;

    private void Awake()
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
        SetupMainMenuNavigator();
    }
    //--------End-Title--------

    //-------------------------
    void SetupMainMenuNavigator()
    {
        _mainMenuNavigator = new MenuNavigator(_mainMenu, _mainMenuButtonClass, _mainMenuButtonHighlightedClass);
    }
    //-------------------------

    public void OnNavigation(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        switch (_activeMenu)
        {
            case EActiveMenu.Main_Menu:
                if (_mainMenuNavigator.IsReady)
                    _mainMenuNavigator.Navigate(context.ReadValue<Vector2>());
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
                if (_mainMenuNavigator.IsReady)
                    _mainMenuNavigator.Select();
                break;

            default:
                break;
        }
    }

    public void OnBack(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        //option not available in main menu
        return;
    }
}
