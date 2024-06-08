using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UIIS;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class Mortar : NetworkBehaviour, Iusable
{

    public float angle = 60f;
    public GameObject mesh;
    public GameObject shell;
    public SoundSource sound;
    float timer = 5f;

    public void OnUse()
    {
        if (timer > 0) return;

        SpawnShellRpc();
        timer = 5f;
        PlaySoundRpc();
    }

    [Rpc(SendTo.Server)]
    void SpawnShellRpc()
    {
        Vector3 dir = Mathf.Cos(Mathf.Deg2Rad * angle) * transform.forward + Mathf.Sin(Mathf.Deg2Rad * angle) * transform.up;
        var go = Instantiate(shell, transform.position + .8f * dir, Quaternion.LookRotation(dir));
        go.GetComponent<NetworkObject>().Spawn();
        go.GetComponent<Rigidbody>().AddForce(2000f * dir);
        
    }

    [Rpc(SendTo.Everyone)]
    void PlaySoundRpc()
    {
        sound.UpdateFilter();
        sound.audioSource.Play();
    }


    private void Update()
    {
        if(!IsOwner) return;

        Vector2 vec = Mouse.current.scroll.ReadValue();

        var nextAngle = Mathf.Clamp(angle + .1f * Time.deltaTime * vec.y, 45f, 90f);

        mesh.transform.Rotate(angle - nextAngle, 0f, 0f);

        angle = nextAngle;

        timer -= Time.deltaTime;



    }

}
