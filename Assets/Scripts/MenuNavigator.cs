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
    private string _highlightedMenuItemClass;
    private float _connectionDistance;

    private MenuItem[] _menuItems;
    private int _activeMenuItem;

    public bool IsReady { get; private set; }

    public MenuNavigator(VisualElement menuRoot, string menuItemClass, string highlightedMenuItemClass, float connectionDistance = 20) 
    { 
        _menuRootElement = menuRoot;
        _menuItemClass = menuItemClass;
        _highlightedMenuItemClass = highlightedMenuItemClass;
        _connectionDistance = connectionDistance;
        IsReady = false;

        Debug.Log("Building Menu Navigation");
        UniTask.Void(BuildNavigation);
    }

    async UniTaskVoid BuildNavigation()
    {
        await UniTask.NextFrame();
        //setup _menuItems
        List<VisualElement> menuItems = _menuRootElement.Query<VisualElement>(null, _menuItemClass).Build().ToList();
        _menuItems = new MenuItem[menuItems.Count];
        for (int i = 0; i < menuItems.Count; i++)
        {
            _menuItems[i] = new MenuItem();
            _menuItems[i].element = menuItems[i];
            Debug.Log($"element found: {_menuItems[i].element.name}");
        }
        
        //find the neighbors of each menu item
        for (int i = 0; i < _menuItems.Length; i++)
        {
            //check up
            Rect elementRect = _menuItems[i].element.worldBound;
            Rect connectionRect = new Rect(elementRect.x, elementRect.y - _connectionDistance, elementRect.width, elementRect.height + _connectionDistance);
            _menuItems[i].neighborUp = FindMenuItemInRectExcludingIndex(connectionRect, i);

            //check down
            connectionRect = new Rect(elementRect.x, elementRect.y, elementRect.width, elementRect.height + _connectionDistance);
            _menuItems[i].neighborDown = FindMenuItemInRectExcludingIndex(connectionRect, i);

            //check left
            connectionRect = new Rect(elementRect.x - _connectionDistance, elementRect.y, elementRect.width + _connectionDistance, elementRect.height);
            _menuItems[i].neighborLeft = FindMenuItemInRectExcludingIndex(connectionRect, i);

            //check right
            connectionRect = new Rect(elementRect.x, elementRect.y, elementRect.width + _connectionDistance, elementRect.height);
            _menuItems[i].neighborRight = FindMenuItemInRectExcludingIndex(connectionRect, i);
        }

        await UniTask.Yield();

        _activeMenuItem = 0;
        UpdateHighlightedMenuItem();

        Debug.Log("Building Navigation Complete");
        IsReady = true;
    }

    int FindMenuItemInRectExcludingIndex(Rect rect, int index)
    {
        for (int j = 0; j < _menuItems.Length; j++)
        {
            //don't check for navigation to self
            if (index == j)
                continue;

            //found a neighbor
            if (_menuItems[j].element.worldBound.Overlaps(rect))
            {
                Debug.Log("Found connection");
                return j;
            }
        }
        return -1;
    }

    void UpdateHighlightedMenuItem()
    {
        for(int i = 0; i < _menuItems.Length; i++)
        {
            if (i == _activeMenuItem)
                _menuItems[i].element.AddToClassList(_highlightedMenuItemClass);
            else
                _menuItems[i].element.RemoveFromClassList(_highlightedMenuItemClass);
        }
    }

    public void Navigate(Vector2 direction)
    {
        

        Debug.Log($"Navigating in direction {direction}");
        if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
        {
            if (direction.y > 0)
            {
                if (_menuItems[_activeMenuItem].neighborUp != -1)
                    _activeMenuItem = _menuItems[_activeMenuItem].neighborUp;
            }
            else
            {
                if (_menuItems[_activeMenuItem].neighborDown != -1)
                    _activeMenuItem = _menuItems[_activeMenuItem].neighborDown;
            }
        }
        else
        {
            if (direction.x > 0)
            {
                if (_menuItems[_activeMenuItem].neighborRight != -1)
                    _activeMenuItem = _menuItems[_activeMenuItem].neighborRight;
            }
            else
            {
                if (_menuItems[_activeMenuItem].neighborLeft != -1)
                    _activeMenuItem = _menuItems[_activeMenuItem].neighborLeft;
            }
        }
        UpdateHighlightedMenuItem();
    }

    public void Select()
    {
        Debug.Log($"Selected menu item {_activeMenuItem}");
        using (var e = new NavigationSubmitEvent() { target = _menuItems[_activeMenuItem].element })
            _menuItems[_activeMenuItem].element.SendEvent(e);
    }
}
