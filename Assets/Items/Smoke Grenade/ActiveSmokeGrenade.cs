using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ActiveSmokeGrenade : NetworkBehaviour
{
    public float timer;
    public Color color;
    public Vector3 force;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        GetComponent<Rigidbody>().AddForce(force);

        if (!IsServer) return;

        StartCoroutine(SmokeAfterTime(timer));

    }

    IEnumerator SmokeAfterTime(float t)
    {
        yield return new WaitForSeconds(t);

        EffectsManager.Singleton.SpawnSmoke(transform.position, color);
        GetComponent<NetworkObject>().Despawn();

    }
}
