using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour {
    public Button prefabItemSlotButton;
    [HideInInspector]
    public List<ItemSlot> itemSlots;
    public bool isEnemy = false;
    public void Start()
    {
        itemSlots = new List<ItemSlot>();

        for (int i = 0; i < 6; i++)
        {
            Button btn = Instantiate(prefabItemSlotButton, this.transform);
            ItemSlot itemSlot = btn.GetComponent<ItemSlot>();
            itemSlot.Initialize(i, isEnemy);
            itemSlots.Add(itemSlot);
        }
    }
}
