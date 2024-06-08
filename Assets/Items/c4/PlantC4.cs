using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIIS;
using Unity.Netcode;

public class PlantC4 : NetworkBehaviour, Iusable
{
    public GameObject armed_c4_prefab;

    public void OnUse()
    {
        SpawnC4Rpc(NetworkManager.Singleton.LocalClientId);
        GetComponent<Consumable>().Consume();
    }

    [Rpc(SendTo.Server)]
    void SpawnC4Rpc(ulong client)
    {
        var go = Instantiate(armed_c4_prefab, transform.position, Quaternion.identity);
        go.GetComponent<NetworkObject>().SpawnWithOwnership(client, true);

    }
}
