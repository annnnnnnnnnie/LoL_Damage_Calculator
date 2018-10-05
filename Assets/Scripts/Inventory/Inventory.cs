using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour {

    public Button prefabItemSlotButton;
    [HideInInspector]
    public List<ItemSlot> itemSlots;

    public void Initialize(Processor processor, Hero owner)
    {
        itemSlots = new List<ItemSlot>();

        for (int i = 0; i < 6; i++)
        {
            Button btn = Instantiate(prefabItemSlotButton, this.transform);
            ItemSlot itemSlot = btn.GetComponent<ItemSlot>();
            itemSlot.Initialize(i, processor, owner);
            itemSlots.Add(itemSlot);
        }
    }

    public List<Item> GetItems()
    {
        List<Item> items = new List<Item>();
        for(int i = 0; i < 6; i++)
        {
            items.Add(itemSlots[i].item);
        }
        return items;
    }
}
