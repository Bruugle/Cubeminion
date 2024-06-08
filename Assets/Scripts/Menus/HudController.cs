using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UIIS;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HudController : NetworkBehaviour
{
    public PlayerPawnHealth health;
    public Inventory inventory;
    public UseSlot slot;
    public RectTransform selectionOrb;
    public Text inv_text;
    public Text blood_text;
    public InteractiveRaycast intercast;
    public Text interaction_text;
    public int selection = 1;

    List<int> openPositions = new List<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
    Dictionary<int, string> map = new();
    Dictionary<string, int> Imap = new();


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            gameObject.SetActive(false);
        }
        else
        {
            inventory.ItemAdded += OnItemAdded;
            inventory.ItemRemoved += OnItemRemoved;
        }
    }

    public override void OnDestroy()
    {
        if (IsOwner)
        {
            inventory.ItemAdded -= OnItemAdded;
            inventory.ItemRemoved -= OnItemRemoved;
        }

        base.OnDestroy();
    }

    private void Update()
    {
        if (!IsOwner) return;

        UpdateBlood();

        interaction_text.text = intercast.promptMessage;

        if (Keyboard.current.digit1Key.wasPressedThisFrame) { OnSelection(1); return; }
        if (Keyboard.current.digit2Key.wasPressedThisFrame) { OnSelection(2); return; }
        if (Keyboard.current.digit3Key.wasPressedThisFrame) { OnSelection(3); return; }
        if (Keyboard.current.digit4Key.wasPressedThisFrame) { OnSelection(4); return; }
        if (Keyboard.current.digit5Key.wasPressedThisFrame) { OnSelection(5); return; }
        if (Keyboard.current.digit6Key.wasPressedThisFrame) { OnSelection(6); return; }
        if (Keyboard.current.digit7Key.wasPressedThisFrame) { OnSelection(7); return; }
        if (Keyboard.current.digit8Key.wasPressedThisFrame) { OnSelection(8); return; }
        if (Keyboard.current.digit9Key.wasPressedThisFrame) { OnSelection(9); return; }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (slot.item == null && map.ContainsKey(selection))
            {
                slot.OnEquipItem(inventory.contents[map[selection]]);
            }
            else
            {
                slot.OnDequip();
            }
            return;
        }

        if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            DropItem();
        }

    }

    void OnSelection(int index)
    {
        selectionOrb.anchoredPosition = new Vector2(0, -16 * index);
        selection = index;

        if (slot.item == null && map.ContainsKey(selection))
        {
            slot.OnEquipItem(inventory.contents[map[selection]]);
        }
        else if (slot.item != null && map.ContainsKey(selection))
        {
            slot.OnDequip();
            slot.OnEquipItem(inventory.contents[map[selection]]);
        }
        else
        {
            slot.OnDequip();
        }
    }

    void OnItemAdded(string hashCode)
    {
        if (openPositions.Count == 0) return;

        openPositions.Sort();
        int position = openPositions[0];
        openPositions.RemoveAt(0);
        map.Add(position, hashCode);
        Imap.Add(hashCode, position);
        OnSelection(position);
        UpdateText();
    }

    void OnItemRemoved(string hashCode)
    {
        openPositions.Add(Imap[hashCode]);
        map.Remove(Imap[hashCode]);
        Imap.Remove(hashCode);
        UpdateText();
    }

    void DropItem()
    {

        if (openPositions.Contains(selection)) return;

        slot.OnDequip();
        SpawnItemRpc(inventory.contents[map[selection]].name);

        inventory.Remove(map[selection]);
    }

    void UpdateText()
    {
        string text = "";
        for (int i = 1; i < 10; i++)
        {
            text += $"{i}. ";
            if (map.ContainsKey(i))
            {
                text += inventory.contents[map[i]].name;
            }
            text += "\n";
        }

        inv_text.text = text;
    }

    [Rpc(SendTo.Server)]
    private void SpawnItemRpc(string itemName)
    {
        var itemToSpawn = Instantiate(ItemDatabase.Singleton.dropPrefabs[itemName]);

        itemToSpawn.transform.position = transform.position;
        itemToSpawn.transform.rotation = transform.rotation;

        itemToSpawn.GetComponent<NetworkObject>().Spawn(true);
    }

    void UpdateBlood()
    {
        blood_text.text = "Blood: " + ((int)health.blood).ToString();
    }
}
