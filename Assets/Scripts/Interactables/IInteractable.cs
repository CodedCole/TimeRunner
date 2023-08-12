using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public abstract void StartInteract(GameObject actor);

    public abstract void EndInteract(GameObject actor);

    // returns a float between 0.0 and 1.0 as a percentage of completion
    public abstract float GetInteractProgress();

    public abstract bool IsInteractable();

    public abstract string GetInteractDescription();
}
