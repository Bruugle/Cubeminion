using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class SpectatorManager : MonoBehaviour
{
    public bool IsSpectating;
    public int capturedSoulCount;
    public int summonCost = 1;

    public AudioClip captureClip;
    public AudioClip summonClip;
    public GrandCube overCube;
    public GameObject soulPrefab;
    public PlayerPawnSpawner spawner;
    public GameObject spectator;
    public Action SpectatorAdded;
    public Action SpectatorRemoved;


    public static SpectatorManager Singleton { get; private set; }

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

    void Init()
    {

    }

    public void RespawnRequest()
    {
        IsSpectating = false;
        SpectatorRemoved?.Invoke();
        spawner.RespawnRpc(NetworkManager.Singleton.LocalClientId);
        Destroy(spectator);
        summonCost = 1;
    }

    public void SpawnSoul(Vector3 spawnPosition)
    {
        var go = Instantiate(soulPrefab, spawnPosition, UnityEngine.Random.rotation, transform);
        go.GetComponent<Soul>().StartListening();

        if (IsSpectating) go.SetActive(true);
    }
}
