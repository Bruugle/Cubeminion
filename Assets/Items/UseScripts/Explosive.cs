using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class Explosive : NetworkBehaviour
{

    public float radius;
    public float forceMultiplier;

    [Rpc(SendTo.Server)]
    void DespawnRpc()
    {
        GetComponent<NetworkObject>().Despawn();
    }

    public void Explode()
    {
        Vector3 scale = radius / 10f * Vector3.one;
        EffectsManager.Singleton.SpawnExplosion(transform.position, scale);
        FearTrigger.Singleton.TriggerFearEvent(transform.position, 2f * radius, 90f);

        foreach (var collider in Physics.OverlapSphere(transform.position, radius))
        {
            if (collider.TryGetComponent<IHealthComponent>(out var healthComponent))
            {
                Vector3 displacement = collider.transform.position - (transform.position - 2f * Vector3.up);
                Vector3 force = (forceMultiplier / displacement.sqrMagnitude) * displacement.normalized;
                healthComponent.InstantKill(force);
            }
        }
        DespawnRpc();

    }
}
