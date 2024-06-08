using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIIS
{
    public class Inventory : MonoBehaviour
    {
        public Dictionary<string, InventoryItem> contents = new();
        public Action<string> ItemAdded;
        public Action<string> ItemRemoved;

        public void Add(string itemName)
        {
            string hashCode = Guid.NewGuid().ToString();
            InventoryItem entry = new InventoryItem(itemName, hashCode, ItemDatabase.Singleton.prefabs[itemName]);
            contents.Add(hashCode, entry);
            ItemAdded?.Invoke(hashCode);
        }

        public void Remove(string hashCode)
        {
            contents.Remove(hashCode);
            ItemRemoved?.Invoke(hashCode);
        }

    }

    [Serializable]
    public struct InventoryItem
    {
        public string name;
        public string hashCode;
        public GameObject prefab;

        public InventoryItem(string itemName, string hash, GameObject itemPrefab)
        {
            name = itemName;
            hashCode = hash;
            prefab = itemPrefab;
        }
    }
}


