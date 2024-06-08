using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Networking;
using Unity.Netcode;
using Utilities.DeveloperConsole;

public class Player : NetworkBehaviour, IKillable
{
    PlayerData playerData;

    public string playerName;
    public Color playerColor;
    public GameObject Pawn;
    public GameObject spectatorObj;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        #region CONSOLE_COMMAND_LISTENERS

        ConsoleController.Singleton.StartListening(Command.kill, OnConsoleKill);

        #endregion

        // when the player spawns on the network it should instruct the server to send the player data to all clients
        if (!IsOwner)
            return;

        PopulatePlayerDataRequestRpc(NetworkManager.LocalClientId);

        NetworkManager.OnClientConnectedCallback += OnClientConnected;

    }

    #region CONSOLE_COMMANDS

    void OnConsoleKill(string[] args)
    {
       if(string.Join(" ", args) == playerName)
       {
            ulong toClient = NetworkObject.OwnerClientId;
            OnConsoleKillRpc(RpcTarget.Single(toClient, RpcTargetUse.Temp));
       }
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void OnConsoleKillRpc(RpcParams rpcParams)
    {
        if (Pawn != null && Pawn.TryGetComponent<DeathComponent>(out var deathComponent))
        {
            deathComponent.TriggerDeath(playerColor);
        }
    }

    #endregion

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        #region CONSOLE_COMMAND_LISTENERS

        ConsoleController.Singleton.StopListening(Command.kill, OnConsoleKill);

        #endregion

        if (!IsOwner) return;

        NetworkManager.OnClientConnectedCallback -= OnClientConnected;

    }


    [Rpc(SendTo.Server)]
    void PopulatePlayerDataRequestRpc(ulong client)
    {

        var payLoad = JsonUtility.ToJson(NetworkController.Singleton.clientData[client]);
        byte[] payLoadBytes = System.Text.Encoding.UTF8.GetBytes(payLoad);

        SendPlayerDataEveryoneRpc(payLoadBytes);

        
    }

    [Rpc(SendTo.Everyone)]
    void SendPlayerDataEveryoneRpc(byte[] payloadBytes)
    {
        var connectionData = System.Text.Encoding.UTF8.GetString(payloadBytes);
        var payload = JsonUtility.FromJson<PlayerData>(connectionData);
        playerData = payload;

        PopulateDataFields();
    }

    private void PopulateDataFields()
    {
        playerName = playerData.name;
        Random.InitState(playerName.GetHashCode());
        playerColor = Random.ColorHSV();
    }

    // send player data to late joiners

    private void OnClientConnected(ulong client)
    {
        RequestPlayerDataToClientRpc(NetworkManager.LocalClientId, client);
    }

    [Rpc(SendTo.Server)]
    void RequestPlayerDataToClientRpc(ulong client, ulong toClient)
    {
        var payLoad = JsonUtility.ToJson(NetworkController.Singleton.clientData[client]);
        byte[] payLoadBytes = System.Text.Encoding.UTF8.GetBytes(payLoad);

        SendPlayerDataToClientRpc(payLoadBytes, RpcTarget.Single(toClient, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void SendPlayerDataToClientRpc(byte[] payloadBytes, RpcParams rpcParams)
    {
        var connectionData = System.Text.Encoding.UTF8.GetString(payloadBytes);
        var payload = JsonUtility.FromJson<PlayerData>(connectionData);
        playerData = payload;

        PopulateDataFields();
    }

    public void OnDeath()
    {
        Debug.Log($"{playerName} has died");

        SpawnSpectator();
    }

    public void SpawnSpectator()
    {
        var spec = Instantiate(spectatorObj, Camera.main.transform.position, Camera.main.transform.rotation);

        MouseAim mouseAim = Camera.main.GetComponentInParent<MouseAim>();

        mouseAim.focus = spec.transform;
        mouseAim.TeleportCamera(spec.transform.position);
        mouseAim.cameraPosition = Vector3.zero;


        SpectatorManager.Singleton.SpectatorAdded?.Invoke();
        SpectatorManager.Singleton.IsSpectating = true;
        SpectatorManager.Singleton.spectator = spec;
    }
}
