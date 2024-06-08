using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using Utilities.DeveloperConsole;
using UnityEngine.UI;
using Networking;


public class GameManager : NetworkBehaviour
{
    public bool gameIsStarted;
    public bool gameIsEnded;
    public bool musicPlaying;
    public GameObject start_screen;
    public GameObject end_screen;
    public Text end_text;

    public Action StartGame;
    public Action EndGame;
    public Action LateConnectionTrigger;

    public List<NetworkObject> pawnList = new List<NetworkObject>();

    public static GameManager Singleton { get; private set; }

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

    public virtual void Init()
    {

        NetworkManager.OnClientConnectedCallback += OnClientConnected;

        #region CONSOLE_COMMAND_LISTENERS

        ConsoleController.Singleton.StartListening(Command.start_game, OnStartGameCommand);
        ConsoleController.Singleton.StartListening(Command.end_game, OnEndGameCommand);

        #endregion

    }

    public override void OnDestroy()
    {
        

        NetworkManager.OnClientConnectedCallback -= OnClientConnected;

        #region CONSOLE_COMMAND_LISTENERS

        ConsoleController.Singleton.StopListening(Command.start_game, OnStartGameCommand);
        ConsoleController.Singleton.StopListening(Command.end_game, OnEndGameCommand);

        #endregion

        base.OnDestroy();
    }
    private void Start()
    {
        if (IsServer)
        {
            StartCoroutine(StartGameAfter(12f));
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            if (gameIsEnded) { return; }

            if(gameIsStarted && pawnList.Count == 1)
            {
                EndGameRpc();
                string n = NetworkController.Singleton.clientData[pawnList[0].GetComponent<NetworkObject>().OwnerClientId].name;
                SetEndGameTextRpc($"{n} has purged the curruption!");
                
            }
        }
    }
    IEnumerator StartGameAfter(float t)
    {
        yield return new WaitForSeconds(t);

        StartGameRpc();
    }

    #region CONSOLE_COMMANDS

    void OnStartGameCommand(string[] args)
    {
        TriggerStartGameRpc();
    }
    void OnEndGameCommand(string[] args)
    {
        TriggerEndGameRpc();
    }

    #endregion

    // trigger start game on server to ensure authority
    [Rpc(SendTo.Server)]
    public void TriggerStartGameRpc()
    {
        if (gameIsStarted)
            return;

        StartGameRpc();
    }

    [Rpc(SendTo.Everyone)]
    void StartGameRpc()
    {
        StartGame?.Invoke();
        gameIsStarted = true;
        start_screen.SetActive(false);
    }
    [Rpc(SendTo.Server)]
    public void TriggerEndGameRpc()
    {
        if (gameIsEnded) return;

        EndGameRpc();
    }

    [Rpc(SendTo.Everyone)]
    void EndGameRpc()
    {
        EndGame?.Invoke();
        gameIsEnded = true;
    }

    [Rpc(SendTo.Everyone)]
    void SetEndGameTextRpc(string txt)
    {
        end_text.text = txt;
        end_screen.SetActive(true);
        Destroy(end_screen, 10f);
        end_screen = null;
    }


    public virtual void OnClientConnected(ulong client)
    {
        if (IsServer)
        {
            SendGameStateRpc(gameIsStarted, gameIsEnded, RpcTarget.Single(client, RpcTargetUse.Temp));
        }
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void SendGameStateRpc(bool gameStarted, bool gameEnded, RpcParams rpcParams)
    {
        gameIsStarted = gameStarted;
        gameIsEnded = gameEnded;

        LateConnectionTrigger?.Invoke();
    }

}
