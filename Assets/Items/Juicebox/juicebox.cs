using System.Collections;
using System.Collections.Generic;
using UIIS;
using Unity.Netcode;
using UnityEngine;

public class juicebox : NetworkBehaviour, Iusable
{
    public SoundSource sound;
    float speedMultiply = 3f;
    float forTime = 15f;

    public void OnUse()
    {
        var inputs = GetComponent<ItemUseInputs>();

        inputs.slot.inventory.gameObject.GetComponent<PawnMovement>().IncreaseRunspeedForSeconds(speedMultiply, forTime);
        PlaySoundRpc();
        GetComponent<Consumable>().Consume();
    }

    [Rpc(SendTo.Everyone)]
    void PlaySoundRpc()
    {
        sound.UpdateFilter();
        sound.audioSource.Play();
    }
}
