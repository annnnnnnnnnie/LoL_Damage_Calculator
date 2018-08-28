using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;

public class ItemScript : MonoBehaviour, IRecycle
{
    public Item item;
    public ItemSlot itemSlot { get; set; }
    public void Initialize()
    {
        item = new Item();
        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => { EquipItem(item); });
    }

    private void EquipItem(Item item)
    {
        Processor processor = GetComponentInParent<Processor>();
        processor.EquipItem(item);
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

    [HideInInspector]
    public List<string> Actives;

    [HideInInspector]
    public List<string> Extras;

    [HideInInspector]
    public int intItemNumber;

    public Item MakeCopy()
    {
        return (new Item { strName = this.strName, Attributes = this.Attributes,
            Actives = this.Actives, Extras = this.Extras, intItemNumber = this.intItemNumber });
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