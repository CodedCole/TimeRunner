using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;

public class MenuNavigator
{
    public class MenuItem
    {
        public VisualElement element;
        public int neighborUp;
        public int neighborDown;
        public int neighborLeft;
        public int neighborRight;
    }

    private VisualElement _menuRootElement;
    private string _menuItemClass;

    private MenuItem[] _menuItems;

    public bool IsReady { get; private set; }

    public MenuNavigator(VisualElement menuRoot, string menuItemClass) 
    { 
        _menuRootElement = menuRoot;
        _menuItemClass = menuItemClass;
        IsReady = false;
    }

    async void BuildNavigation()
    {
        await UniTask.Yield();

        IsReady = true;
    }

    void Navigate()
    {

    }

    void Select()
    {

    }
}
