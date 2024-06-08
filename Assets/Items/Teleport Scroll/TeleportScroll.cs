using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIIS;
using Networking;

public class TeleportScroll : MonoBehaviour, Iusable
{
    public void OnUse()
    {
        Vector3 tp_position;
        int layerMask = 1 << 6;
        RaycastHit _hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out _hit, 250f, layerMask))
        {
            tp_position = new Vector3(_hit.point.x, 0f, _hit.point.z);
        }
        else
        {
            tp_position = Vector3.ProjectOnPlane(transform.position, Vector3.up) + Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up) * 250f;
        }

        var pos = GetComponent<ItemUseInputs>().slot.inventory.transform.position;
        var rot = GetComponent<ItemUseInputs>().slot.inventory.transform.rotation;
        var c = GetComponent<ItemUseInputs>().slot.inventory.GetComponent<Pawn>().color;
        GetComponent<ItemUseInputs>().slot.inventory.GetComponent<ClientNetworkTransform>().Teleport(tp_position, rot, Vector3.one);
        EffectsManager.Singleton.SpawnTeleportSmoke(tp_position, c);
        EffectsManager.Singleton.SpawnTeleportSmoke(pos, c);

        GetComponent<Consumable>().Consume();

    }
}
