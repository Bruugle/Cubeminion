using System.Collections;
using System.Collections.Generic;
using UIIS;
using UnityEngine;

public class Consumable : MonoBehaviour
{
    public void Consume()
    {
        var inputs = GetComponent<ItemUseInputs>();
        inputs.slot.inventory.Remove(inputs.slot.itemHashCode);
        inputs.slot.OnDequip();
    }
}
