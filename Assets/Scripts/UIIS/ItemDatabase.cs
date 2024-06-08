using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIIS
{
    public class ItemDatabase : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> itemPrefabList;
        [SerializeField]
        private List<GameObject> dropPrefabList;


        public Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
        public Dictionary<string, GameObject> dropPrefabs = new Dictionary<string, GameObject>();

        public static ItemDatabase Singleton { get; private set; }


        private void Awake()
        {
            if (Singleton == null)
            {
                Singleton = this;
                Init();
            }
            else if (Singleton != this)
            {
                Destroy(gameObject);
            }
        }

        private void Init()
        {
            DontDestroyOnLoad(gameObject);
            foreach (GameObject item in itemPrefabList) { prefabs.Add(item.name, item); }
            foreach(GameObject drop in dropPrefabList) { dropPrefabs.Add(drop.GetComponent<Item>().itemName, drop); }
        }
    }

}

