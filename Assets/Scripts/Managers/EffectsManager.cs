using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class EffectsManager : NetworkBehaviour
{
    public GameObject GunSmokePrefab;
    public GameObject ExplosionPrefab;
    public GameObject SmokePrefab;
    public GameObject TeleportSmokePrefab;

    public static EffectsManager Singleton { get; private set; }

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

    public void SpawnGunSmoke(Vector3 position, Quaternion rotation)
    {
        SpawnGunSmokeRpc(position, rotation);
    }
    public void SpawnExplosion(Vector3 Position, Vector3 scale)
    {
        SpawnExplosionRpc(Position, scale);
    }
    public void SpawnSmoke(Vector3 Position, Color color)
    {
        SpawnSmokeRpc(Position, color.r, color.g, color.b);
    }
    public void SpawnTeleportSmoke(Vector3 Position, Color color)
    {
        SpawnTeleportSmokeRpc(Position, color.r, color.g, color.b);
    }

    [Rpc(SendTo.Everyone)]
    void SpawnGunSmokeRpc(Vector3 position, Quaternion rotation)
    {
        var go = Instantiate(GunSmokePrefab, position, rotation, transform);
        Destroy(go, 30f);
    }
    [Rpc(SendTo.Everyone)]
    void SpawnExplosionRpc(Vector3 Position, Vector3 scale)
    {
        var go = Instantiate(ExplosionPrefab, Position, Quaternion.identity, transform);
        go.transform.localScale = scale;
        Destroy(go, 30f);
    }
    [Rpc(SendTo.Everyone)]
    void SpawnSmokeRpc(Vector3 Position, float r, float g, float b)
    {
        var go = Instantiate(SmokePrefab, Position, Quaternion.identity, transform);
        go.GetComponent<VisualEffect>().SetVector4("color", new Color(r, g, b));
        Destroy(go, 60f);
    }
    [Rpc(SendTo.Everyone)]
    void SpawnTeleportSmokeRpc(Vector3 Position, float r, float g, float b)
    {
        var go = Instantiate(TeleportSmokePrefab, Position, Quaternion.identity, transform);
        go.GetComponent<VisualEffect>().SetVector4("color", new Color(r, g, b));
        Destroy(go, 30f);
    }

}
