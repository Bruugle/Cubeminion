using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIIS;
using Unity.Netcode;
using Unity.Netcode.Components;

public class SmokeGrenadeItem : NetworkBehaviour, Iusable
{
    public GameObject GrenadePrefab;
    public float throwforce = 100f;

    public void OnUse()
    {
        var c = GetComponent<ItemUseInputs>().slot.inventory.GetComponent<Pawn>().color;
        SpawnGrenadeRpc(Camera.main.transform.forward * throwforce, c.r, c.g, c.b);

        GetComponent<Consumable>().Consume();
    }

    [Rpc(SendTo.Server)]
    void SpawnGrenadeRpc(Vector3 force, float r, float g, float b)
    {
        var go = Instantiate(GrenadePrefab, transform.position, Quaternion.identity);
        go.GetComponent<ActiveSmokeGrenade>().color = new Color(r, g, b);
        go.GetComponent<ActiveSmokeGrenade>().force = force;
        go.GetComponent<NetworkObject>().Spawn(true);
        
    }
}
