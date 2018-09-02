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
    public List<Buff> buffs;

    protected int intTime;// 1=10ms

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

    public abstract void Update(SpellCast spellCast = null);

}

public class Enemy : Hero
{
    private EnemyInfo enemyInfo;
    private Inventory inventory;
    
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
        intTime = 0;
        Attributes = CalculateAttributes();
        fCurrentHealth = Attributes["HP"];
        buffs = new List<Buff>();
    }

    public override void Update(SpellCast spellCast = null)
    {
        if (spellCast != null)
        {
            Debug.Log("Receiving spell");
            ReceiveSpell(spellCast);
        }
        //Debug.Log("Normal update");
        double dmg = NormalUpdate();
        if (dmg > 0)
            Debug.Log("Damage is " + dmg.ToString());
        intTime += 10;
    }

    public double NormalUpdate()
    {
        double damage = 0f;
        List<Buff> buffsToBeRemoved = new List<Buff>();
        foreach (Buff buff in buffs)
        {
            if (buff.intDuration == 0)
            {
                Debug.Log(buff.strName + " has ended");
                buffsToBeRemoved.Add(buff);
            }
            else
            {
                if (buff is DoT)
                {
                    DoT dot = (DoT)buff;
                    Debug.Log("Time: "+ (intTime).ToString()+dot.strName + " DoT damage detected" + dot.ToString());

                    if ((intTime-dot.intTimeOfStart)%dot.intInterval == 0)
                    {
                        switch (dot.strDmgType)
                        {
                            case "True":
                                damage = dot.fDmgPerTick;
                                break;

                        }
                        dot.intDuration -= dot.intInterval;
                        dot.intTickNumber -= 1;
                    }
                }
            }
        }

        foreach(Buff buff in buffsToBeRemoved)
        {
            buffs.Remove(buff);
        }

        //---Health and mana regen---
        //Update every 0.5 second(intTime%50 ==0)
        if(intTime % 50 == 0)
        {
            //Attributes[]
        }

        return damage;
    }

    public double ReceiveSpell(SpellCast spellReceived)
    {
        double damage = 0f;

        switch (spellReceived.strDmgType)
        {
            case "AP":
                float fMR = Attributes["MR"];
                fMR *= (1 - spellReceived.amplifier.fMRpercentagePenetration);
                fMR -= spellReceived.amplifier.fMRpenetration;
                fMR = Mathf.Clamp(fMR, 0, 9999);
                fMR -= spellReceived.amplifier.fMRreduction;

                damage = spellReceived.dDamage * (100 / (100 + fMR));
                Debug.Log("Damage is: " + damage + ", with effective enemy MR of " + fMR);
                break;
            default:
                Debug.Log("Damage Type not recognized");
                damage = 0f;
                break;
        }
        ReceiveBuff(spellReceived.debuff);
        ReceiveDamage(damage);
        return damage;
    }

    private void ReceiveBuff(Buff buff)
    {
        if (buff == null) return;
        bool isNewBuff = true;
        foreach (Buff existingBuff in buffs)
        {
            if (existingBuff.Equals(buff))
            {
                isNewBuff = false;
                if (buff.intTimeOfStart == 0)
                {
                    Debug.Log(buff.strName + " has been refreshed");
                    existingBuff.intTimeOfStart = intTime;
                }
                else
                {
                    Debug.LogError("Unknown bug in refreshing buffs");
                }
            }
        }
        if (isNewBuff)
        {
            buff.intTimeOfStart = intTime;
            buffs.Add(buff);
        }
    }

    private void ReceiveDamage(double dmg)
    {
        fCurrentHealth -= (float)dmg;
    }

    private void ReceiveHealing(double healing)
    {
        fCurrentHealth += (float)healing;
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
        buffs = new List<Buff>();
        intTime = 0;

        intElectrocuteCount = 0;

    }

    public override void Update(SpellCast spellCast = null)
    {
        if(spellCast == null)
        {
            //Debug.Log("NormalUpdate");
        }
        else
        {
            Debug.Log("Receiving spell");
        }

        intTime += 10;
    }

    public SpellCast CastSpell(string spell)
    {
        int[] levels = new int[4] { annieInfo.GetLevel("Q"), annieInfo.GetLevel("W"), annieInfo.GetLevel("E"), annieInfo.GetLevel("R") };//0=Q 5=HeroLevel

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
            case "Ignite":
                spellcast.debuff = new DoT()
                {
                    isDamage = true,
                    intDuration = 500,
                    intInterval = 100,
                    intTickNumber = 5,
                    strDmgType = "True",
                    fDmgPerTick = (float)0.2 * (70 + 20 * annieInfo.GetLevel("Level")),
                    strDescription = "Ignite: deal 70 + 20 * level true damage in 5 seconds",
                    strName = "Ignite"
                };
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
        if (Attributes.TryGetValue("APPenetration", out f)) spellcast.amplifier = new Amplifier() { fMRpenetration = f };
        if (Attributes.TryGetValue("APPPenetration", out f)) spellcast.amplifier = new Amplifier() { fMRpercentagePenetration = f };

        return spellcast;
    }

    public void ReceiveSpell(SpellCast spellCast)
    {

    }

    private void GenerateSpellPanel()
    {
        List<string> spellList = new List<string>
        {
            "Q",
            "W",
            "E",
            "R",
            "Ignite"
        };
        spellPanel.Initialize(spellList);
    }

}

