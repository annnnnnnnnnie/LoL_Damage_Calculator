using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemList : MonoBehaviour
{
    public GameObject itemListPanel;
    public Button btnClose;
    public Button itemButton;
    public Processor processor;
    public List<Button> ExistingButtons;

    private List<ItemListData> itemData;
    

    public void Start()
    {
        itemListPanel.SetActive(false);
        itemData = new List<ItemListData>();
    }

    public void OpenItemList()
    {
        itemListPanel.SetActive(true);
        ExistingButtons = new List<Button>();
        //Debug.Log("-------------------item list opened-------------------");
        itemData = new List<ItemListData>();
        DisplayItems();
    }


    private void DisplayItems()
    {   //Load Data of items

        TextAsset dataAsJson = new TextAsset();
        dataAsJson = Resources.Load<TextAsset>("ItemListJson");
        ItemListDatas allItemListData = new ItemListDatas();
        allItemListData = JsonUtility.FromJson<ItemListDatas>(dataAsJson.text);
        ItemListData[] itemListDatas = allItemListData.itemListDatas;
        
        itemData = new List<ItemListData>();
        foreach(ItemListData itemListData in itemListDatas)
        {
            itemData.Add(itemListData);
        }

        //Debug_ShowItemDatas(itemData);//for debug use only

        //Instantiate a button
 
        foreach (ItemListData thisItemData in itemData)
        {
            GameObject buttonGameObject = GameObjectUtility.CustomInstantiate(itemButton.gameObject, this.transform);
            Button btn = buttonGameObject.GetComponent<Button>();
            btn.GetComponentInChildren<Text>().text = thisItemData.strItemName;
            btn.GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>("ItemIcons/" +thisItemData.strItemName);
            ExistingButtons.Add(btn);

            //Attach appropriate onClick events to the button
            ItemScript itemScript = buttonGameObject.GetComponent<ItemScript>();

            itemScript.Initialize(processor);
            foreach (ItemAttribute itemAttri in thisItemData.itemAttributes)
            {
                itemScript.item.Attributes.Add(itemAttri.key, itemAttri.value);

            }
            itemScript.item.strName = thisItemData.strItemName;

        }
    }

    public void CloseItemList()
    {
        foreach(Button btn in ExistingButtons)
        {
            GameObjectUtility.CustomDestroy(btn.gameObject);
            //Debug.Log("Clearing Existing buttons");
        }
        //Debug.Log("-------------------item list closed-------------------");
        itemListPanel.SetActive(false);
    }

    private void Debug_ShowItemDatas(List<ItemListData> datas)
    {
        foreach(ItemListData thisData in datas)
        {
            Debug.Log(thisData.strItemName);
        }
        for (int i = 0; i < 5; i++)
        {
            Debug.Log("--------------------------");
        }
    }

}
[System.Serializable]
public class ItemListDatas
{
    public ItemListData[] itemListDatas;
}
[System.Serializable]
public class ItemListData
{
    public string strItemName;
    public ItemAttribute[] itemAttributes;
}
[System.Serializable]
public class ItemAttribute
{
    public string key;
    public float value;
    public ItemAttribute(string key, float value)
    {
        this.key = key;
        this.value = value;
    }
}
