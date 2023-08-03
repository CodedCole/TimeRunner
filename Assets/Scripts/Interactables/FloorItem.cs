using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorItem : MonoBehaviour, IInteractable
{
    [SerializeField] private Item _item;

    public void StartInteract()
    {
        int pickedUp = FindObjectOfType<Inventory>().GetContainer().AddItem(_item.MakeItemInstance());
        if (pickedUp == 0)
        {
            Destroy(gameObject);
        }
    }

    public void EndInteract()
    {

    }

    public float GetInteractProgress()
    {
        return 0;
    }

    public string GetInteractDescription()
    {
        return "Pick up";
    }

    public bool IsInteractable()
    {
        return true;
    }
}
