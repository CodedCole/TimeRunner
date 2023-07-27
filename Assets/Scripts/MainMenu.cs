using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour, Controls.IMenuActions
{
    [SerializeField] private string _hiddenClass;

    private UIDocument _mainMenuDocument;

    private VisualElement _titleScreen;
    private VisualElement _mainMenu;
    private VisualElement _optionsMenu;

    private Controls _controls;

    private void Awake()
    {
        _mainMenuDocument = GetComponent<UIDocument>();
        _titleScreen = _mainMenuDocument.rootVisualElement.Q<VisualElement>("title-screen");
        _mainMenu = _mainMenuDocument.rootVisualElement.Q<VisualElement>("main-menu");
        _optionsMenu = _mainMenuDocument.rootVisualElement.Q<VisualElement>("options-menu");

        _titleScreen.RemoveFromClassList(_hiddenClass);
        _mainMenu.AddToClassList(_hiddenClass);
        _optionsMenu.AddToClassList(_hiddenClass);

        _controls = new Controls();
        _controls.Menu.SetCallbacks(this);
        _controls.Menu.Enable();
    }

    public void OnNavigation(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
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
