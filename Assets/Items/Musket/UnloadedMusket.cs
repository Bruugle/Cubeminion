using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIIS;

public class UnloadedMusket : MonoBehaviour, Iusable
{
    bool isReloading;
    public float reloadTimer = 20f;
    public Animator animator;
    bool finished;

    public void OnUse()
    {
        isReloading = true;
        animator.SetTrigger("use");
    }

    private void Update()
    {
        

        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
        }

        if (reloadTimer < 0f && !finished)
        {
            finished = true;
            GetComponent<Consumable>().Consume();
            var inventory = GetComponent<ItemUseInputs>().slot.inventory;
            if (inventory.contents.Count >= 9) return;
            inventory.Add("musket");
        }

    }

}
