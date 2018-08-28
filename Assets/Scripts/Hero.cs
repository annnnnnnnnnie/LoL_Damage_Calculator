using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public abstract class Hero {

    public string strName;
    public float fLevel;

    public Rune rune;
    public List<Item> items = new List<Item>();
    public Dictionary<string, float> Attributes;

    public float fCurrentHealth;
    public Buff buff;

    public abstract List<Item> GetItems();
    public abstract int GetHeroLevel();
    public abstract Rune GetRune();
    public abstract Dictionary<string, float> CalculateBaseAttributes();

    public Dictionary<string, float> CalculateItemAttributes()
    {

        items = GetItems();
        Attributes = new Dictionary<string, float>();
        StringBuilder debugMsg = new StringBuilder();
        foreach (Item item in items)
        {
            if (item != null && item.strName != null)
            {
                debugMsg.Append("Calculating item attribute: " + item.strName +"\n");
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

    public Dictionary<string,float> CalculateAttributes()
    {
        Dictionary<string, float> tempDic = GameStatsUtility.CombineAttributes(CalculateBaseAttributes(), CalculateItemAttributes());
        
        return GameStatsUtility.CalculateEffectiveAttributes(tempDic);
    }

}

public class Enemy : Hero
{
    private EnemyInfo enemyInfo;
    private Inventory inventory;

    private float fTime;
    
    public void Refresh()
    {
        Initialize();
    }

    public void Initialize()
    {
        enemyInfo = GameObject.Find("EnemyInfo").GetComponent<EnemyInfo>();
        items = GetItems();
    }

    public override int GetHeroLevel()
    {
        return enemyInfo.GetLevel("Level") != 0? enemyInfo.GetLevel("Level") :-1;
    }

    public override List<Item> GetItems()
    {
        inventory = GameObject.Find("EnemyInventory").GetComponent<Inventory>();
        items = new List<Item>();
        for (int i = 0; i < 6; i++)
        {
            items.Add(inventory.itemSlots[i].item);
        }
        return items;
    }

    public override Rune GetRune()
    {
        Rune rune = new Rune
        {
            strStones = enemyInfo.GetSelectedRunes()
        };
        return rune;
    }

    public override Dictionary<string, float> CalculateBaseAttributes()
    { 
        return enemyInfo.GetBaseAttributes("Enemy");
    }

    public void PrepareToReceiveSpells()
    {
        fTime = 0f;
        Attributes = CalculateAttributes();
        fCurrentHealth = Attributes["HP"];
    }

    public double ReceiveSpell(SpellCast spellReceived)
    {
        switch (spellReceived.strDmgType)
        {
            case "AP":
                float fMR = Attributes["MR"];
                fMR *= (1 - spellReceived.amplifier.fMRpercentagePenetration);
                fMR -= spellReceived.amplifier.fMRpenetration;
                fMR = Mathf.Clamp(fMR, 0, 9999);
                fMR -= spellReceived.amplifier.fMRreduction;

                double damage = spellReceived.dDamage * (100 / (100 + fMR));
                Debug.Log("Damage is: " + damage + ", with effective enemy MR of " + fMR);
                return damage;
            default:
                Debug.LogError("Damage Type not recognized");
                return 0;
        }
    }

    public void Update()
    {
        
    }
    
}


public class Annie : Hero
{
    private SpellPanel spellPanel;
    private Inventory inventory;
    private AnnieInfo annieInfo;

    private int intElectrocuteCount;


    public void Initialize()
    {
        spellPanel = GameObject.Find("SpellPanel").GetComponent<SpellPanel>();
        annieInfo = GameObject.Find("AnnieInfo").GetComponent<AnnieInfo>();
        GenerateSpellPanel();
    }

    public void Refresh()
    {
        Initialize();
    }

    public override int GetHeroLevel()
    {
        return annieInfo.GetLevel("Level");
    }

    public override List<Item> GetItems()
    {
        items = new List<Item>();
        inventory = GameObject.Find("AnnieInventory").GetComponent<Inventory>();
        for(int i = 0; i<6; i++)
        {
            items.Add(inventory.itemSlots[i].item);
        }
        return items;
    }

    public override Rune GetRune()
    {
        Rune rune = new Rune
        {
            strStones = annieInfo.GetSelectedRunes()
        };
        return rune;
    }

    public override Dictionary<string, float> CalculateBaseAttributes()
    {
        return annieInfo.GetBaseAttributes("Annie");
    }

    public void PrepareToCastSpells()
    {
        items = GetItems();
        rune = GetRune();
        Attributes = CalculateAttributes();

        intElectrocuteCount = 0;

    }
    
    public SpellCast CastSpell(string spell)
    {
        int[] levels = new int[4] { annieInfo.GetLevel("Q"), annieInfo.GetLevel("W"), annieInfo.GetLevel("E"), annieInfo.GetLevel("R") };//0=Q 5=HeroLevel

        bool isThunderLoad = annieInfo.testThunder.isOn;


        GameDebugUtility.Debug_ShowDictionary("Cast Skill \n",Attributes);

        SpellCast spellcast = new SpellCast();
        switch (spell)
        {
            case "Q":
                if (!levels[0].Equals(0))
                {
                    spellcast.dDamage = 45 + 35 * levels[0] + 0.8 * Attributes["AP"];
                    spellcast.strDmgType = "AP";
                    spellcast.fCooldown = 4f;
                    Debug.Log("Casting Q of level " + levels[0] + ", Raw Damage is: " + spellcast.dDamage);
                }
                break;
            case "W":
                if (!levels[1].Equals(0))
                {
                    spellcast.dDamage = 25 + 45 * levels[1] + 0.85 * Attributes["AP"];
                    spellcast.strDmgType = "AP";
                    spellcast.fCooldown = 6f;
                    Debug.Log("Casting W of level " + levels[1] + ", Raw Damage is: " + spellcast.dDamage);
                }
                break;
            case "E":
                if (!levels[2].Equals(0))
                {
                    spellcast.dDamage = 15 + 15 * levels[2] + 0.3 * Attributes["AP"];
                    spellcast.strDmgType = "AP";
                    Debug.Log("Casting E of level " + levels[2] + ", Raw Damage is: " + spellcast.dDamage);
                }
                break;
            case "R":
                if (!levels[3].Equals(0))
                {
                    spellcast.dDamage = 75 + 80 * levels[3] + 0.65 * Attributes["AP"];
                    spellcast.strDmgType = "AP";
                    Debug.Log("Casting R of level " + levels[3] + ", Raw Damage is: " + spellcast.dDamage);
                }
                break;
            case "Electrocute":
                spellcast.dDamage = 40 + 10 * annieInfo.GetLevel("Level") + 0.3 * Attributes["AP"];
                spellcast.strDmgType = "AP";
                Debug.Log("Casting Electrocute of level " + annieInfo.GetLevel("Level") + ", Raw Damage is: " + spellcast.dDamage);
                break;
            default:
                Debug.LogError("SkillCastNotRecognized");
                break;
        }

        if (rune.strStones.Contains("Electrocute"))
        {
            intElectrocuteCount += 1;
            if(intElectrocuteCount == 3)
            {
                intElectrocuteCount = 0;
                spellcast.strAdditionalInfo = "Electrocute";
            }
        }

        float f;
        if (Attributes.TryGetValue("APPenetration", out f)) spellcast.amplifier.fMRpenetration = f;
        if (Attributes.TryGetValue("APPPenetration", out f)) spellcast.amplifier.fMRpercentagePenetration = f;

        return spellcast;
    }

    private void GenerateSpellPanel()
    {
        List<string> spellList = new List<string>
        {
            "Q",
            "W",
            "E",
            "R"
        };
        spellPanel.Initialize(spellList);
    }

}

