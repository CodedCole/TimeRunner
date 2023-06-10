using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public abstract void StartInteract();

    public abstract void EndInteract();

    // returns a float between 0.0 and 1.0 as a percentage of completion
    public abstract float GetInteractCompletion();

    public abstract bool IsInteractable();

    public abstract string GetInteractDescription();
}
