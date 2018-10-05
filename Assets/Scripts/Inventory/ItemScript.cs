using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;

public class ItemScript : MonoBehaviour, IRecycle //Item Displayed in ItemList
{
    public Item item;
    public ItemSlot itemSlot { get; set; }
    private Processor processor;
    public void Initialize(Processor processor)
    {
        this.processor = processor;
        item = new Item();
        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => { EquipItem(item); });
    }

    private void EquipItem(Item item)
    {
        processor.EquipItem(item);
    }

    public void AddAttribute(string attriName, float value)
    {
        item.AddAttribute(attriName, value);
    }

    public void AddExtra(string extraName, int value)
    {
        item.AddExtra(extraName, value);
    }

    public void Shutdown()
    {

    }

    public void Restart()
    {

    }
}

public class Item
{
    [HideInInspector]
    public string strName;
    [HideInInspector]
    public Dictionary<string,float> Attributes = new Dictionary<string,float>();

    //[HideInInspector]
    //public List<string> Actives;

    //[HideInInspector]
    public Dictionary<string, int> Extras = new Dictionary<string, int>();

    [HideInInspector]
    public int intItemNumber;

    public void AddAttribute(string attriName, float value)
    {
        Attributes.Add(attriName, value);
    }
    public void AddExtra(string extraName, int value)
    {
        Extras.Add(extraName, value);
    }

    public override bool Equals(object obj)
    {
        var item = obj as Item;
        return item != null &&
               strName == item.strName;
    }

    public override int GetHashCode()
    {
        return 1581483051 + EqualityComparer<string>.Default.GetHashCode(strName);
    }

    public Item MakeCopy()
    {
        return (new Item {
            strName = strName,
            Attributes = Attributes,
            Extras = Extras, 
            intItemNumber = intItemNumber
        });
    }
     
}



public class Rune
{
    public List<string> strStones;
    public new string ToString()
    {
        StringBuilder str = new StringBuilder("Rune: \n");
        foreach(string thisRune in strStones)
        {
            str.Append(thisRune + " \n");
        }
        return str.ToString();
    }
}

public class Extra
{
    public Rune rune;
    public Shield shield;
    public Halo halo;
}

public class Shield
{

}
public class Halo
{

}