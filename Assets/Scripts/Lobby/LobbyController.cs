using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Networking;
using UnityEngine.SceneManagement;


public class LobbyController : NetworkBehaviour
{
    public Text playerList;
    public Text joinCode;
    public string gameScene;
    public GameObject startButton;
    public InputField item_count;
    public InputField rare_rate;
    public InputField cube_count;
    public InputField spawn_radius;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        UpdatePlayerList();

        if (!IsServer)
        {
            startButton.SetActive(false);
        }

        rare_rate.SetTextWithoutNotify(LobbySettings.Singleton.rareItemDropCount.ToString());
        item_count.SetTextWithoutNotify(LobbySettings.Singleton.itemSpawnCount.ToString());
        cube_count.SetTextWithoutNotify(LobbySettings.Singleton.cubeCount.ToString());
        spawn_radius.SetTextWithoutNotify(LobbySettings.Singleton.spawnRadius.ToString());

        joinCode.text = joinCode.text + NetworkController.Singleton.gameJoinCode;

        NetworkManager.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        NetworkManager.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.OnClientDisconnectCallback -= OnClientDisconnect;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void UpdatePlayerList()
    {
        playerList.text = string.Empty;

        RequestPlayerListRpc(NetworkManager.LocalClientId);
    }

    [Rpc(SendTo.Server)]
    void RequestPlayerListRpc(ulong client)
    {
        foreach(var player in NetworkController.Singleton.clientData)
        {
            var payLoad = JsonUtility.ToJson(player.Value);
            byte[] payLoadBytes = System.Text.Encoding.UTF8.GetBytes(payLoad);

            SendPlayerDataToClientRpc(payLoadBytes, RpcTarget.Single(client, RpcTargetUse.Temp));
        }

    }

    [Rpc(SendTo.SpecifiedInParams)]
    void SendPlayerDataToClientRpc(byte[] payloadBytes, RpcParams rpcParams)
    {
        var connectionData = System.Text.Encoding.UTF8.GetString(payloadBytes);
        var payload = JsonUtility.FromJson<PlayerData>(connectionData);
        Random.InitState(payload.name.GetHashCode());
        playerList.text = playerList.text + $"<color=#{ColorUtility.ToHtmlStringRGB(Random.ColorHSV())}>\u25A3</color> {payload.name}\n";

    }

    [Rpc(SendTo.Everyone)]
    void TriggerPlayerListUpdateRpc()
    {
        UpdatePlayerList();
    }


    void OnClientConnected(ulong clientId)
    {
        if (IsServer) { TriggerPlayerListUpdateRpc(); }
    }

    void OnClientDisconnect(ulong clientId)
    {
        if (IsServer) { TriggerPlayerListUpdateRpc(); }
    }

    public void OnLeaveGame()
    {
        Debug.Log("leaving");
        NetworkController.Singleton.LeaveServer();
    }
    public void OnStartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(gameScene, LoadSceneMode.Single);

    }

    [Rpc(SendTo.Everyone)]
    public void UpdateCubeSettingsRpc(string value)
    {
        int n = Mathf.Clamp(int.Parse(value), 0, 2500);
        
        if (IsServer)
        {
            LobbySettings.Singleton.cubeCount = n;
        }

        cube_count.text = n.ToString();
        
        
    }
    [Rpc(SendTo.Everyone)]
    public void UpdateItemSettingsRpc(string value)
    {
        int n = Mathf.Clamp(int.Parse(value), 0, 2500);

        if (IsServer)
        {
            LobbySettings.Singleton.itemSpawnCount = n;
        }

        item_count.text = n.ToString();

    }
    [Rpc(SendTo.Everyone)]
    public void UpdateRareSettingsRpc(string value)
    {
        float n = Mathf.Clamp(float.Parse(value), 0, 100);

        if (IsServer)
        {
            LobbySettings.Singleton.rareItemDropCount = n;
        }

        rare_rate.text = n.ToString();

    }

    [Rpc(SendTo.Everyone)]
    public void UpdateSpawnRadiusSettingRpc(string value)
    {
        float n = Mathf.Clamp(float.Parse(value), 0, 500f);

        if (IsServer)
        {
            LobbySettings.Singleton.spawnRadius = n;
        }

        spawn_radius.text = n.ToString();

    }


}
