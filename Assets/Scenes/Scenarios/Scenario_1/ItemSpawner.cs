using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Utilities.DeveloperConsole;
using System;

public class ItemSpawner : NetworkBehaviour
{
    public int initialItemCount;
    public float itemDropRadius;

    public float rareDropRate;

    public GameObject dropCratePrefab;
    public List<ItemDrop> dropList = new List<ItemDrop>();
    public List<ItemDrop> rareList = new List<ItemDrop>();
    Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
    
    void Start()
    {
        ConsoleController.Singleton.StartListening(Command.spawn_item, ConsoleSpawnItem);

        foreach (ItemDrop item in dropList) { prefabs.Add(item.name, item.prefab); }

        initialItemCount = LobbySettings.Singleton.itemSpawnCount;
        rareDropRate = LobbySettings.Singleton.rareItemDropCount;

        GameSetUp();
    }

    public override void OnDestroy()
    {

        ConsoleController.Singleton.StopListening(Command.spawn_item, ConsoleSpawnItem);
        base.OnDestroy();
    }

    void GameSetUp()
    {
        if (!IsServer) return;

        itemDropRadius = Mathf.Clamp(2 * LobbySettings.Singleton.spawnRadius, 0, 500f);

        int totalItems = 0;
        int i = 0;

        UnityEngine.Random.InitState(Time.frameCount);

        while (totalItems < initialItemCount)
        {
            if (UnityEngine.Random.value * 100f < dropList[i].dropChance)
            {
                Vector3 pos = itemDropRadius * UnityEngine.Random.insideUnitSphere;
                pos.y = 0;
                SpawnItem(dropList[i].name, pos, Quaternion.identity);
                totalItems++;
            }

            i = (i + 1) % dropList.Count;
        }

        if (rareDropRate > 0 && rareList.Count > 0)
        {
            StartCoroutine(RareDropRoutine(2f * 60f / rareDropRate));
        }

    }

    void ConsoleSpawnItem(string[] args)
    {
        if (args.Length == 0) return;
        SpawnItemRpc(string.Join(" ", args));
    }

    [Rpc(SendTo.Server)]
    void SpawnItemRpc(string item)
    {
        if (!prefabs.ContainsKey(item)) return;

        SpawnItem(item, Camera.main.transform.position - new Vector3(0, Camera.main.transform.position.y, 0), Quaternion.identity);
    }

    void SpawnItem(string itemName, Vector3 position, Quaternion rotation)
    {
        var go = Instantiate(prefabs[itemName], position, rotation);
        go.GetComponent<NetworkObject>().Spawn(true);
    }

    IEnumerator RareDropRoutine(float t)
    {
        yield return new WaitForSeconds(t);

        Vector3 pos = itemDropRadius * UnityEngine.Random.insideUnitSphere;
        pos.y = 0;
        RareDropRpc(pos);

        StartCoroutine(RareDropRoutine(t));
    }

    [Rpc(SendTo.Everyone)]
    void RareDropRpc(Vector3 position)
    {
        StartCoroutine(RareDrop(position));
    }

    IEnumerator RareDrop(Vector3 position)
    {
        float t = 0f;
        Vector3 initPos = position + Vector3.up * 240f;
        var go = Instantiate(dropCratePrefab, position, Quaternion.identity);
        
        Transform dropCrate = go.transform;

        while(t < 120f)
        {
            t += Time.deltaTime;

            dropCrate.position = Vector3.Lerp(initPos, position, t / 120f);

            yield return null;
        }

        Destroy(go);

        if (IsServer)
        {
            ItemDrop drop = rareList[UnityEngine.Random.Range(0, rareList.Count)];
            SpawnItem(drop.name, position, Quaternion.identity);
        }
    }

    [Serializable]
    public struct ItemDrop
    {
        public string name;
        public GameObject prefab;
        public float dropChance;

        public ItemDrop(string itemName, GameObject itemPrefab, float itemDropChance)
        {
            name = itemName;
            prefab = itemPrefab;
            dropChance = itemDropChance;
        }
    }
}
