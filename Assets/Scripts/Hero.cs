using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public abstract class Hero {
    //Under development, use Hero_Base for now
    public string strName;
    public float fLevel;

    public HeroInfo heroInfo;
    public Rune rune;
    public List<Item> items = new List<Item>();
    public Dictionary<string, float> Attributes;

    public float fCurrentHealth;
    public List<Buff> buffs;

    protected int intTime;// 1=10ms
    protected static readonly int updateInterval = 10;

    public abstract int GetHeroLevel();
    public abstract Rune GetRune();
    public abstract Dictionary<string, float> CalculateBaseAttributes();

    public void Refresh()
    {

    }
    public void Initialize()
    {
        heroInfo = GameObject.Find(strName + "Info").GetComponent<HeroInfo>();
        items = GetItems();
    }
    private List<Item> GetItems()
    {
        return heroInfo.inventory.GetItems();
    }

    public Dictionary<string, float> CalculateItemAttributes()
    {

        items = GetItems();
        Attributes = new Dictionary<string, float>();
        StringBuilder debugMsg = new StringBuilder();
        foreach (Item item in items)
        {
            if (item != null && item.strName != null)
            {
                debugMsg.Append("Calculating item attribute: " + item.strName + "\n");
                foreach (KeyValuePair<string, float> attribute in item.Attributes)
                {
                    if (this.Attributes.ContainsKey(attribute.Key))
                    {
                        this.Attributes[attribute.Key] += attribute.Value;
                    }
                    else
                    {
                        this.Attributes.Add(attribute.Key, attribute.Value);
                    }
                    debugMsg.Append("Current " + attribute.Key + " is " + this.Attributes[attribute.Key] + "\n");
                }
            }
            else
            {
                debugMsg.Append("Current itemSlot is empty\n");
            }
        }
        Debug.Log(debugMsg.ToString());
        return Attributes;
    }

    public Dictionary<string, float> CalculateAttributes()
    {
        Dictionary<string, float> tempDic = GameStatsUtility.CombineAttributes(CalculateBaseAttributes(), CalculateItemAttributes());

        return GameStatsUtility.CalculateEffectiveAttributes(tempDic);
    }

    public abstract void Update(SpellCast spellCast = null);

}
