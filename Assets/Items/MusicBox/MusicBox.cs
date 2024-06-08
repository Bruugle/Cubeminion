using Networking;
using System.Collections;
using System.Collections.Generic;
using UIIS;
using Unity.Netcode;
using UnityEngine;

public class MusicBox : NetworkBehaviour, Iinteractable
{

    public SoundSource music;

    public Transform tape1;
    public Transform tape2;

    private void Awake()
    {
        int seed = 1234;
        if (NetworkController.Singleton.gameJoinCode != null)
            seed = NetworkController.Singleton.gameJoinCode.GetHashCode();
        Random.InitState(seed);
        music.audioSource.timeSamples = Random.Range(0, music.audioSource.clip.samples);
    }

    public void OnInteract(GameObject sender)
    {
        ToggleMusicRpc(!music.audioSource.mute);
    }

    [Rpc(SendTo.Everyone)]
    private void ToggleMusicRpc(bool state)
    {
        music.audioSource.mute = state;
        GameManager.Singleton.musicPlaying = !state;
    }

    private void Update()
    {
        if (!music.audioSource.mute)
        {
            tape1.Rotate(0, 30f * Time.deltaTime, 0);
            tape2.Rotate(0, 30f * Time.deltaTime, 0);
        }
    }

}
