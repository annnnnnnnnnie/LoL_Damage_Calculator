using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

//Outdated, use Hero instead;

/*
public abstract class Hero_Base {

    public string strName;
    public float fLevel;

    public Rune rune;
    public List<Item> items = new List<Item>();
    public Dictionary<string, float> Attributes;

    public float fCurrentHealth;
    public List<Buff> buffs;

    protected int intTime;// 1=10ms
    protected static readonly int updateInterval = 10;

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
        
        return GameStatsUtility.CalculateEffectiveAttributes(tempDic, null);
    }

    public abstract void Update(SpellCast spellCast = null);

}

public class Enemy : Hero_Base
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
        return inventory.GetItems();
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
        return enemyInfo.CalculateBaseAttributes("Enemy");
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
        ReceiveDamage(dmg);
        if (fCurrentHealth < 1f)
            Debug.LogError("Enemy is dead");
        if (dmg > 0)
            Debug.Log("Damage is " + dmg.ToString());
        if (intTime % 100 == 10)
            Debug.Log("Current Time: " + intTime + "\nCurrentHealth: " + fCurrentHealth.ToString() + "\nDamage Dealt: " + (Attributes["HP"] - fCurrentHealth));

        intTime += updateInterval;
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
                    //Debug.Log("Time: "+ (intTime).ToString()+dot.strName + " DoT damage detected" + dot.ToString());

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
                }else if(buff is Debuff)
                {
                    Debuff debuff = (Debuff)buff;
                    debuff.intDuration -= updateInterval;
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
            ReceiveHealing(Attributes["HealthRegen"] * 0.1f);
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
        foreach (Buff buff in spellReceived.listBuffs)
        {
            ReceiveBuff(buff);
        }
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
        foreach(Buff buff in buffs)
        {
            if(buff.strName.Contains("Healing Reduction"))
            {
                Debug.Log("Reduced healing");
                healing *= 0.5f;
            }
        }
        fCurrentHealth += (float)healing;
        fCurrentHealth = Mathf.Clamp(fCurrentHealth, 0f, Attributes["HP"]);
    }

}


public class Annie : Hero_Base
{
    private SpellPanel spellPanel;
    private Inventory inventory;
    private AnnieInfo annieInfo;

    private Counter counter;


    public void Initialize()
    {
        spellPanel = GameObject.Find("SpellPanel").GetComponent<SpellPanel>();
        annieInfo = GameObject.Find("AnnieInfo").GetComponent<AnnieInfo>();
        InitializeSpellPanel();
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
        return inventory.GetItems();
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
        return annieInfo.CalculateBaseAttributes("Annie");
    }

    public void PrepareToCastSpells()
    {
        rune = GetRune();
        Attributes = CalculateAttributes();
        buffs = new List<Buff>();
        intTime = 0;

        counter.intElectrocuteCount = 0;

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

        intTime += updateInterval;
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
            case "A":
                spellcast.dDamage = Attributes["AD"];
                spellcast.strDmgType = "AD";
                Debug.Log("Auto Attacking, Raw Damage is: " + spellcast.dDamage);
                break;
            case "Electrocute":
                spellcast.dDamage = 40 + 10 * annieInfo.GetLevel("Level") + 0.3 * Attributes["AP"];
                spellcast.strDmgType = "AP";
                Debug.Log("Casting Electrocute of level " + annieInfo.GetLevel("Level") + ", Raw Damage is: " + spellcast.dDamage);
                break;
            case "Ignite":
                spellcast.listBuffs.Add(new DoT()
                {
                    isDamage = true,
                    intDuration = 500,
                    intInterval = 100,
                    intTickNumber = 5,
                    strDmgType = "True",
                    fDmgPerTick = (float)0.2 * (55 + 25 * annieInfo.GetLevel("Level")),
                    strDescription = "Ignite: deal 55 + 25 * level true damage in 5 seconds",
                    strName = "Ignite"
                });
                spellcast.listBuffs.Add(new Debuff()
                {
                    intDuration = 500,
                    strDescription = "Healing Reduction from Ignite",
                    strName = "Healing Reduction from Ignite"
                });
                break;
            default:
                Debug.LogError("SkillCastNotRecognized");
                break;
        }

        if (rune.strStones.Contains("Electrocute"))
        {
            counter.intElectrocuteCount += 1;
            if(counter.intElectrocuteCount == 3)
            {
                counter.intElectrocuteCount = 0;
                spellcast.strAdditionalInfo.Add("Electrocute");
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

    public void LearnNewSpell(string spell)
    {
        spellPanel.NewSpell(spell);
    }

    public void UnlearnSpell(string spell)
    {
        spellPanel.RemoveSpell(spell);
    }

    private void InitializeSpellPanel()
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

*/