using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Border : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.gameObject.TryGetComponent<IHealthComponent>(out var healthComponent))
        {
            healthComponent.InstantKill(10f * Vector3.up);
        }
    }
}
