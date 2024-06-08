using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIIS;
using Unity.Netcode;

public class Bandage : NetworkBehaviour, Iusable
{

    public SoundSource sound;

    public void OnUse()
    {
        var inputs = GetComponent<ItemUseInputs>();

        inputs.slot.inventory.gameObject.GetComponent<PlayerPawnHealth>().isBleeding = false;
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
