using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Unity.Netcode;
using Utilities.DeveloperConsole;



public class Herd : NetworkBehaviour
{
    public float spawnRadius;
    public int popToSpawn;
    public GameObject prefab;
    public float maxSpawnRatePerSec = 100f;
    public int population = 0;
    public float hashWidth = 10f;

    public int currentMessagesPerSecond = 0;
    public int peakMessagesPerSecond = 0;
    public int messagesPerSecondRunningAverage = 0;

    [HideInInspector]
    public int pendingMessages = 0;

    int total = 0;
    int elapsedSeconds = 0;

    ulong initId = 0;

    public Dictionary<ulong, HerdBehaviour> members = new();
    public Dictionary<ulong, List<ulong>> hashGrid = new();


    // Start is called before the first frame update

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += SpawnMembersOnClient;
        }
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= SpawnMembersOnClient;
        }
    }
    void Start()
    {
        #region CONSOLE_COMMAND_LISTENERS

        ConsoleController.Singleton.StartListening(Command.set_herd_pop, OnSetHerdPop);

        #endregion

        if (!IsServer)
            return;

        StartCoroutine(ProcessMessages());


        popToSpawn = LobbySettings.Singleton.cubeCount;
        spawnRadius = LobbySettings.Singleton.spawnRadius;

        for (int i = 0; i < popToSpawn; i++)
        {
            initId++;
            Vector2 coords = spawnRadius * Random.insideUnitCircle;
            Vector3 position = new Vector3(coords.x, 0f, coords.y);

            StartCoroutine(SpawnAfter(i/100f, position, initId, Random.Range(0, int.MaxValue)));
        }
    }

    #region CONSOLE_COMMANDS

    void OnSetHerdPop(string[] args)
    {
        if (!IsServer) return;

        if (int.TryParse(args[0], out int pop))
        {
            for (int i = 0; i < pop; i++)
            {
                initId++;
                Vector2 coords = spawnRadius * Random.insideUnitCircle;
                Vector3 position = new Vector3(coords.x, 0f, coords.y);

                StartCoroutine(SpawnAfter(i / 100f, position, initId, Random.Range(0, int.MaxValue)));
            }
        }
    }

    #endregion

    private void Update()
    {

        foreach(HerdBehaviour member in members.Values)
        {
            member.HerdUpdate();
        }
    }

    public void SendTask(bool hasAuthority, ulong herdId, Vector3 prevPosition, Vector3 currentTarget, float taskTime, int rnd, int state, float args)
    {
        pendingMessages++;
        if (IsServer || hasAuthority)
            SendTaskRpc(herdId, prevPosition, currentTarget, taskTime, rnd, state, args);
    }

    [Rpc(SendTo.NotMe)]
    void SendTaskRpc(ulong herdId, Vector3 prevPosition, Vector3 currentTarget, float taskTime, int rnd, int state, float args)
    {
        if (!members.ContainsKey(herdId))
            return;
        members[herdId].Rectify(prevPosition, currentTarget, taskTime, rnd, state, args);
        
    }



    [Rpc(SendTo.SpecifiedInParams)]
    void SpawnOnClientRpc(Vector3 position, Quaternion rotation, ulong herdId, Vector3 currentTarget, float taskTime, int rnd, int state, RpcParams rpcParams)
    {
        var instance = Instantiate(prefab, position, rotation, transform);
        var agent = instance.GetComponent<HerdBehaviour>();
        agent.id = herdId;
        agent.herd = this;
        agent.previousPosition = position;
        agent.targetPosition = currentTarget;
        agent.timeToCompleteTask = taskTime;
        agent.rndState = rnd;
        agent.State = state;

        members.Add(agent.id, agent);

        agent.HashGridQ();
        population++;
    }

    [Rpc(SendTo.Everyone)]
    void SpawnNewOnEveryoneRpc(Vector3 position, ulong initId, int rnd)
    {
        if (members.ContainsKey(initId))
            return;
        var instance = Instantiate(prefab, position, Quaternion.identity, transform);
        var agent = instance.GetComponent<HerdBehaviour>();
        agent.id = initId;
        agent.herd = this;
        agent.previousPosition = position;
        agent.rndState = rnd;

        members.Add(agent.id, agent);

        agent.HashGridQ();

        population++;
    }

    // for spawning on late joiners
    void SpawnMembersOnClient(ulong clientId)
    {

        int i = 0;
        foreach (var member in members.Keys) 
        {
            StartCoroutine(SpawnOnClientAfter(clientId, member, i / maxSpawnRatePerSec));
            i++;
        }
    }

    [Rpc(SendTo.Everyone)]
    public void MemberDeathRpc(ulong id, Vector3 flingForce)
    {
        if (members.TryGetValue(id, out var agent))
        {
            // remove from the hash grid

            if (hashGrid.TryGetValue(agent.hashKey, out var hash))
            {
                hash.Remove(id);
            }
            
            // remove from member dict

            members.Remove(id);

            // trigger death event on herdbeaviour

            agent.Death?.Invoke(flingForce);

            

            population--;
        }
    }



    IEnumerator ProcessMessages()
    {
        yield return new WaitForSeconds(1f);

        elapsedSeconds++;

        total += pendingMessages;
        currentMessagesPerSecond = pendingMessages;


        if(pendingMessages > peakMessagesPerSecond)
            peakMessagesPerSecond = pendingMessages;

        messagesPerSecondRunningAverage = total / elapsedSeconds;

        pendingMessages = 0;

        StartCoroutine(ProcessMessages());
    }


    IEnumerator SpawnAfter(float seconds, Vector3 position, ulong initId, int rnd)
    {
        yield return new WaitForSeconds(seconds);

        SpawnNewOnEveryoneRpc(position, initId, rnd);

    }

    IEnumerator SpawnOnClientAfter(ulong clientId, ulong herdId, float seconds) 
    {
        yield return new WaitForSeconds(seconds);

        if (members.TryGetValue(herdId, out var agent))
        {

            SpawnOnClientRpc(
                agent.transform.position,
                agent.transform.rotation,
                herdId,
                agent.targetPosition,
                agent.timeToCompleteTask - agent.timeSinceTaskStart,
                agent.rndState,
                agent.State,
                RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }

    }


}


