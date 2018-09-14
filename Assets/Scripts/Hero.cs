using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public abstract class Hero {
    //Under development, use Hero_Base for now
    public HeroInfo heroInfo; //Assign through inspector for now

    public string strName;
    public string heroName;
    public float fLevel;
    public float fCurrentHealth;
    
    protected Dictionary<string, float> Attributes;
    public Rune rune;

    private bool ApAdapted;
    public List<Buff> buffs;
    protected int intTime;// 1=10ms
    protected static readonly int updateInterval = 10;
    protected Counter counter = new Counter();//for Liandry's torment, electrocute, etc

    public void Refresh()
    {

    }
    public void Initialize(string heroName)
    {
        this.heroName = heroName;
        strName = heroName;
        heroInfo = GameObject.Find(strName + "Info").GetComponent<HeroInfo>();//Future development: HeroInfo would automatically register itself to use Inventory, RunePage, etc

        InitializeSpellPanel();
    }

    public int GetHeroLevel()
    {
        return heroInfo.GetLevel("Level");
    }
    public Rune GetRune()
    {
        return new Rune()
        {
            strStones = heroInfo.GetSelectedRunes()
        };
    }

    protected void GetPrepared()
    {
        intTime = 0;
        Attributes = heroInfo.CalculateAttributes();
        rune = GetRune();
        fCurrentHealth = Attributes["HP"];
        buffs = new List<Buff>();
        counter.Reset();
    }

    

    public void PrepareToCastSpells()//Temporary
    {
        GetPrepared();
    }
    public void PrepareToReceiveSpells()//Temporary
    {
        GetPrepared();
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
    public void Update(SpellCast spellCast = null)
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
        if (intTime % 100 == 0)
            Debug.Log("Time: " + intTime + "0ms" + "CurrentHealth: " + fCurrentHealth.ToString() + "\nDamage Dealt: " + (Attributes["HP"] - fCurrentHealth) + "(" 
                + heroName + ")");

        intTime += updateInterval;
    }

    public double NormalUpdate()
    {
        double damage = 0f;
        List<Buff> buffsToBeRemoved = new List<Buff>();
        foreach (Buff buff in buffs)
        {
            if (buff.intDuration <= 0)
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

                    if ((intTime - dot.intTimeOfStart) % dot.intInterval == 0)
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
                else if (buff is Debuff)
                {
                    Debuff debuff = (Debuff)buff;
                    debuff.intDuration -= updateInterval;
                    Debug.Log("debuff: " + debuff.strName);
                }
            }
        }

        foreach (Buff buff in buffsToBeRemoved)
        {
            buffs.Remove(buff);
        }

        //---Health and mana regen---
        //Update every 0.5 second(intTime%50 ==0)
        if (intTime % 50 == 0)
        {
            ReceiveHealing(Attributes["HealthRegen"] * 0.1f);
        }

        return damage;
    }



    protected void ReceiveBuff(Buff buff)
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
        foreach (Buff buff in buffs)
        {
            if (buff.strName.Contains("Healing Reduction"))
            {
                Debug.Log("Reduced healing: " + heroName);
                healing *= 0.5f;
            }
        }
        fCurrentHealth += (float)healing;
        fCurrentHealth = Mathf.Clamp(fCurrentHealth, 0f, Attributes["HP"]);
    }

    public void LearnNewSpell(string spell)
    {
        heroInfo.spellPanel.NewSpell(spell);
    }

    public void UnlearnSpell(string spell)
    {
        heroInfo.spellPanel.RemoveSpell(spell);
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
        heroInfo.spellPanel.Initialize(spellList);
    }
}

public class TestHero : Hero
{
    public SpellCast CastSpell(string spell)
    {
        int[] levels = new int[4] { heroInfo.GetLevel("Q"), heroInfo.GetLevel("W"), heroInfo.GetLevel("E"), heroInfo.GetLevel("R") };//0=Q 5=HeroLevel

        GameDebugUtility.Debug_ShowDictionary("Cast Skill \n", Attributes);

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

                    if (rune.strStones.Contains("Electrocute") && !buffs.Contains(Debuff.ElectrocuteCD))
                    {
                        counter.intElectrocuteCount += 1;
                    }
                }
                break;
            case "W":
                if (!levels[1].Equals(0))
                {
                    spellcast.dDamage = 25 + 45 * levels[1] + 0.85 * Attributes["AP"];
                    spellcast.strDmgType = "AP";
                    spellcast.fCooldown = 6f;
                    Debug.Log("Casting W of level " + levels[1] + ", Raw Damage is: " + spellcast.dDamage);
                    if (rune.strStones.Contains("Electrocute") && !buffs.Contains(Debuff.ElectrocuteCD))
                    {
                        counter.intElectrocuteCount += 1;
                    }
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
                    if (rune.strStones.Contains("Electrocute") && !buffs.Contains(Debuff.ElectrocuteCD))
                    {
                        counter.intElectrocuteCount += 1;
                    }
                }
                break;
            case "A":
                spellcast.dDamage = Attributes["AD"];
                spellcast.strDmgType = "AD";
                Debug.Log("Auto Attacking, Raw Damage is: " + spellcast.dDamage);

                if (rune.strStones.Contains("Electrocute") && !buffs.Contains(Debuff.ElectrocuteCD))
                {
                    counter.intElectrocuteCount += 1;
                }
                break;
            case "Electrocute":
                spellcast.dDamage = 40 + 10 * heroInfo.GetLevel("Level") + 0.3 * Attributes["AP"];
                spellcast.strDmgType = "AP";
                Debug.Log("Casting Electrocute of level " + heroInfo.GetLevel("Level") + ", Raw Damage is: " + spellcast.dDamage);
                break;
            case "Echo":
                spellcast.dDamage = 100 + 0.15 * Attributes["AP"];
                spellcast.strDmgType = "AP";
                Debug.Log("Casting Echo, Raw Damage is: " + spellcast.dDamage);
                break;
            case "Ignite":
                spellcast.listBuffs.Add(new DoT()
                {
                    isDamage = true,
                    intDuration = 500,
                    intInterval = 100,
                    intTickNumber = 5,
                    strDmgType = "True",
                    fDmgPerTick = (float)0.2 * (55 + 25 * heroInfo.GetLevel("Level")),
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


        if (counter.intElectrocuteCount == 3)
        {
            counter.intElectrocuteCount = 0;
            spellcast.strAdditionalInfo.Add("Electrocute");
            ReceiveBuff(new Debuff
            {
                intDuration = (25 - 2 * GetHeroLevel()),
                intTimeOfStart = intTime,
                strName = "ElectrocuteCD",
                strDescription = "ElectrocuteCD"
            });
        }



        float f;
        if (Attributes.TryGetValue("APPenetration", out f)) spellcast.amplifier = new Amplifier() { fMRpenetration = f };
        if (Attributes.TryGetValue("APPPenetration", out f)) spellcast.amplifier = new Amplifier() { fMRpercentagePenetration = f };

        return spellcast;
    }
}
