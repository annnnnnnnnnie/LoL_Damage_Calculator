using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour
{
    private Button itemButton;
    private Sprite originalSprite;

    public int itemSlotNumber;
    [HideInInspector]
    public Item item;

    private bool isEnemy = false;


    private Text descriptionText;

    public void Initialize(int itemSlotNumber, bool isEnemy)
    {
        this.itemSlotNumber = itemSlotNumber;
        this.isEnemy = isEnemy;

        itemButton = GetComponent<Button>();
        itemButton.onClick.RemoveAllListeners();
        itemButton.onClick.AddListener(() =>
        {
            HandleOnClick();
        });

        descriptionText = itemButton.GetComponentInChildren<Text>();
        descriptionText.text = "itemSlot " + itemSlotNumber;
    }


    public void HandleOnClick()
    {
        Debug.Log("Item " + itemSlotNumber + " is selected");

        SendMessageUpwards("OpenItemList", this);//Tell the processor to open itemList, with a referrence of itemSlot
    }

    public void EquipItem(Item item)
    {
        this.item = item.MakeCopy();//deep copying
        Debug.Log("Item equipped: " + this.item.strName);

        foreach (KeyValuePair<string, float> currentPair in this.item.Attributes)
        {
            Debug.Log("Item Attributes: " + currentPair.Key + " = " + currentPair.Value);
        }

        descriptionText.text = this.item.strName;
        originalSprite = GetComponentInChildren<Image>().sprite;
        GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>("ItemIcons/" + this.item.strName);
    }

    public void UnequipItem()
    {
        if (item != null && item.strName != null)
        {
            Debug.Log("Item unequipped: " + item.strName);
            item = new Item();
        }
        else
        {
            Debug.Log("Already an empty itemSlot");
        }

        descriptionText.text = "itemSlot " + itemSlotNumber;
        GetComponentInChildren<Image>().sprite = originalSprite;
    }

    public Item GetItem()
    {
        return item;
    }
}