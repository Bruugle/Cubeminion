using System.Collections;
using System.Collections.Generic;
using UIIS;
using UnityEngine;

public class LobbySettings : MonoBehaviour
{
    public int itemSpawnCount;
    public float rareItemDropCount;
    public int cubeCount;
    public float spawnRadius;

    public static LobbySettings Singleton { get; private set; }


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

    private void Init()
    {
        DontDestroyOnLoad(gameObject);
    }
}
