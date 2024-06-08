using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UIIS;
using Utilities;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class Pawn : NetworkBehaviour, IKillable
{
    public Vector3 initialCameraPosition;
    public MeshRenderer faceRender;
    public Color color;
    public SoundSource sound;

    public override void OnNetworkSpawn()
    {
        GameManager.Singleton.pawnList.Add(GetComponent<NetworkObject>());

        if (IsOwner)
        {

            MouseAim mouseAim = Camera.main.GetComponentInParent<MouseAim>();
            mouseAim.focus = transform;
            mouseAim.TeleportCamera(transform.position);
            mouseAim.cameraPosition = initialCameraPosition;

            NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>().Pawn = gameObject;
            SetFaceRpc(NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>().playerName);
            SetColorRpc(NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>().playerName.GetHashCode());
        }

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        GameManager.Singleton.pawnList.Remove(GetComponent<NetworkObject>());

        base.OnNetworkDespawn();
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Keyboard.current.zKey.wasPressedThisFrame)
        {
            ShiftFaceRpc(0f);
        }
        if (Keyboard.current.zKey.wasReleasedThisFrame)
        {
            ShiftFaceRpc(.5f);
        }

        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            int i = Random.Range(0, SoundBank.Singleton.idleNpcClips.Length);
            PlayTalkingSoundRpc(i);
        }

    }

    [Rpc(SendTo.Everyone)]
    public void SetFaceRpc(string name)
    {
        Random.InitState(name.GetHashCode());
        faceRender.material.mainTextureOffset = new Vector2((float)Random.Range(0, ArtUtils.FaceAtlasCount) / ArtUtils.FaceAtlasCount, .5f);
        Random.InitState(name.GetHashCode());
        sound.audioSource.pitch = Mathf.Pow(2f, Mathf.Pow(Random.Range(-1f, 1f), 3f));
    }
    [Rpc(SendTo.Everyone)]
    public void ShiftFaceRpc(float amount)
    {
        Vector2 offset = faceRender.material.mainTextureOffset;
        offset.y = amount;
        faceRender.material.mainTextureOffset = offset;

    }
    [Rpc(SendTo.Everyone)]
    public void SetColorRpc(int seed)
    {
        Random.InitState(seed);
        color = Random.ColorHSV();
    }
    [Rpc(SendTo.Everyone)]
    public void PlayTalkingSoundRpc(int seed)
    {
        sound.UpdateFilter();
        sound.audioSource.clip = SoundBank.Singleton.idleNpcClips[seed];
        sound.audioSource.Play();
    }

    public void OnDeath()
    {
        
        gameObject.SetActive(false);


        if (IsOwner)
        {
            NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<IKillable>().OnDeath();

            if (TryGetComponent<DeathComponent>(out var deathComponent))
            {
                DeathRpc(deathComponent.flingForce);
            }   

        }

        if (IsServer)
        {
            NetworkObject.Despawn(gameObject);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void DeathRpc(Vector3 force)
    {
        if (IsOwner) return;

        if(TryGetComponent<DeathComponent>(out var deathComponent))
        {
            deathComponent.flingForce = force;
            deathComponent.TriggerDeath(color);
        }
    }

 
}
