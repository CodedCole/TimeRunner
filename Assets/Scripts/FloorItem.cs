using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorItem : MonoBehaviour, IInteractable
{
    [SerializeField] private Item _item;

    public void StartInteract()
    {
        bool pickedUp = FindObjectOfType<Inventory>().GetContainer().AddItem(_item);
        if (pickedUp)
        {
            Destroy(gameObject);
        }
    }

    public void EndInteract()
    {

    }

    public float GetInteractCompletion()
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
