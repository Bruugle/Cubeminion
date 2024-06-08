using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;
using UIIS;
using Unity.Networking;
using Unity.Netcode;

public class test2 : NetworkBehaviour, Iusable, Iinteractable
{
    public void OnInteract(GameObject sender)
    {
        InteractionRpc();
    }

    public void OnUse()
    {
        Debug.Log("test2 used");
    }


    [Rpc(SendTo.Everyone)]
    void InteractionRpc()
    {
        Debug.Log("interaction registered");
    }

}
