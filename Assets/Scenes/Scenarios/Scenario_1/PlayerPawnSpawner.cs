using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerPawnSpawner : NetworkBehaviour
{

    public GameObject pawnPrefab;
    public float radius;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        NetworkManager.OnClientConnectedCallback += OnClientConnected;

        GameManager.Singleton.StartGame += OnGameStart;
        
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        NetworkManager.OnClientConnectedCallback -= OnClientConnected;
        GameManager.Singleton.StartGame -= OnGameStart;

    }

    void OnGameStart()
    {
        if (!IsServer) return;

        radius = LobbySettings.Singleton.spawnRadius;

        foreach (var clientId in NetworkManager.ConnectedClientsIds)
        {
            SpawnPlayerPawn(clientId);
        }
    }


    [Rpc(SendTo.Server)]
    public void RespawnRpc(ulong clientId)
    {
        SpawnPlayerPawn(clientId);
    }

    void SpawnPlayerPawn(ulong client)
    {
        Vector2 coords = Random.insideUnitCircle;
        Vector3 position = radius * new Vector3(coords.x, 0f, coords.y);

        GameObject go = Instantiate(pawnPrefab, position, Quaternion.identity);
        go.GetComponent<NetworkObject>().SpawnWithOwnership(client, true);

    }

    void OnClientConnected(ulong client)
    {

        if (client == NetworkManager.LocalClientId && GameManager.Singleton.gameIsStarted)
        {
            NetworkManager.LocalClient.PlayerObject.GetComponent<Player>().SpawnSpectator();
        }
    }
   
}
