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
    public Hero owner;
    private Processor processor;
    [HideInInspector]
    public Item item;

    private bool onClickResolved;

    private Text descriptionText;

    public void Initialize(int itemSlotNumber, Processor processor, Hero owner)
    {
        this.itemSlotNumber = itemSlotNumber;
        this.processor = processor;
        this.owner = owner;

        itemButton = GetComponent<Button>();

        descriptionText = itemButton.GetComponentInChildren<Text>();
        descriptionText.text = "itemSlot " + itemSlotNumber;
        originalSprite = GetComponentInChildren<Image>().sprite;

        item = null;
    }


    public void HandlePointerDown()
    {
        Debug.Log("Item " + itemSlotNumber + " is selected");
        StopAllCoroutines();
        StartCoroutine(Press());
    }

    public void HandlePointerUp(bool held = false)
    {
        StopAllCoroutines();
        if (held)
        {
            HandlePressAndHold();
            onClickResolved = true;
        }
        else if(!onClickResolved)
        {
            processor.OpenItemList(this);//Tell the processor to open itemList, with a referrence of itemSlot
        }
    }
    public void HandlePressAndHold()
    {
        if (item!= null && item.Extras.Count > 0)
        {
            Debug.Log("Opening Advanced Setting Tab");
            processor.OpenAdvanceSetting(item);//Tell the processor to open advanceSetting
        }
    }

    private IEnumerator Press()
    {
        onClickResolved = false;
        for(int i = 0; i < 50; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        if(!onClickResolved)
        {
            HandlePointerUp(true);
        }
    }

    public void EquipItem(Item item)
    {
        this.item = item.MakeCopy();//deep copying
        Debug.Log("Item equipped: " + this.item.strName);

        foreach (KeyValuePair<string, float> currentPair in this.item.Attributes)
        {
            Debug.Log("Item Attributes: " + currentPair.Key + " = " + currentPair.Value);
        }

        foreach(KeyValuePair<string, int> currentPair in this.item.Extras)
        {
            Debug.Log("Item Extras: " + currentPair.Key + " = " + currentPair.Value);
        }

        descriptionText.text = this.item.strName;
        
        GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>("ItemIcons/" + this.item.strName);
    }

    public Item UnequipItem()
    {
        Item unequippedItem;
        if (item != null && item.strName != null)
        {
            Debug.Log("Item unequipping: " + item.strName);
            unequippedItem = item.MakeCopy();
            item = null;
        }
        else
        {
            unequippedItem = null;
            Debug.Log("Already an empty itemSlot");
        }

        descriptionText.text = "itemSlot " + itemSlotNumber;
        GetComponentInChildren<Image>().sprite = originalSprite;
        return unequippedItem;
    }

    public Item GetItem()
    {
        return item;
    }
}