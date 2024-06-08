using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Utilities.DeveloperConsole;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;


namespace Networking
{
    public class NetworkController : NetworkBehaviour
    {
        public bool useRelay;

        ConnectionPayloadData connectionPayload = new();
        public Dictionary<ulong, PlayerData> clientData;
        public PlayerData localPlayerData;
        public string gameJoinCode;
        public int allocationNum;
        

        public static NetworkController Singleton { get; private set; }
        bool isDying;

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
                isDying = true;
            }
        }

        private void Start()
        {
            if (isDying) { return; }

            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            NetworkManager.Singleton.OnServerStopped += OnServerStop;
            NetworkManager.Singleton.OnClientStopped += OnServerStop;

            #region CONSOLE_COMMAND_LISTENERS

            ConsoleController.Singleton.StartListening(Command.server_info, OnDebugInfo);

            #endregion
        }


        #region CONSOLE_COMMANDS

        void OnDebugInfo(string[] args)
        {
            ConsoleController.Singleton.AddLog($"connected client count: {clientData.Count}");
            ConsoleController.Singleton.AddLog("client ids:");
            foreach (var client in clientData)
            {
                ConsoleController.Singleton.AddLog($" client Id: {client.Key} || player name: {client.Value.name}");
            }
        }

        #endregion


        private void Init()
        {
            DontDestroyOnLoad(gameObject);
            localPlayerData = new PlayerData();
            GetPlayerPrefData();
        }
        private void OnApplicationQuit()
        {
            SetPlayerPrefData();
        }

        public async void StartServer()
        {
            if (allocationNum < 1)
            {
                allocationNum = 1;
            }

            clientData = new()
            {
                { NetworkManager.Singleton.LocalClientId, localPlayerData }
            };

            if (useRelay)
            {
                gameJoinCode = await StartHostWithRelay(allocationNum);
            }
            else
            {
                gameJoinCode = string.Empty;
                NetworkManager.Singleton.StartHost();
            }


            NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
        }

        public void ReloadLobby()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
            }
        }

        public async void JoinServer()
        {
            connectionPayload.playerData = localPlayerData;
            var payLoad = JsonUtility.ToJson(connectionPayload);
            byte[] payLoadBytes = System.Text.Encoding.UTF8.GetBytes(payLoad);

            NetworkManager.Singleton.NetworkConfig.ConnectionData = payLoadBytes;

            if (useRelay)
            {
                bool success = await StartClientWithRelay(gameJoinCode);
            }
            else
            {
                NetworkManager.Singleton.StartClient();
            }

            ConsoleController.Singleton.consoleAllowed = false;

        }

        public async Task<string> StartHostWithRelay(int maxConnections = 5)
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return NetworkManager.Singleton.StartHost() ? joinCode : null;
        }

        public async Task<bool> StartClientWithRelay(string joinCode)
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
            return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
        }


        public void LeaveServer()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.Shutdown();
            }
            else
            {
                DisconnectClientRpc(NetworkManager.LocalClientId);
            }
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            var clientId = request.ClientNetworkId;
            var connectionData = System.Text.Encoding.UTF8.GetString(request.Payload);
            var payLoad = JsonUtility.FromJson<ConnectionPayloadData>(connectionData);

            if (!clientData.ContainsKey(clientId))
            {
                clientData.Add(clientId, payLoad.playerData);
            }

            response.Position = Vector3.zero;

            response.Approved = true;
            response.CreatePlayerObject = true;

            

        }

        private void OnClientDisconnected(ulong client)
        {
            if (IsServer)
            {
                clientData.Remove(client);
            }
        }

        private void OnServerStop(bool value)
        {
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }


        public void UpdatePlayerData(ulong client, PlayerData playerData)
        {
            ConnectionPayloadData payloadData = new();
            payloadData.playerData = playerData;
            var payLoad = JsonUtility.ToJson(payloadData);
            byte[] payLoadBytes = System.Text.Encoding.UTF8.GetBytes(payLoad);

            UpdateClientDataRpc(client, payLoadBytes);
        }

        [Rpc(SendTo.Server)]
        private void UpdateClientDataRpc(ulong client, byte[] payloadBytes)
        {
            var connectionData = System.Text.Encoding.UTF8.GetString(payloadBytes);
            var payload = JsonUtility.FromJson<ConnectionPayloadData>(connectionData);

            if (!clientData.ContainsKey(client))
            {
                clientData.Add(client, payload.playerData);
            }
            else 
            {
                clientData[client] = payload.playerData;
            }
            
        }

        void GetPlayerPrefData()
        {
            localPlayerData.name = PlayerPrefs.GetString("username", "player name");
        }

        void SetPlayerPrefData()
        {
            PlayerPrefs.SetString("username", localPlayerData.name);
        }

        [Rpc(SendTo.Server)]
        void DisconnectClientRpc(ulong client)
        {
            NetworkManager.Singleton.DisconnectClient(client);
        }

    }
}

