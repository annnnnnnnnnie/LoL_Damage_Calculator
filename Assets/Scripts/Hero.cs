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
    public int intHeroLevel;
    public float fCurrentHealth;
    protected float fTotalDmgReceived;

    protected Dictionary<string, float> Attributes;
    public Rune rune;

    protected bool ApAdapted;
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
        if (heroName.Equals("Annie"))
        {
            InitializeSpellPanel();
        }
        intHeroLevel = GetHeroLevel();
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
        fTotalDmgReceived = 0;

        ApAdapted = Attributes["IAD"] < Attributes["IAP"];
        if (rune.strStones.Contains("AbsoluteFocus"))
        {
            Debug.Log("AbsoluteFocus detected");
            if (fCurrentHealth >= 0.7 * Attributes["HP"])
            {
                if (ApAdapted)
                {
                    Attributes["AP"] += (float)(5.0 + 2.06 * (intHeroLevel - 1));
                }
                else
                {
                    Attributes["AD"] += (float)(3.0 + 1.24 * (intHeroLevel - 1));
                }
            }
        }
        if (Attributes.ContainsKey("Unique_Passive_Echo"))
        {
            Debug.Log("Echo Detected");
            counter.EchoCount = 100;
        }
        if (Attributes.ContainsKey("Unique_Passive_MagicBolt"))
        {
            Debug.Log("HexRevolver Detected");
            ReceiveBuff(Buff.Hextech);
        }

    }



    public void PrepareToCastSpells()//Temporary
    {
        GetPrepared();
    }
    public void PrepareToReceiveSpells()//Temporary
    {
        GetPrepared();
    }
    public void ReceiveSpell(SpellCast spellReceived)
    {
        ReceiveDamage(spellReceived.dDamage, spellReceived.strDmgType, spellReceived.amplifier);
        foreach (Buff buff in spellReceived.listBuffs)
        {
            ReceiveBuff(buff);
        }
    }
    public void Update(List<SpellCast> spellCasts = null)
    {
        if (spellCasts != null)
        {
            foreach (SpellCast spellCast in spellCasts)
            {
                Debug.Log("Receiving spell");
                ReceiveSpell(spellCast);
            }
        }

        NormalUpdate();

        if (fCurrentHealth < 1f)
            Debug.LogError("Enemy is dead");


        bool isOn = true;
        if (isOn && (intTime % 10 == 0) && fTotalDmgReceived > 0)
        {
            Debug.Log("Time: " + intTime + "0ms" + "CurrentHealth: " + fCurrentHealth.ToString()
                + "\nDamage Dealt: " + (Attributes["HP"] - fCurrentHealth) + "("
                + heroName + ")");
            GameDebugUtility.AddDebugMsg("Total damage dealt: " + (Attributes["HP"] - fCurrentHealth) + "("
                + heroName + ")");
        }

        intTime += updateInterval;
    }

    public void NormalUpdate()
    {
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
                        ReceiveDamage(dot.fDmgPerTick, dot.strDmgType, dot.amplifier);
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
        //if (intTime % 50 == 0)
        //{
        //    ReceiveHealing(Attributes["HealthRegen"] * 0.1f);
        //}

    }



    protected void ReceiveBuff(Buff buff)
    {
        if (buff == null) return;
        buff = buff.MakeCopy();
        bool isNewBuff = true;
        foreach (Buff existingBuff in buffs)
        {
            if (existingBuff.Equals(buff))
            {
                isNewBuff = false;
                if (buff.intTimeOfStart != intTime)
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
        if (buffs.Contains(Debuff.CoupDeGrace) && (fCurrentHealth / Attributes["HP"] <= 0.4))
        {
            Debug.Log("CoupDeGraceDmgAmp");
            dmg *= 1.07;
        }

        fTotalDmgReceived += (float)dmg;
        fCurrentHealth -= (float)dmg;
    }
    private void ReceiveDamage(double dDamage, string dmgType, Amplifier amplifier)
    {
        double damage = 0;
        switch (dmgType)
        {
            case "AP":
                float fMR = Attributes["MR"];
                fMR *= (1 - amplifier.fMRpercentagePenetration);
                fMR -= amplifier.fMRpenetration;
                fMR = Mathf.Clamp(fMR, 0, 9999);
                fMR -= amplifier.fMRreduction;

                damage = dDamage * (100 / (100 + fMR));
                Debug.Log("Damage is: " + damage + ", with effective enemy MR of " + fMR);
                break;
            case "AD":
                float fArmor = Attributes["Armor"];
                damage = dDamage * (100 / (100 + fArmor));
                Debug.Log("Damage is: " + damage + ", with effective enemy Armor of " + fArmor);
                break;
            case "True":
                damage = dDamage;
                Debug.Log("Damage is: " + damage);
                break;
            default:
                Debug.Log("Damage Type not recognized");
                damage = 0f;
                break;
        }
        if ((buffs.Contains(Debuff.CoupDeGrace) || amplifier.otherAmplifers.Contains("CoupDeGrace"))
            && (fCurrentHealth / Attributes["HP"] <= 0.4 && !dmgType.Equals("True")))
        {
            Debug.Log("CoupDeGraceDmgAmp");
            damage *= 1.07;
        }

        fTotalDmgReceived += (float)damage;
        fCurrentHealth -= (float)damage;
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

    protected void ReduceBuffTime(string buffName, int timeReduced)
    {
        foreach(Buff buff in buffs)
        {
            if (buff.strName.Equals(buffName))
            {
                buff.intDuration -= timeReduced;
            }
        }
    }
    protected void ReduceBuffTime(string buffName, float percentageTimeReduced)
    {
        bool flag = false;
        foreach (Buff buff in buffs)
        {
            if (buff.strName.Equals(buffName))
            {
                buff.intDuration = Mathf.RoundToInt(buff.intDuration *(1-percentageTimeReduced));
                flag = true;
                Debug.Log(buff.strName + " is reduced");
            }
        }
        if (!flag)
        {
            Debug.LogError("Cannot Reduce buff time: " + buffName + " is not in the buff list");
        }
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
            "RA",
            "Ignite",
            "A"
        };
        heroInfo.spellPanel.Initialize(spellList);
    }
}

public class Annie_Test : Hero
{
    private string msg;
    public SpellCast CastSpell(string spell)
    {
        int[] levels = new int[4] { heroInfo.GetLevel("Q"), heroInfo.GetLevel("W"), heroInfo.GetLevel("E"), heroInfo.GetLevel("R") };//0=Q 5=HeroLevel

        

        GameDebugUtility.Debug_ShowDictionary("Cast Skill ", Attributes);
        SpellCast spellcast = new SpellCast();

        float f;
        Amplifier amplifier = new Amplifier();
        if (Attributes.TryGetValue("APPenetration", out f)) amplifier.fMRpenetration = f;
        if (Attributes.TryGetValue("APPPenetration", out f)) amplifier.fMRpercentagePenetration = f;
        if (rune.strStones.Contains("CoupDeGrace")) amplifier.otherAmplifers.Add("CoupDeGrace");

        switch (spell)
        {
            case "Q":
                if (!levels[0].Equals(0))
                {
                    spellcast.dDamage = 45 + 35 * levels[0] + 0.8 * Attributes["AP"];
                    spellcast.strDmgType = "AP";
                    spellcast.fCooldown = 4f;
                    msg = "Casting Q of level " + levels[0] + ", Raw Damage is: " + spellcast.dDamage;
                    Debug.Log(msg);
                    GameDebugUtility.AddDebugMsg(msg);
                    if (rune.strStones.Contains("Electrocute") && !buffs.Contains(Debuff.ElectrocuteCD))
                    {
                        counter.intElectrocuteCount += 1;
                    }
                    if (rune.strStones.Contains("ArcaneComet"))
                    {
                        if (!buffs.Contains(Debuff.ArcaneCometCD))
                        {
                            spellcast.strAdditionalInfo.Add("ArcaneComet");
                        }
                        else
                        {
                            ReduceBuffTime("ArcaneCometCD", 0.2f);
                        }
                    }
                    if (Attributes.ContainsKey("Unique_Passive_TouchOfCorruption"))
                    {
                        spellcast.listBuffs.Add(new DoT()
                        {
                            isDamage = true,
                            intDuration = 300,
                            intInterval = 100,
                            intTickNumber = 3,
                            strDmgType = "AP",
                            fDmgPerTick = (float)(0.3333 * (15 + 0.88 * heroInfo.GetLevel("Level"))),
                            strDescription = "CorruptingPotion: deal 15 + 0.88 * level AP damage in 3 seconds",
                            strName = "CorruptingPotion",
                            amplifier = amplifier.MakeCopy()
                        });
                    }
                    if (Attributes.ContainsKey("Unique_Passive_Echo"))
                    {
                        if (counter.EchoCount >= 100)
                        {
                            spellcast.strAdditionalInfo.Add("Echo");
                        }
                        else
                        {
                            counter.EchoCount += 10;
                        }

                    }
                    if (Attributes.ContainsKey("Unique_Passive_SpellBlade") && !buffs.Contains(Debuff.SpellBladeCD))
                    {
                        ReceiveBuff(Buff.SpellBlade);
                    }
                }
                else
                {
                    Debug.Log("Q has not been learnt");
                }
                break;
            case "W":
                if (!levels[1].Equals(0))
                {
                    spellcast.dDamage = 25 + 45 * levels[1] + 0.85 * Attributes["AP"];
                    spellcast.strDmgType = "AP";
                    spellcast.fCooldown = 6f;
                    msg = "Casting W of level " + levels[1] + ", Raw Damage is: " + spellcast.dDamage;
                    Debug.Log(msg);
                    GameDebugUtility.AddDebugMsg(msg);
                    if (rune.strStones.Contains("Electrocute") && !buffs.Contains(Debuff.ElectrocuteCD))
                    {
                        counter.intElectrocuteCount += 1;
                    }
                    if (rune.strStones.Contains("ArcaneComet"))
                    {
                        if (!buffs.Contains(Debuff.ArcaneCometCD))
                        {
                            spellcast.strAdditionalInfo.Add("ArcaneComet");
                        }
                        else
                        {
                            ReduceBuffTime("ArcaneCometCD", 0.1f);
                        }
                    }
                    if (Attributes.ContainsKey("Unique_Passive_TouchOfCorruption"))
                    {
                        spellcast.listBuffs.Add(new DoT()
                        {
                            isDamage = true,
                            intDuration = 300,
                            intInterval = 100,
                            intTickNumber = 3,
                            strDmgType = "AP",
                            fDmgPerTick = (float)(0.5 * 0.3333 * (15 + 0.88 * heroInfo.GetLevel("Level"))),
                            strDescription = "(Halved)CorruptingPotion: deal 15 + 0.88 * level AP damage in 3 seconds",
                            strName = "CorruptingPotion(Halved)",
                            amplifier = amplifier.MakeCopy()
                        });
                    }
                    if (Attributes.ContainsKey("Unique_Passive_Echo"))
                    {
                        if (counter.EchoCount >= 100)
                        {
                            spellcast.strAdditionalInfo.Add("Echo");
                        }
                        else
                        {
                            counter.EchoCount += 10;
                        }

                    }
                    if (Attributes.ContainsKey("Unique_Passive_SpellBlade") && !buffs.Contains(Debuff.SpellBladeCD))
                    {
                        ReceiveBuff(Buff.SpellBlade);
                    }
                }
                else
                {
                    Debug.Log("W has not been learnt");
                }
                break;
            case "E":
                if (!levels[2].Equals(0))
                {
                    ReceiveBuff(new Buff
                    {
                        intDuration = 300,
                        strName = "Molten_Shield_Damage",
                        strDescription = "MoltenShield_Damage"
                    });
                    ReceiveBuff(new Buff
                    {
                        intDuration = 300,
                        strName = "Molten_Shield_Damage_Reduction",
                        strDescription = "MoltenShield_Damage_Reduction"
                    });
                    if (Attributes.ContainsKey("Unique_Passive_SpellBlade") && !buffs.Contains(Debuff.SpellBladeCD))
                    {
                        ReceiveBuff(Buff.SpellBlade);
                    }
                    Debug.Log("Casting E of level " + levels[2]);
                }
                else
                {
                    Debug.Log("E has not been learnt");
                }
                break;
            case "Edmg":
                if (!levels[2].Equals(0))
                {
                    spellcast.dDamage = 15 + 15 * levels[2] + 0.3 * Attributes["AP"];
                    spellcast.strDmgType = "AP";
                    Debug.Log("Casting Edmg of level " + levels[2] + ", Raw Damage is: " + spellcast.dDamage);
                    if (rune.strStones.Contains("Electrocute") && !buffs.Contains(Debuff.ElectrocuteCD))
                    {
                        counter.intElectrocuteCount += 1;
                    }
                    if (rune.strStones.Contains("ArcaneComet"))
                    {
                        if (!buffs.Contains(Debuff.ArcaneCometCD))
                        {
                            spellcast.strAdditionalInfo.Add("ArcaneComet");
                        }
                        else
                        {
                            ReduceBuffTime("ArcaneCometCD", 0.1f);
                        }
                    }
                    if (Attributes.ContainsKey("Unique_Passive_Echo"))
                    {
                        if (counter.EchoCount >= 100)
                        {
                            spellcast.strAdditionalInfo.Add("Echo");
                        }
                        else
                        {
                            counter.EchoCount += 10;
                        }

                    }
                }
                break;
            case "R":
                if (!levels[3].Equals(0))
                {
                    spellcast.dDamage = 75 + 80 * levels[3] + 0.65 * Attributes["AP"];
                    spellcast.strDmgType = "AP";
                    msg = "Casting R of level " + levels[3] + ", Raw Damage is: " + spellcast.dDamage;
                    Debug.Log(msg);
                    GameDebugUtility.AddDebugMsg(msg);
                    spellcast.listBuffs.Add(new DoT()
                    {
                        isDamage = true,
                        intDuration = 500,
                        intInterval = 100,
                        intTickNumber = 5,
                        strDmgType = "AP",
                        fDmgPerTick = (float)(5 + 5 * levels[3] + 0.1 * Attributes["AP"]),
                        strDescription = "Tibbers Burn",
                        strName = "Tibbers Burn",
                        amplifier = amplifier.MakeCopy()
                    });
                    if (rune.strStones.Contains("Electrocute") && !buffs.Contains(Debuff.ElectrocuteCD))
                    {
                        counter.intElectrocuteCount += 1;
                    }
                    if (rune.strStones.Contains("ArcaneComet"))
                    {
                        if (!buffs.Contains(Debuff.ArcaneCometCD))
                        {
                            spellcast.strAdditionalInfo.Add("ArcaneComet");
                        }
                        else
                        {
                            ReduceBuffTime("ArcaneCometCD", 0.1f);
                        }
                    }
                    if (Attributes.ContainsKey("Unique_Passive_TouchOfCorruption"))
                    {
                        spellcast.listBuffs.Add(new DoT()
                        {
                            isDamage = true,
                            intDuration = 300,
                            intInterval = 100,
                            intTickNumber = 3,
                            strDmgType = "AP",
                            fDmgPerTick = (float)(0.5 * 0.3333 * (15 + 0.88 * heroInfo.GetLevel("Level"))),
                            strDescription = "(Halved)CorruptingPotion: deal 15 + 0.88 * level AP damage in 3 seconds",
                            strName = "CorruptingPotion(Halved)",
                            amplifier = amplifier.MakeCopy()
                        });
                    }
                    if (Attributes.ContainsKey("Unique_Passive_Echo"))
                    {
                        if (counter.EchoCount >= 100)
                        {
                            spellcast.strAdditionalInfo.Add("Echo");
                        }
                        else
                        {
                            counter.EchoCount += 10;
                        }

                    }
                    if (Attributes.ContainsKey("Unique_Passive_SpellBlade") && !buffs.Contains(Debuff.SpellBladeCD))
                    {
                        ReceiveBuff(Buff.SpellBlade);
                    }
                }
                else
                {
                    Debug.Log("R has not been learnt");
                }
                break;
            case "RA":
                if (!levels[3].Equals(0))
                {
                    spellcast.dDamage = 25 + 25 * levels[3] + 0.15 * Attributes["AP"];
                    spellcast.strDmgType = "AP";
                    msg = "Casting RA of level " + levels[3] + ", Raw Damage is: " + spellcast.dDamage;
                    Debug.Log(msg);
                    GameDebugUtility.AddDebugMsg(msg);
                    if (rune.strStones.Contains("ArcaneComet"))
                    {
                        if (!buffs.Contains(Debuff.ArcaneCometCD))
                        {
                            spellcast.strAdditionalInfo.Add("ArcaneComet");
                        }
                        else
                        {
                            ReduceBuffTime("ArcaneCometCD", 0.1f);
                        }
                    }
                    if (Attributes.ContainsKey("Unique_Passive_TouchOfCorruption"))
                    {
                        spellcast.listBuffs.Add(new DoT()
                        {
                            isDamage = true,
                            intDuration = 300,
                            intInterval = 100,
                            intTickNumber = 3,
                            strDmgType = "AP",
                            fDmgPerTick = (float)(0.5 * 0.3333 * (15 + 0.88 * heroInfo.GetLevel("Level"))),
                            strDescription = "(Halved)CorruptingPotion: deal 15 + 0.88 * level AP damage in 3 seconds",
                            strName = "CorruptingPotion(Halved)",
                            amplifier = amplifier.MakeCopy()
                        });
                    }
                    if (Attributes.ContainsKey("Unique_Passive_Echo"))
                    {
                        if (counter.EchoCount >= 100)
                        {
                            spellcast.strAdditionalInfo.Add("Echo");
                        }
                        else
                        {
                            counter.EchoCount += 10;
                        }

                    }
                }
                else
                {
                    Debug.Log("R has not been learnt");
                }
                break;
            case "A":
                spellcast.dDamage = Attributes["AD"];
                spellcast.strDmgType = "AD";
                msg = "Auto Attacking, Raw Damage is: " + spellcast.dDamage;
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg);

                if (rune.strStones.Contains("Electrocute") && !buffs.Contains(Debuff.ElectrocuteCD))
                {
                    counter.intElectrocuteCount += 1;
                }
                if (buffs.Contains(Buff.Hextech) && !buffs.Contains(Debuff.HextechCD))
                {
                    spellcast.strAdditionalInfo.Add("HextechRevolver");
                }
                if (Attributes.ContainsKey("Unique_Passive_TouchOfCorruption"))
                {
                    spellcast.listBuffs.Add(new DoT()
                    {
                        isDamage = true,
                        intDuration = 300,
                        intInterval = 100,
                        intTickNumber = 3,
                        strDmgType = "AP",
                        fDmgPerTick = (float)(0.3333 * (15 + 0.88 * heroInfo.GetLevel("Level"))),
                        strDescription = "CorruptingPotion: deal 15 + 0.88 * level AP damage in 3 seconds",
                        strName = "CorruptingPotion"
                    });
                }
                if (buffs.Contains(Buff.SpellBlade)&& !buffs.Contains(Debuff.SpellBladeCD))
                {
                    if (Attributes.ContainsKey("Unique_Passive_SpellBlade"))
                    {
                        spellcast.strAdditionalInfo.Add("SpellBlade_LichBane");
                    }
                }
                break;
            case "Electrocute":
                spellcast.dDamage = 40 + 10 * intHeroLevel + 0.3 * Attributes["AP"];
                spellcast.strDmgType = "AP";
                ReceiveBuff(new Debuff
                {
                    intDuration = (25 - 2 * intHeroLevel) * 100,
                    intTimeOfStart = intTime,
                    strName = "ElectrocuteCD",
                    strDescription = "ElectrocuteCD"
                });
                msg = "Casting Electrocute at level " + intHeroLevel + ", Raw Damage is: " + spellcast.dDamage;
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg);
                break;
            case "ArcaneComet":
                spellcast.dDamage = 15.88 + 4.12 * intHeroLevel + 0.2 * Attributes["AP"];
                spellcast.strDmgType = "AP";
                ReceiveBuff(new Debuff
                {
                    intDuration = (25 - 2 * intHeroLevel),
                    intTimeOfStart = intTime,
                    strName = "ArcaneCometCD",
                    strDescription = "ArcaneCometCD"
                });
                msg = "Casting ArcaneComet at level " + intHeroLevel + ", Raw Damage is: " + spellcast.dDamage;
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg);
                break;
            case "Echo":
                spellcast.dDamage = 100 + 0.10 * Attributes["AP"];
                spellcast.strDmgType = "AP";
                msg = "Casting Echo, Raw Damage is: " + spellcast.dDamage;
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg);
                counter.EchoCount = 0;
                break;
            case "HextechRevolver":
                spellcast.dDamage = 50 + 4.41 * intHeroLevel;
                spellcast.strDmgType = "AP";
                msg = "Casting HextechRevolver at level " + intHeroLevel + ", Raw Damage is: " + spellcast.dDamage;
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg);
                ReceiveBuff(Debuff.HextechCD);
                break;
            case "HextechProtobelt_01":
                int rockets = (int)Attributes["Unique_Active_FireBolt"];
                double oneRocketDmg = 75 + 4.41 * intHeroLevel + 0.25 * Attributes["AP"];
                spellcast.dDamage = oneRocketDmg + 0.1 * Mathf.Clamp((rockets - 1), 0, 7);
                spellcast.strDmgType = "AP";
                msg = ("Casting Protobelt at level " + intHeroLevel + ", " + rockets + " rockets fired, Raw Damage is: " + spellcast.dDamage);
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg);
                if (rune.strStones.Contains("Electrocute") && !buffs.Contains(Debuff.ElectrocuteCD))
                {
                    counter.intElectrocuteCount += 1;
                }
                if (rune.strStones.Contains("ArcaneComet"))
                {
                    if (!buffs.Contains(Debuff.ArcaneCometCD))
                    {
                        spellcast.strAdditionalInfo.Add("ArcaneComet");
                    }
                    else
                    {
                        ReduceBuffTime("ArcaneCometCD", 0.1f);
                    }
                }
                if (Attributes.ContainsKey("Unique_Passive_TouchOfCorruption"))
                {
                    spellcast.listBuffs.Add(new DoT()
                    {
                        isDamage = true,
                        intDuration = 300,
                        intInterval = 100,
                        intTickNumber = 3,
                        strDmgType = "AP",
                        fDmgPerTick = (float)(0.5 * 0.3333 * (15 + 0.88 * heroInfo.GetLevel("Level"))),
                        strDescription = "(Halved)CorruptingPotion: deal 15 + 0.88 * level AP damage in 3 seconds",
                        strName = "CorruptingPotion(Halved)",
                        amplifier = amplifier.MakeCopy()
                    });
                }
                if (Attributes.ContainsKey("Unique_Passive_Echo"))
                {
                    if (counter.EchoCount >= 100)
                    {
                        spellcast.strAdditionalInfo.Add("Echo");
                    }
                    else
                    {
                        counter.EchoCount += 10;
                    }

                }
                break;
            case "SpellBlade_LichBane":
                spellcast.dDamage = 0.75 * Attributes["BAD"] + 0.5 * Attributes["AP"];
                spellcast.strDmgType = "AP";
                msg = "Casting SpellBlade_LichBane, Raw Damage is: " + spellcast.dDamage;
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg);
                ReceiveBuff(Debuff.SpellBladeCD);
                break;
            case "Ignite":
                spellcast.listBuffs.Add(new DoT()
                {
                    isDamage = true,
                    intDuration = 500,
                    intInterval = 100,
                    intTickNumber = 5,
                    strDmgType = "True",
                    fDmgPerTick = (float)0.2 * (55 + 25 * intHeroLevel),
                    strDescription = "Ignite: deal 55 + 25 * level true damage in 5 seconds",
                    strName = "Ignite",
                    amplifier = new Amplifier()
                });
                spellcast.listBuffs.Add(new Debuff()
                {
                    intDuration = 500,
                    strDescription = "Healing Reduction from Ignite",
                    strName = "Healing Reduction from Ignite"
                });
                msg = "Casting Ignite at level " + intHeroLevel;
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg);
                break;
            default:
                Debug.LogError("SpellCastNotRecognized");
                break;
        }


        if (counter.intElectrocuteCount == 3)
        {
            counter.intElectrocuteCount = 0;
            spellcast.strAdditionalInfo.Add("Electrocute");
            
        }

        spellcast.amplifier = amplifier.MakeCopy();

        return spellcast;
    }
}
