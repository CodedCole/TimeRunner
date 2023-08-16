using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthGadgetItemInstance : GadgetItemInstance
{
    public override Item item { get { return healthGadget; } }
    public HealthGadgetItem healthGadget { get; private set; }

    public HealthGadgetItemInstance(HealthGadgetItem templateItem) : base(templateItem)
    {
        healthGadget = templateItem;
    }

    public override void Use(GameObject user)
    {
        Health userHealth = user.GetComponentInChildren<Health>();
        if (userHealth != null)
        {
            bool fullHealth = userHealth.Heal(healthGadget.HealthRestored);
            if (fullHealth)
                Debug.Log("Fully Healed");
        }
        base.Use(user);
    }
}
