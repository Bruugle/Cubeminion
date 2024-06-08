using System.Collections;
using System.Collections.Generic;
using UIIS;
using UnityEngine;

public class Musket : MonoBehaviour, Iusable
{
    public float attackRange = 100f;
    public float spread = .05f;
    public float instantKillChance = .5f;
    public float bleedAmount = 30f;
    public float pushForce;
    public float upwardForce;

    public void OnUse()
    {
        Vector2 variation = spread * Random.insideUnitCircle;
        Vector3 direction = transform.forward + variation.x * transform.right + variation.y * transform.up;
        var ray = new Ray(transform.position + transform.forward, direction);
        var hitInfo = new RaycastHit();
        if (Physics.Raycast(ray, out hitInfo, attackRange))
        {
            var other = hitInfo.collider.gameObject;
            if (other.TryGetComponent<IHealthComponent>(out IHealthComponent healthComponent))
            {
                if (Random.value * attackRange > hitInfo.distance)
                {
                    Vector3 killForce = transform.forward * pushForce + transform.up * upwardForce;
                    healthComponent.InstantKill(killForce);
                }
                else
                {
                    healthComponent.AddWound(bleedAmount * Random.value);
                }
            }
        }


        EffectsManager.Singleton.SpawnGunSmoke(transform.position + transform.forward, transform.rotation);
        FearTrigger.Singleton.TriggerFearEvent(transform.position, 10f, 50f);
        GetComponent<Consumable>().Consume();

        var inventory = GetComponent<ItemUseInputs>().slot.inventory;
        if (inventory.contents.Count >= 9) return;
        inventory.Add("unloaded musket");
    }
}
