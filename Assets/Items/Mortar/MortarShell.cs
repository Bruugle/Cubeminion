using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MortarShell : NetworkBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (IsServer)
        {
            GetComponent<Explosive>().Explode();
        }
    }
}
