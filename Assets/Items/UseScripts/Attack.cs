using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIIS;

public class Attack : MonoBehaviour, Iusable
{
    public float instantKillChance;
    public float bleedAmount;
    public float attackRange;
    public float pushForce;
    public float upwardForce;


    public OwnerNetworkAnimator animator;
    

    public void OnUse()
    {
        animator.SetTrigger("attack");


        var ray = new Ray(transform.position - transform.forward * .5f - transform.right * .5f, transform.forward);
        var hitInfo = new RaycastHit();
        if (Physics.Raycast(ray, out hitInfo, attackRange + .5f))
        {
            var other = hitInfo.collider.gameObject;
            if (other.TryGetComponent<IHealthComponent>(out IHealthComponent healthComponent))
            {
                if (Random.value < instantKillChance)
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


    }

}
