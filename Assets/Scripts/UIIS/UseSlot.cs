using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;
using Unity.Netcode;


namespace UIIS
{
    public class UseSlot : NetworkBehaviour
    {
        public Inventory inventory;
        public GameObject item;
        public string itemHashCode;

        public bool isUsable = false;
        public ItemUseInputs _inputs;


        public void OnEquipItem(InventoryItem itemToEquip)
        {
            if (item != null) { return; }

            SpawnItemRpc(itemToEquip.name, NetworkManager.Singleton.LocalClientId);
            itemHashCode = itemToEquip.hashCode;
        }

        public void OnDequip()
        {
            if (item != null)
            {
                if (item.TryGetComponent<Animator>(out var anim))
                {
                    anim.SetTrigger("stow");
                }
                StartCoroutine(DequipRoutine(item));
                isUsable = false;
                item = null;
                _inputs = null;
                
            }
        }

        IEnumerator DequipRoutine(GameObject itemRef)
        {

            float t = 0f;
            while (t < 1f)
            {
                itemRef.transform.position = transform.position;
                itemRef.transform.rotation = transform.rotation;
                t += Time.deltaTime;
                yield return null;
            }

            ulong objectId = itemRef.GetComponent<NetworkObject>().NetworkObjectId;
            DespawnItemRpc(objectId);

        }

        [Rpc(SendTo.Server)]
        private void SpawnItemRpc(string itemName, ulong clientId)
        {
            var itemToSpawn = Instantiate(ItemDatabase.Singleton.prefabs[itemName]);

            itemToSpawn.transform.position = transform.position;
            itemToSpawn.transform.rotation = transform.rotation;

            itemToSpawn.GetComponent<NetworkObject>().SpawnWithOwnership(clientId, true);

            ulong objectId = itemToSpawn.GetComponent<NetworkObject>().NetworkObjectId;
            AssignItemRpc(objectId);

        }

        [Rpc(SendTo.Server)]
        private void DespawnItemRpc(ulong objectId)
        {
            NetworkManager.Singleton.SpawnManager.SpawnedObjects[objectId].Despawn();
        }

        [Rpc(SendTo.Owner)]
        private void AssignItemRpc(ulong objectId)
        {
            item = NetworkManager.Singleton.SpawnManager.SpawnedObjects[objectId].gameObject;

            if (item.TryGetComponent<ItemUseInputs>(out _inputs))
            {
                isUsable = true;
                _inputs.slot = this;
            }
        }

        void OnButtonPress (ButtonControl buttonControl, Iusable usable)
        {
            if (buttonControl.wasPressedThisFrame)
            {
                usable.OnUse();
            }

        }

        private void Update()
        {
            if (!IsOwner)
                return;

            if (item != null)
            {
                item.transform.position = transform.position;
                item.transform.rotation = transform.rotation;
            }

            if (isUsable)
            {
                // check through all inputs and call bindings to Iusables OnUse method

                if (_inputs.leftClickBind != null)
                {
                    OnButtonPress(Mouse.current.leftButton, _inputs.leftClickBind);
                    if (!isUsable) return;

                }
                if (_inputs.rightClickBind != null)
                {
                    OnButtonPress(Mouse.current.rightButton, _inputs.rightClickBind);
                    if (!isUsable) return;
                }

                foreach(var bind in _inputs.keyBindings)
                {
                    OnButtonPress(Keyboard.current[bind.key], bind.usable);
                    if (!isUsable) return;
                }
            }
        }
    }
}

