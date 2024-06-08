using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FearTrigger : NetworkBehaviour
{

    public static FearTrigger Singleton { get; private set; }

    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
            Init();
        }
        else if (Singleton != this)
        {
            Destroy(gameObject);
        }
    }

    public void Init()
    {
        
    }
    public void TriggerFearEvent(Vector3 center, float radius, float amount)
    {
        if (!IsServer) return;

        Collider[] hitColliders = Physics.OverlapSphere(center, radius);

        foreach (Collider collider in hitColliders)
        {
            if (collider.gameObject.TryGetComponent<HerdAnimal>(out var agent)) 
            {
                agent.OnFearEvent(center, amount);
            }
        }
    }
}
