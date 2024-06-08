using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class ArmedC4 : NetworkBehaviour
{
    public SoundSource sound;

    private void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            if (IsOwner)
            {
                PlaySoundRpc();
                StartCoroutine(ExplodeAfter(.62f));
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    void PlaySoundRpc()
    {
        sound.UpdateFilter();
        sound.audioSource.Play();
    }


    IEnumerator ExplodeAfter(float t)
    {
        yield return new WaitForSeconds(t);

        GetComponent<Explosive>().Explode();

    }
}
