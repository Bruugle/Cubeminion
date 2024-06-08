using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace UIIS
{
    
    public class Item : NetworkBehaviour, Iinteractable
    {
        /// <summary>
        /// an item is any object in the game that can be picked up, dropped, and put in inventories
        /// always put at the top level of an item gameobject's hierarchy
        /// </summary>
        /// 
        public string itemName;


        public void OnInteract(GameObject sender) 
        {

            Inventory _inventory;
            if (sender.TryGetComponent<Inventory>(out _inventory))
            {
                // add the item to the inventory contents
                if (_inventory.contents.Count >= 9) return;
                _inventory.Add(itemName);

                // network response
                
                ItemInteractionRpc(NetworkManager.Singleton.LocalClientId);
                
            }
        }

        [Rpc(SendTo.Server)]
        void ItemInteractionRpc(ulong clientId)
        {

            NetworkObject.Despawn();
        }



    }
}


