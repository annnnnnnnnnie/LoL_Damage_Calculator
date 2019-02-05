using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public abstract class Hero {
    
    public HeroInfo heroInfo; //Assign through inspector for now

    public string heroName;
    public int intHeroLevel;
    public float fCurrentHealth;
    protected float fTotalDmgReceived;

    protected Dictionary<string, float> BaseAttributes;
    protected Dictionary<string, float> RuntimeAttributes;
    public Rune rune;

    protected bool ApAdapted;
    public List<Buff> buffs;
    protected int intTime;// 1=10ms
    protected static readonly int updateInterval = 10;
    protected Counter counter = new Counter();//for Liandry's torment, electrocute, etc

    public void Refresh()
    {

    }

    public void Initialize(string heroName, GameObject heroInfoPrefab, Transform holderTransform, Processor processor)
    {
        this.heroName = heroName;
        heroInfo = GameObjectUtility.CustomInstantiate(heroInfoPrefab, holderTransform).GetComponent<HeroInfo>();
        if (heroName.Equals("Annie"))
        {
            heroInfo.Initialize(false, processor, this);
            //Debug.Log(this.heroName);
        }
        else
        {
            heroInfo.Initialize(false, processor, this);
            //Debug.Log(this.heroName);
        }
        InitializeSpellPanel(heroName);
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
        intHeroLevel = GetHeroLevel();
        BaseAttributes = heroInfo.CalculateAttributes();
        RuntimeAttributes = heroInfo.CalculateAttributes();

        fCurrentHealth = BaseAttributes["HP"];
        ApAdapted = RuntimeAttributes["IAD"] < RuntimeAttributes["IAP"];

        rune = GetRune();
        buffs = new List<Buff>();
        counter.Reset();
        fTotalDmgReceived = 0;

        
        if (RuntimeAttributes.ContainsKey("Unique_Passive_Echo"))
        {
            Debug.Log("Echo Detected");
            counter.EchoCount = 100;
        }
        if (RuntimeAttributes.ContainsKey("Unique_Passive_MagicBolt"))
        {
            Debug.Log("HexRevolver Detected");
            ReceiveBuff(Buff.Hextech);
        }
        if (rune.strStones.Contains("NullifyingOrb"))
        {
            Debug.Log("NullifyingOrb Detected");
            ReceiveBuff(Buff.NullifyingOrb);
        }
        if (rune.strStones.Contains("CheapShot"))
        {
            Debug.Log("CheapShot Detected");
            ReceiveBuff(Buff.CheapShot);
        }

        int maxCDR = 40;
        Debug.Assert(BaseAttributes["CDR"] == 0);
        if (rune.strStones.Contains("Transcendence"))
        {
            if(intHeroLevel >= 10)
            {
                BaseAttributes["CDR"] += 10;
            }
        }
        if (rune.strStones.Contains("CosmicInsight"))
        {
            maxCDR = 45;
            BaseAttributes["CDR"] += 5;
            BaseAttributes.Add("UltCDR", 5);
        }
        BaseAttributes["CDR"] += BaseAttributes["ICD"];
        Debug.Log("CDR is " + BaseAttributes["CDR"] + "%");
        if (rune.strStones.Contains("Transcendence"))
        {
            if (ApAdapted)
            {
                BaseAttributes["AP"] += (float)2 * Mathf.Max(0, (BaseAttributes["CDR"] - maxCDR));
                Debug.Log("Transcendence AP: " + ((float)2 * Mathf.Max(0, (BaseAttributes["CDR"] - maxCDR))));
            }
            else
            {
                BaseAttributes["AD"] += (float)1.2 * Mathf.Max(0, (BaseAttributes["CDR"] - maxCDR));
            }
        }
        BaseAttributes["CDR"] = Mathf.Clamp(BaseAttributes["CDR"], 0, maxCDR);
        Debug.Log("Final CDR is " + BaseAttributes["CDR"] + "%");

        NormalUpdate();
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
        ReceiveDamage(spellReceived.dDamage, spellReceived.strDmgType, spellReceived.source);
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
        
        if (fCurrentHealth < 1f)
            Debug.LogError("Enemy is dead");

        bool isOn = true;
        if (isOn && (intTime % 50 == 0) && fTotalDmgReceived > 0)
        {
            Debug.Log("Time: " + intTime + "0ms" + "CurrentHealth: " + fCurrentHealth.ToString()
                + "\nDamage Dealt: " + (RuntimeAttributes["MaxHP"] - fCurrentHealth) + "(against "
                + heroName + ")");
            GameDebugUtility.AddDebugMsg("Total damage dealt: " + (RuntimeAttributes["MaxHP"] - fCurrentHealth) + "("
                + heroName + ")", intTime);
        }
        NormalUpdate();

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
                    if ((intTime != dot.intTimeOfStart) && (intTime - dot.intTimeOfStart) % dot.intInterval == 0)
                    {
                        switch (dot.strName)
                        {
                            case "CorruptingPotion_Halved":
                                if (!buffs.Contains(new DoT { strName = "CorruptingPotion" }))
                                {
                                    ReceiveDamage(dot.fDmgPerTick, dot.strDmgType, dot.source);
                                }
                                else
                                {
                                    Debug.Log("Skipping CorruptingPotion_Halved because of the presence of CorruptingPotion");
                                }
                                dot.intDuration -= dot.intInterval;
                                break;
                            case "Torment":
                                if (buffs.Contains(Debuff.Stun) || buffs.Contains(Debuff.Icy))
                                {
                                    ReceiveDamage((float)(0.5 * 0.025 * RuntimeAttributes["MaxHP"]), "AP", dot.source);
                                }
                                else
                                {
                                    ReceiveDamage((float)(0.5 * 0.015 * RuntimeAttributes["MaxHP"]), "AP", dot.source);
                                }
                                dot.intDuration -= dot.intInterval;
                                break;
                            default:
                                ReceiveDamage(dot.fDmgPerTick, dot.strDmgType, dot.source);
                                dot.intDuration -= dot.intInterval;
                                break;
                        }
                    }
                }
                else if (buff is Debuff)
                {
                    Debuff debuff = (Debuff)buff;
                    debuff.intDuration -= updateInterval;
                    if(heroName == "Enemy")
                    Debug.Log("debuff: " + debuff.strName + "Remainig Time: " + debuff.intDuration);
                }else if (buff is Shield)
                {
                    Debug.Log(heroName + " has a Shield: " + ((Shield)buff).strName);
                    buff.intDuration -= updateInterval;
                }
                else if (buff is Buff)
                {
                    switch (buff.strName)
                    {
                        case "InCombat":
                            if(RuntimeAttributes.ContainsKey("Unique_Passive_Madness") && intTime % 100 == 0)
                            {
                                counter.MadnessCount += 1;
                                counter.MadnessCount = Mathf.Clamp(counter.MadnessCount, 0, 5);
                                Debug.Log("Madness stack: " + counter.MadnessCount);
                            }
                            break;
                        default:
                            break;
                    }
                    buff.intDuration -= updateInterval;
                }
            }
        }

        foreach (Buff buff in buffsToBeRemoved)
        {
            if (buff.Equals(Buff.InCombat))
            {
                Debug.Log("Existing Combat");
                counter.MadnessCount = 0;
            }
            if (buff.Equals(Buff.Spellbinder_Active))
            {
                RuntimeAttributes["AP"] -= int.Parse(buff.strDescription);
            }
            if (buff.Equals(Buff.SuddenImpact))
            {
                RuntimeAttributes["Lethality"] -= 7;
                RuntimeAttributes["APPenetration"] -= 6;
                ReceiveBuff(Debuff.SuddentImpactCD);
            }
            buffs.Remove(buff);
        }

        //---Health and mana regen-- -
        //Update every 0.5 second(intTime % 50 == 0)
        if (intTime % 50 == 0)
        {
            ReceiveHealing(RuntimeAttributes["HealthRegen"] * 0.1f);
        }

        UpdateRuntimeAttributes();

    }

    protected void UpdateRuntimeAttributes()
    {
        //fCurrentHealth = RuntimeAttributes["MaxHP"];
        RuntimeAttributes["AP"] = BaseAttributes["AP"];
        RuntimeAttributes["AD"] = BaseAttributes["AD"];

        foreach (Buff bf in buffs)
        {
            if (bf.Equals(Buff.Spellbinder_Active))
            {
                RuntimeAttributes["AP"] = BaseAttributes["AP"] + int.Parse(bf.strDescription);
                Debug.Log("bounous AP from Spellbinder");
            }
            if (bf.Equals(Buff.SuddenImpact))
            {
                RuntimeAttributes["APPenetration"] = BaseAttributes["APPenetration"] + 6;
                RuntimeAttributes["Lethality"] = BaseAttributes["Lethality"] + 7;
                Debug.Log("bounous AP penetration from SuddenImpact");
            }
        }
        
        if (rune.strStones.Contains("AbsoluteFocus"))
        {
            //Debug.Log("Updating AbsoluteFocus Status");
            if (fCurrentHealth >= 0.7 * RuntimeAttributes["MaxHP"])
            {
                if (ApAdapted)
                {
                    RuntimeAttributes["AP"] += (float)(5.0 + 2.06 * (intHeroLevel - 1));
                }
                else
                {
                    RuntimeAttributes["AD"] += (float)(3.0 + 1.24 * (intHeroLevel - 1));
                }
            }
        }
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
                if (buff.intTimeOfStart <= intTime)
                {
                    Debug.Log(buff.strName + " has been refreshed");
                    existingBuff.intDuration = buff.intDuration;

                    //if(existingBuff is DoT)
                    //{
                    //    DoT dot = (DoT)existingBuff;
                    //}
                }
                else
                {
                    Debug.LogError("Unknown bug in refreshing buffs: " + existingBuff.strName);
                }
            }
        }

        if (isNewBuff)
        {
            buff.intTimeOfStart = intTime;
            buffs.Add(buff);
        }
    }

    private void ReceiveDamage(double dDamage, string dmgType, Hero source)
    {
        double damage = 0;
        Amplifier amplifier = source.GetBaseAmplifier();
        switch (dmgType)
        {
            case "AP":
                float fMR = RuntimeAttributes["MR"];
                fMR *= (1 - amplifier.fMRpercentagePenetration);
                fMR -= amplifier.fMRpenetration;
                fMR = Mathf.Clamp(fMR, 0, 9999);
                fMR -= amplifier.fMRreduction;
                foreach (float modifier in amplifier.fPercentageDmgModifiers)
                {
                    dDamage *= (1 + modifier);
                }
                damage = dDamage * (100 / (100 + fMR));
                Debug.Log("Damage is: " + damage + ", with effective enemy MR of " + fMR);
                break;
            case "AD":
                float fArmor = RuntimeAttributes["Armor"];
                foreach (float modifier in amplifier.fPercentageDmgModifiers)
                {
                    dDamage *= (1 + modifier);
                }
                fArmor -= amplifier.fAMPenetration;
                damage = dDamage * (100 / (100 + fArmor));
                Debug.Log("Damage is: " + damage + ", with effective enemy Armor of " + fArmor);
                break;
            case "True":
                damage = dDamage;
                Debug.Log("Damage is: " + damage);
                break;
            case "NotDamage":
                Debug.Log("Not damage");
                break;
            default:
                Debug.Log("Damage Type not recognized");
                damage = 0d;
                break;
        }
        if ((buffs.Contains(Debuff.CoupDeGrace) || amplifier.otherAmplifers.Contains("CoupDeGrace"))
            && (fCurrentHealth / RuntimeAttributes["MaxHP"] <= 0.4 && !dmgType.Equals("True")))
        {
            Debug.Log("CoupDeGraceDmgAmp");
            damage *= 1.07;
        }

        fTotalDmgReceived += (float)damage;

        if (buffs.Contains(Buff.NullifyingOrb)
            && !buffs.Contains(Debuff.NullifyingOrbCD)
            && dmgType.Equals("AP")
            && (fCurrentHealth - damage < 0.30 * RuntimeAttributes["MaxHP"]))
        {
            Debug.Log("Nullifying Orb Triggered");
            ReceiveBuff(new Shield
            {
                strName = "Nullifying_Orb_Shield",
                fStrength = (float)(35.29 + 4.71 * intHeroLevel + 0.1 * RuntimeAttributes["AP"] + 0.15 * (RuntimeAttributes["AD"] - BaseAttributes["BAD"])),
                isWhiteShield = false,
                intDuration = 400
            });
            ReceiveBuff(Debuff.NullifyingOrbCD);
        }

        Debug.Log("Shield for " + heroName + " ?:" + (buffs.Exists(x => (x is Shield)).ToString()));

        if(buffs.Exists(x => (x is Shield)))
        {
            switch (dmgType)
            {
                case "AP":
                    Shield shield =(Shield)buffs.Find(x => (x is Shield && !((Shield)x).isWhiteShield));
                    if(shield == null)
                    {
                        Debug.Log("No black shield in shields");
                    }
                    else
                    {
                        double remainingDmg = Mathf.Clamp((float)(damage - shield.fStrength), 0, 99999);
                        damage = remainingDmg;
                        shield.fStrength -= (float)damage;
                        if(shield.fStrength < 0)
                        {
                            buffs.Remove(shield);
                            Debug.Log("Shield broken");
                        }
                    }

                    break;
                case "AD":
                    break;
                case "True":
                    break;
                default:
                    break;
            }
        }

        fCurrentHealth -= (float)damage;
        Debug.Log("Damage Received = " + damage);
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
        fCurrentHealth = Mathf.Clamp(fCurrentHealth, 0f, RuntimeAttributes["MaxHP"]);
    }

    protected void ReduceBuffTime(string buffName, int timeReduced)
    {
        foreach(Buff buff in buffs)
        {
            if (buff.strName.Equals(buffName))
            {
                buff.intDuration -= timeReduced;
                Debug.Log(buff.strName + " is reduced to " + buff.intDuration);
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
                Debug.Log(buff.strName + " is reduced to " + buff.intDuration);
            }
        }
        if (!flag)
        {
            Debug.LogError("Cannot Reduce buff time: " + buffName + " is not in the buff list");
        }
    }

    public Amplifier GetBaseAmplifier()
    {
        //Debug.Log(strName + " is getting Amplifier");
        float f;
        Amplifier amplifier = new Amplifier();
        if (RuntimeAttributes.TryGetValue("APPenetration", out f)) amplifier.fMRpenetration = f;
        if (RuntimeAttributes.TryGetValue("APPPenetration", out f)) amplifier.fMRpercentagePenetration = f;
        if (rune.strStones.Contains("CoupDeGrace")) amplifier.otherAmplifers.Add("CoupDeGrace");
        if (RuntimeAttributes.TryGetValue("Lethality", out f))
        {
            amplifier.fAMPenetration = GameStatsUtility.LethalityToAMPenetration(f, GetHeroLevel());
        }
        if (RuntimeAttributes.ContainsKey("Unique_Passive_Madness"))
        {
            amplifier.fPercentageDmgModifiers.Add((float)(counter.MadnessCount * 0.02));
        }
        return amplifier;
    }

    public Amplifier GetRuntimeAmplifier()
    {
        return new Amplifier();
    }

    public void LearnNewSpell(string spell)
    {
        heroInfo.spellPanel.NewSpell(spell);
    }

    public void UnlearnSpell(string spell)
    {
        heroInfo.spellPanel.RemoveSpell(new SpellListItem { strSpell = spell, caster = this, receivers = null});
    }

    private void InitializeSpellPanel(string heroName)
    {
        List<string> spellList;
        switch (heroName)
        {
            case "Annie":
                spellList = new List<string>
        {
            "Q",
            "W",
            "E",
            "R",
            "RA",
            "Ignite",
            "Flash",
            "A"
        };
                break;
            default:
                spellList = new List<string>
        {
            "Flash"
        };
                break;

        }
        heroInfo.spellPanel.Initialize(spellList, this);
    }

    public SpellCast CastSpell(BaseSpell baseSpell)
    {
        SpellCastProperty spellCastProperty = null;

        int[] levels = new int[4] { heroInfo.GetLevel("Q"), heroInfo.GetLevel("W"), heroInfo.GetLevel("E"), heroInfo.GetLevel("R") };//0=Q 5=HeroLevel

        Amplifier amplifier = GetBaseAmplifier();

        /*foreach (Buff bf in buffs)
        {
            if (bf.Equals(Buff.Spellbinder_Active))
            {
                RuntimeAttributes["AP"] = BaseAttributes["AP"] + int.Parse(bf.strDescription);
                Debug.Log("bounous AP from Spellbinder");
            }
            if (bf.Equals(Buff.SuddenImpact))
            {
                RuntimeAttributes["APPenetration"] = BaseAttributes["APPenetration"] + 6;
                RuntimeAttributes["Lethality"] = BaseAttributes["Lethality"] + 7;
                Debug.Log("bounous AP penetration from SuddenImpact");
            }
        }*/

        GameDebugUtility.Debug_ShowDictionary("Cast Base Spell ", RuntimeAttributes);

        SpellCast spellcast = new SpellCast();

        string msg;

        switch (baseSpell.spellName)
        {
            case "A":
                spellcast.dDamage = RuntimeAttributes["AD"];
                spellcast.strDmgType = "AD";
                msg = "Auto Attacking, Raw Damage is: " + spellcast.dDamage;
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg, intTime);
                spellCastProperty = new SpellCastProperty()
                {
                    isInCombat = true,
                    isSingleTarget = true,
                    canGiveSpellBlade = false,
                    canTriggerElectrocute = false,
                    canTriggerArcaneComet = false,
                    canTriggerOnHit = true,
                    canTriggerEcho = false,
                    canTriggerCorruptingPotion = true,
                    canStackEcho = false
                };
                break;
            case "Electrocute":
                spellcast.dDamage = 40 + 10 * intHeroLevel + 0.3 * RuntimeAttributes["AP"];
                spellcast.strDmgType = ApAdapted ? "AP" : "AD";
                ReceiveBuff(new Debuff
                {
                    intDuration = (25 - 2 * intHeroLevel) * 100,
                    intTimeOfStart = intTime,
                    strName = "ElectrocuteCD",
                    strDescription = "ElectrocuteCD"
                });
                msg = "Casting Electrocute at level " + intHeroLevel + ", Raw Damage is: " + spellcast.dDamage;
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg, intTime);
                spellCastProperty = new SpellCastProperty()
                {
                    isInCombat = true,
                    isSingleTarget = true,
                    canGiveSpellBlade = false,
                    canTriggerElectrocute = false,
                    canTriggerArcaneComet = false,
                    canTriggerOnHit = false,
                    canTriggerEcho = false,
                    canTriggerCorruptingPotion = true,
                    canStackEcho = false,
                    canTriggerScorch = false
                };
                break;
            case "ArcaneComet":
                spellcast.dDamage = 15.88 + 4.12 * intHeroLevel + 0.2 * RuntimeAttributes["AP"];
                spellcast.strDmgType = ApAdapted ? "AP" : "AD";
                ReceiveBuff(new Debuff
                {
                    intDuration = (int)((20 - 0.71 * (intHeroLevel - 1)) * 100),
                    intTimeOfStart = intTime,
                    strName = "ArcaneCometCD",
                    strDescription = "ArcaneCometCD"
                });
                msg = "Casting ArcaneComet at level " + intHeroLevel + ", Raw Damage is: " + spellcast.dDamage;
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg, intTime);
                spellCastProperty = new SpellCastProperty()
                {
                    isInCombat = true,
                    isSingleTarget = true,
                    canGiveSpellBlade = false,
                    canTriggerElectrocute = false,
                    canTriggerArcaneComet = false,
                    canTriggerOnHit = false,
                    canTriggerEcho = false,
                    canTriggerCorruptingPotion = true,
                    canStackEcho = false,
                    canTriggerScorch = false
                };
                break;
            case "Scorch":
                spellcast.dDamage = 10 + 1.18 * (intHeroLevel - 1);
                spellcast.strDmgType = "AP";
                msg = "Casting Scorch at level " + intHeroLevel + ", Raw Damage is: " + spellcast.dDamage;
                ReceiveBuff(Debuff.ScorchCD);
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg, intTime);
                spellCastProperty = new SpellCastProperty()
                {
                    isInCombat = true,
                    isSingleTarget = true,
                    canGiveSpellBlade = false,
                    canTriggerElectrocute = false,
                    canTriggerArcaneComet = false,
                    canTriggerOnHit = false,
                    canTriggerEcho = false,
                    canTriggerCorruptingPotion = false,
                    canStackEcho = false,
                    canTriggerScorch = false
                };
                break;
            case "Echo":
                spellcast.dDamage = 100 + 0.10 * RuntimeAttributes["AP"];
                spellcast.strDmgType = "AP";
                msg = "Casting Echo, Raw Damage is: " + spellcast.dDamage;
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg, intTime);
                spellCastProperty = new SpellCastProperty()
                {
                    isInCombat = true,
                    isSingleTarget = true,
                    canGiveSpellBlade = false,
                    canTriggerElectrocute = false,
                    canTriggerArcaneComet = false,
                    canTriggerOnHit = false,
                    canTriggerEcho = false,
                    canTriggerCorruptingPotion = true,
                    canStackEcho = false,
                    canTriggerScorch = false
                };
                counter.EchoCount = 0;
                break;
            case "HextechRevolver":
                if (buffs.Contains(Debuff.HextechCD))
                {
                    Debug.Log("Hextech in CD");
                    break;
                }
                spellcast.dDamage = 50 + 4.41 * intHeroLevel;
                spellcast.strDmgType = "AP";
                msg = "Casting HextechRevolver at level " + intHeroLevel + ", Raw Damage is: " + spellcast.dDamage;
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg, intTime);
                spellCastProperty = new SpellCastProperty()
                {
                    isInCombat = true,
                    isSingleTarget = true,
                    canGiveSpellBlade = false,
                    canTriggerElectrocute = false,
                    canTriggerArcaneComet = false,
                    canTriggerOnHit = false,
                    canTriggerEcho = false,
                    canTriggerCorruptingPotion = true,
                    canStackEcho = false,
                    canTriggerScorch = false
                };
                ReceiveBuff(Debuff.HextechCD);
                break;
            case "HextechProtobelt_01":
                if (buffs.Contains(Debuff.HextechCD))
                {
                    Debug.Log("Hextech in CD");
                    break;
                }
                int rockets = (int)RuntimeAttributes["Unique_Active_FireBolt"];
                double oneRocketDmg = 75 + 4.41 * intHeroLevel + 0.25 * RuntimeAttributes["AP"];
                spellcast.dDamage = oneRocketDmg + 0.1 * Mathf.Clamp((rockets - 1), 0, 7);
                spellcast.strDmgType = "AP";
                msg = "Casting Protobelt at level " + intHeroLevel + ", " + rockets + " rockets fired, Raw Damage is: " + spellcast.dDamage;
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg, intTime);
                spellCastProperty = new SpellCastProperty()
                {
                    isInCombat = true,
                    isSingleTarget = false,
                    canGiveSpellBlade = false,
                    canTriggerElectrocute = true,
                    canTriggerArcaneComet = true,
                    canTriggerOnHit = false,
                    canTriggerEcho = true,
                    canTriggerCorruptingPotion = true,
                    canStackEcho = false,
                    canTriggerScorch = false,
                    isDisplacement = true
                };

                break;
            case "HextechGunblade":
                if (buffs.Contains(Debuff.HextechCD))
                {
                    Debug.Log("Hextech in CD");
                    break;
                }
                spellcast.dDamage = 175 + 78/17*(intHeroLevel-1) + 0.30 * RuntimeAttributes["AP"];
                spellcast.strDmgType = "AP";
                msg = "Casting HextechGunblade at level " + intHeroLevel + ", Raw Damage is: " + spellcast.dDamage;
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg, intTime);
                spellCastProperty = new SpellCastProperty()
                {
                    isInCombat = true,
                    isSingleTarget = true,
                    canGiveSpellBlade = false,
                    canTriggerElectrocute = false,
                    canTriggerArcaneComet = false,
                    canTriggerOnHit = false,
                    canTriggerEcho = false,
                    canTriggerCorruptingPotion = true,
                    canStackEcho = false,
                    canTriggerScorch = false
                };
                ReceiveBuff(Debuff.HextechCD);
                break;
            case "SpellBlade_LichBane":
                spellcast.dDamage = 0.75 * RuntimeAttributes["BAD"] + 0.5 * RuntimeAttributes["AP"];
                spellcast.strDmgType = "AP";
                msg = "Casting SpellBlade_LichBane, Raw Damage is: " + spellcast.dDamage;
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg, intTime);
                spellCastProperty = new SpellCastProperty()
                {
                    isInCombat = true,
                    isSingleTarget = true,
                    canGiveSpellBlade = false,
                    canTriggerElectrocute = false,
                    canTriggerArcaneComet = false,
                    canTriggerOnHit = false,
                    canTriggerEcho = false,
                    canTriggerCorruptingPotion = true,
                    canStackEcho = false,
                    canTriggerScorch = false
                };
                ReceiveBuff(Debuff.SpellBladeCD);
                break;
            case "Ignite":
                spellcast.strDmgType = "Not Damage";
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
                    source = this
                });
                spellcast.listBuffs.Add(new Debuff()
                {
                    intDuration = 500,
                    strDescription = "Healing Reduction from Ignite",
                    strName = "Healing Reduction from Ignite"
                });
                msg = "Casting Ignite at level " + intHeroLevel;
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg, intTime);
                spellCastProperty = new SpellCastProperty()
                {
                    isInCombat = true,
                    isSingleTarget = true,
                    canGiveSpellBlade = false,
                    canTriggerElectrocute = false,
                    canTriggerArcaneComet = false,
                    canTriggerOnHit = false,
                    canTriggerEcho = false,
                    canTriggerCorruptingPotion = false,
                    canStackEcho = false,
                    canTriggerScorch = false
                };
                break;
            case "Flash": //Flash will provide a shield
                spellcast.strDmgType = "NotDamage";


                Shield genericShield = new Shield() //For Debug purposes
                {
                    intDuration = 500,
                    strName = "Generic shield",
                    fStrength = 100f,
                    isWhiteShield = true
                };
                ReceiveBuff(genericShield);


                msg = "Flashed";
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg);
                spellCastProperty = new SpellCastProperty()
                {
                    isInCombat = false,
                    isSingleTarget = false,
                    canGiveSpellBlade = false,
                    canTriggerElectrocute = false,
                    canTriggerArcaneComet = false,
                    canTriggerOnHit = false,
                    canTriggerEcho = false,
                    canTriggerCorruptingPotion = false,
                    canStackEcho = false,
                    canTriggerScorch = false,
                    isDisplacement = true
                };
                break;
            case "Spellbinder":
                ReceiveBuff(new Buff()
                {
                    intDuration = 500,
                    source = this,
                    strName = "Spellbinder_Active",
                    strDescription = RuntimeAttributes["Unique_Active_Spellbinder"].ToString()
                });
                msg = "Casting Spellbinder with " + RuntimeAttributes["Unique_Active_Spellbinder"].ToString() + "stacks";
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg);
                spellCastProperty = new SpellCastProperty()
                {
                    isInCombat = false,
                    isSingleTarget = false,
                    canGiveSpellBlade = false,
                    canTriggerElectrocute = false,
                    canTriggerArcaneComet = false,
                    canTriggerOnHit = false,
                    canTriggerEcho = false,
                    canTriggerCorruptingPotion = false,
                    canStackEcho = false,
                    canTriggerScorch = false
                };
                break;
            default:
                Debug.LogError("SpellCastNotRecognized: " + baseSpell.spellName);
                break;
        }

        if (rune.strStones.Contains("Electrocute") && !buffs.Contains(Debuff.ElectrocuteCD) && spellCastProperty.canTriggerElectrocute)
        {
            counter.intElectrocuteCount += 1;
        }

        if (rune.strStones.Contains("ArcaneComet") && spellCastProperty.canTriggerArcaneComet)
        {
            if (!buffs.Contains(Debuff.ArcaneCometCD))
            {
                spellcast.strAdditionalInfo.Add("ArcaneComet");
            }
            else
            {
                if (spellCastProperty.isSingleTarget)
                {
                    ReduceBuffTime("ArcaneCometCD", 0.2f);
                }
                else
                {
                    ReduceBuffTime("ArcaneCometCD", 0.1f);
                }
            }
        }

        if (RuntimeAttributes.ContainsKey("Unique_Passive_TouchOfCorruption") && spellCastProperty.canTriggerCorruptingPotion)
        {
            if (spellCastProperty.isSingleTarget)
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
                    source = this
                });

            }
            else
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
                    strName = "CorruptingPotion_Halved",
                    source = this
                });
            }
        }

        if (RuntimeAttributes.ContainsKey("Unique_Passive_Echo") && spellCastProperty.canTriggerEcho)
        {
            if (counter.EchoCount >= 100)
            {
                spellcast.strAdditionalInfo.Add("Echo");
            }
            else
            {
                if (spellCastProperty.canStackEcho)
                {
                    counter.EchoCount += 10;
                }
            }

        }

        if (RuntimeAttributes.ContainsKey("Unique_Passive_SpellBlade") && !buffs.Contains(Debuff.SpellBladeCD) && spellCastProperty.canGiveSpellBlade)
        {
            ReceiveBuff(Buff.SpellBlade);
        }
        if (spellCastProperty.canTriggerOnHit)
        {
            if (buffs.Contains(Buff.Hextech) && !buffs.Contains(Debuff.HextechCD))
            {
                spellcast.strAdditionalInfo.Add("HextechRevolver");
            }

            if (buffs.Contains(Buff.SpellBlade) && !buffs.Contains(Debuff.SpellBladeCD))
            {
                if (RuntimeAttributes.ContainsKey("Unique_Passive_SpellBlade"))
                {
                    spellcast.strAdditionalInfo.Add("SpellBlade_LichBane");
                }
            }
        }
        if (rune.strStones.Contains("Scorch") && !buffs.Contains(Debuff.ScorchCD) && spellCastProperty.canTriggerScorch)
        {
            spellcast.strAdditionalInfo.Add("Scorch");
        }
        if (RuntimeAttributes.ContainsKey("Unique_Passive_Icy") && spellCastProperty.canTriggerEcho)
        {
            spellcast.listBuffs.Add(Debuff.Icy);
        }
        if (RuntimeAttributes.ContainsKey("Unique_Passive_Torment") && spellCastProperty.canTriggerEcho)
        {
            DoT dot = (DoT)DoT.Torment.MakeCopy();
            dot.source = this;
            spellcast.listBuffs.Add(dot);
        }
        if (spellCastProperty.isInCombat)
        {
            ReceiveBuff(Buff.InCombat);
        }
        if (rune.strStones.Contains("SuddenImpact") && spellCastProperty.isDisplacement && !buffs.Contains(Debuff.SuddentImpactCD))
        {
            ReceiveBuff(Buff.SuddenImpact);
        }


        if (counter.intElectrocuteCount == 3)
        {
            counter.intElectrocuteCount = 0;
            spellcast.strAdditionalInfo.Add("Electrocute");
        }

        spellcast.source = this;
        Debug.Log("Total price: " + RuntimeAttributes["price"]);
        GameDebugUtility.AddDebugMsg("Total price: " + RuntimeAttributes["price"]);
        return spellcast;
    }


}

public class Annie_Test : Hero
{

    public SpellCast CastSpell(string spell)
    {

        SpellCastProperty spellCastProperty = null;

        int[] levels = new int[4] { heroInfo.GetLevel("Q"), heroInfo.GetLevel("W"), heroInfo.GetLevel("E"), heroInfo.GetLevel("R") };//0=Q 5=HeroLevel

        Amplifier amplifier = GetBaseAmplifier();

        /*foreach(Buff bf in buffs)
        {
            if (bf.Equals(Buff.Spellbinder_Active))
            {
                RuntimeAttributes["AP"] = BaseAttributes["AP"] + int.Parse(bf.strDescription);
                Debug.Log("bounous AP from Spellbinder");
            }
            if (bf.Equals(Buff.SuddenImpact))
            {
                RuntimeAttributes["APPenetration"] = BaseAttributes["APPenetration"] + 6;
                RuntimeAttributes["Lethality"] = BaseAttributes["Lethality"] + 7;
                Debug.Log("bounous AP penetration from SuddenImpact");
            }
        }*/

        GameDebugUtility.Debug_ShowDictionary("Cast Spell ", RuntimeAttributes);
        
        SpellCast spellcast = new SpellCast();

        string msg;

        switch (spell)
        {
            case "Q":
                if (!levels[0].Equals(0))
                {
                    spellcast.dDamage = 45 + 35 * levels[0] + 0.8 * RuntimeAttributes["AP"];
                    spellcast.strDmgType = "AP";
                    spellcast.fCooldown = 4f;
                    msg = "Casting Q of level " + levels[0] + ", Raw Damage is: " + spellcast.dDamage;
                    Debug.Log(msg);
                    GameDebugUtility.AddDebugMsg(msg, intTime);

                    spellCastProperty = new SpellCastProperty()
                    {
                        isInCombat = true,
                        isSingleTarget = true,
                        canGiveSpellBlade = true,
                        canTriggerElectrocute = true,
                        canTriggerArcaneComet = true,
                        canTriggerOnHit = false,
                        canTriggerEcho = true,
                        canTriggerCorruptingPotion = true,
                        canStackEcho = true,
                        canTriggerScorch = true
                    };
                }
                else
                {
                    Debug.Log("Q has not been learnt");
                }
                break;
            case "W":
                if (!levels[1].Equals(0))
                {
                    spellcast.dDamage = 25 + 45 * levels[1] + 0.85 * RuntimeAttributes["AP"];
                    spellcast.strDmgType = "AP";
                    spellcast.fCooldown = 6f;
                    msg = "Casting W of level " + levels[1] + ", Raw Damage is: " + spellcast.dDamage;
                    Debug.Log(msg);
                    GameDebugUtility.AddDebugMsg(msg, intTime);
                    spellCastProperty = new SpellCastProperty()
                    {
                        isInCombat = true,
                        isSingleTarget = false,
                        canGiveSpellBlade = true,
                        canTriggerElectrocute = true,
                        canTriggerArcaneComet = true,
                        canTriggerOnHit = false,
                        canTriggerEcho = true,
                        canTriggerCorruptingPotion = true,
                        canStackEcho = true,
                        canTriggerScorch = true
                    };
                    
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
                    spellCastProperty = new SpellCastProperty()
                    {
                        isInCombat = false,
                        isSingleTarget = true,
                        canGiveSpellBlade = true,
                        canTriggerElectrocute = false,
                        canTriggerArcaneComet = false,
                        canTriggerOnHit = false,
                        canTriggerEcho = false,
                        canTriggerCorruptingPotion = false,
                        canStackEcho = true,
                        canTriggerScorch = false
                    };
                    
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
                    spellcast.dDamage = 15 + 15 * levels[2] + 0.3 * RuntimeAttributes["AP"];
                    spellcast.strDmgType = "AP";
                    Debug.Log("Casting Edmg of level " + levels[2] + ", Raw Damage is: " + spellcast.dDamage);
                    spellCastProperty = new SpellCastProperty()
                    {
                        isInCombat = true,
                        isSingleTarget = true,
                        canGiveSpellBlade = false,
                        canTriggerElectrocute = true,
                        canTriggerArcaneComet = true,
                        canTriggerOnHit = false,
                        canTriggerEcho = true,
                        canTriggerCorruptingPotion = true,
                        canStackEcho = false,
                        canTriggerScorch = true
                    };
                   
                   
                }
                break;
            case "R":
                if (!levels[3].Equals(0))
                {
                    spellcast.dDamage = 75 + 80 * levels[3] + 0.65 * RuntimeAttributes["AP"];
                    spellcast.strDmgType = "AP";
                    msg = "Casting R of level " + levels[3] + ", Raw Damage is: " + spellcast.dDamage;
                    Debug.Log(msg);
                    GameDebugUtility.AddDebugMsg(msg, intTime);
                    spellCastProperty = new SpellCastProperty()
                    {
                        isInCombat = true,
                        isSingleTarget = false,
                        canGiveSpellBlade = true,
                        canTriggerElectrocute = true,
                        canTriggerArcaneComet = true,
                        canTriggerOnHit = false,
                        canTriggerEcho = true,
                        canTriggerCorruptingPotion = true,
                        canStackEcho = true,
                        canTriggerScorch = true
                    };
                    spellcast.listBuffs.Add(new DoT()
                    {
                        isDamage = true,
                        intDuration = 500,
                        intInterval = 100,
                        intTickNumber = 5,
                        strDmgType = "AP",
                        fDmgPerTick = (float)(5 + 5 * levels[3] + 0.1 * RuntimeAttributes["AP"]),
                        strDescription = "Tibbers Burn",
                        strName = "Tibbers Burn",
                        source = this
                    });
                    
                   
                }
                else
                {
                    Debug.Log("R has not been learnt");
                }
                break;
            case "RA":
                if (!levels[3].Equals(0))
                {
                    spellcast.dDamage = 25 + 25 * levels[3] + 0.15 * RuntimeAttributes["AP"];
                    spellcast.strDmgType = "AP";
                    msg = "Casting RA of level " + levels[3] + ", Raw Damage is: " + spellcast.dDamage;
                    Debug.Log(msg);
                    GameDebugUtility.AddDebugMsg(msg, intTime);
                    spellCastProperty = new SpellCastProperty()
                    {
                        isInCombat = true,
                        isSingleTarget = true,
                        canGiveSpellBlade = false,
                        canTriggerElectrocute = true,
                        canTriggerArcaneComet = false,
                        canTriggerOnHit = false,
                        canTriggerEcho = true,
                        canTriggerCorruptingPotion = true,
                        canStackEcho = false,
                        canTriggerScorch = true
                    };
                    
                    
                }
                else
                {
                    Debug.Log("R has not been learnt");
                }
                break;
            //Do not discard in case of unknown bugs
            /*case "A":
                spellcast.dDamage = RuntimeAttributes["AD"];
                spellcast.strDmgType = "AD";
                msg = "Auto Attacking, Raw Damage is: " + spellcast.dDamage;
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg, intTime);
                spellCastProperty = new SpellCastProperty()
                {
                    isInCombat = true,
                    isSingleTarget = true,
                    canGiveSpellBlade = false,
                    canTriggerElectrocute = false,
                    canTriggerArcaneComet = false,
                    canTriggerOnHit = true,
                    canTriggerEcho = false,
                    canTriggerCorruptingPotion = true,
                    canStackEcho = false
                };
               
                
                break;
            case "Electrocute":
                spellcast.dDamage = 40 + 10 * intHeroLevel + 0.3 * RuntimeAttributes["AP"];
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
                GameDebugUtility.AddDebugMsg(msg, intTime);
                spellCastProperty = new SpellCastProperty()
                {
                    isInCombat = true,
                    isSingleTarget = true,
                    canGiveSpellBlade = false,
                    canTriggerElectrocute = false,
                    canTriggerArcaneComet = false,
                    canTriggerOnHit = false,
                    canTriggerEcho = false,
                    canTriggerCorruptingPotion = true,
                    canStackEcho = false,
                    canTriggerScorch = false
                };
                break;
            case "ArcaneComet":
                spellcast.dDamage = 15.88 + 4.12 * intHeroLevel + 0.2 * RuntimeAttributes["AP"];
                spellcast.strDmgType = "AP";
                ReceiveBuff(new Debuff
                {
                    intDuration = (int)((20 - 0.71 * (intHeroLevel-1)) * 100),
                    intTimeOfStart = intTime,
                    strName = "ArcaneCometCD",
                    strDescription = "ArcaneCometCD"
                });
                msg = "Casting ArcaneComet at level " + intHeroLevel + ", Raw Damage is: " + spellcast.dDamage;
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg, intTime);
                spellCastProperty = new SpellCastProperty()
                {
                    isInCombat = true,
                    isSingleTarget = true,
                    canGiveSpellBlade = false,
                    canTriggerElectrocute = false,
                    canTriggerArcaneComet = false,
                    canTriggerOnHit = false,
                    canTriggerEcho = false,
                    canTriggerCorruptingPotion = true,
                    canStackEcho = false,
                    canTriggerScorch = false
                };
                break;
            case "Scorch":
                spellcast.dDamage = 10 + 1.18 * (intHeroLevel - 1);
                spellcast.strDmgType = "AP";
                msg = "Casting Scorch at level " + intHeroLevel + ", Raw Damage is: " + spellcast.dDamage;
                ReceiveBuff(Debuff.ScorchCD);
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg, intTime);
                spellCastProperty = new SpellCastProperty()
                {
                    isInCombat = true,
                    isSingleTarget = true,
                    canGiveSpellBlade = false,
                    canTriggerElectrocute = false,
                    canTriggerArcaneComet = false,
                    canTriggerOnHit = false,
                    canTriggerEcho = false,
                    canTriggerCorruptingPotion = false,
                    canStackEcho = false,
                    canTriggerScorch = false
                };
                break;
            case "Echo":
                spellcast.dDamage = 100 + 0.10 * RuntimeAttributes["AP"];
                spellcast.strDmgType = "AP";
                msg = "Casting Echo, Raw Damage is: " + spellcast.dDamage;
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg, intTime);
                spellCastProperty = new SpellCastProperty()
                {
                    isInCombat = true,
                    isSingleTarget = true,
                    canGiveSpellBlade = false,
                    canTriggerElectrocute = false,
                    canTriggerArcaneComet = false,
                    canTriggerOnHit = false,
                    canTriggerEcho = false,
                    canTriggerCorruptingPotion = true,
                    canStackEcho = false,
                    canTriggerScorch = false
                };
                counter.EchoCount = 0;
                break;
            case "HextechRevolver":
                spellcast.dDamage = 50 + 4.41 * intHeroLevel;
                spellcast.strDmgType = "AP";
                msg = "Casting HextechRevolver at level " + intHeroLevel + ", Raw Damage is: " + spellcast.dDamage;
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg, intTime);
                spellCastProperty = new SpellCastProperty()
                {
                    isInCombat = true,
                    isSingleTarget = true,
                    canGiveSpellBlade = false,
                    canTriggerElectrocute = false,
                    canTriggerArcaneComet = false,
                    canTriggerOnHit = false,
                    canTriggerEcho = false,
                    canTriggerCorruptingPotion = true,
                    canStackEcho = false,
                    canTriggerScorch = false
                };
                ReceiveBuff(Debuff.HextechCD);
                break;
            case "HextechProtobelt_01":
                int rockets = (int)RuntimeAttributes["Unique_Active_FireBolt"];
                double oneRocketDmg = 75 + 4.41 * intHeroLevel + 0.25 * RuntimeAttributes["AP"];
                spellcast.dDamage = oneRocketDmg + 0.1 * Mathf.Clamp((rockets - 1), 0, 7);
                spellcast.strDmgType = "AP";
                msg = "Casting Protobelt at level " + intHeroLevel + ", " + rockets + " rockets fired, Raw Damage is: " + spellcast.dDamage;
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg, intTime);
                spellCastProperty = new SpellCastProperty()
                {
                    isInCombat = true,
                    isSingleTarget = false,
                    canGiveSpellBlade = false,
                    canTriggerElectrocute = true,
                    canTriggerArcaneComet = true,
                    canTriggerOnHit = false,
                    canTriggerEcho = true,
                    canTriggerCorruptingPotion = true,
                    canStackEcho = false,
                    canTriggerScorch = false,
                    isDisplacement = true
                };
             
                break;
            case "SpellBlade_LichBane":
                spellcast.dDamage = 0.75 * RuntimeAttributes["BAD"] + 0.5 * RuntimeAttributes["AP"];
                spellcast.strDmgType = "AP";
                msg = "Casting SpellBlade_LichBane, Raw Damage is: " + spellcast.dDamage;
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg, intTime);
                spellCastProperty = new SpellCastProperty()
                {
                    isInCombat = true,
                    isSingleTarget = true,
                    canGiveSpellBlade = false,
                    canTriggerElectrocute = false,
                    canTriggerArcaneComet = false,
                    canTriggerOnHit = false,
                    canTriggerEcho = false,
                    canTriggerCorruptingPotion = true,
                    canStackEcho = false,
                    canTriggerScorch = false
                };
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
                    source = this
                });
                spellcast.listBuffs.Add(new Debuff()
                {
                    intDuration = 500,
                    strDescription = "Healing Reduction from Ignite",
                    strName = "Healing Reduction from Ignite"
                });
                msg = "Casting Ignite at level " + intHeroLevel;
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg, intTime);
                spellCastProperty = new SpellCastProperty()
                {
                    isInCombat = true,
                    isSingleTarget = true,
                    canGiveSpellBlade = false,
                    canTriggerElectrocute = false,
                    canTriggerArcaneComet = false,
                    canTriggerOnHit = false,
                    canTriggerEcho = false,
                    canTriggerCorruptingPotion = false,
                    canStackEcho = false,
                    canTriggerScorch = false
                };
                break;
            case "Flash":
                spellcast.strDmgType = "NotDamage";
                msg = "Flashed";
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg);
                spellCastProperty = new SpellCastProperty()
                {
                    isInCombat = false,
                    isSingleTarget = false,
                    canGiveSpellBlade = false,
                    canTriggerElectrocute = false,
                    canTriggerArcaneComet = false,
                    canTriggerOnHit = false,
                    canTriggerEcho = false,
                    canTriggerCorruptingPotion = false,
                    canStackEcho = false,
                    canTriggerScorch = false,
                    isDisplacement = true
                };
                break;
            case "Spellbinder":
                ReceiveBuff(new Buff()
                {
                    intDuration = 500,
                    source = this,
                    strName = "Spellbinder_Active",
                    strDescription = RuntimeAttributes["Unique_Active_Spellbinder"].ToString()
                });
                msg = "Casting Spellbinder with " + RuntimeAttributes["Unique_Active_Spellbinder"].ToString() + "stacks";
                Debug.Log(msg);
                GameDebugUtility.AddDebugMsg(msg);
                spellCastProperty = new SpellCastProperty()
                {
                    isInCombat = false,
                    isSingleTarget = false,
                    canGiveSpellBlade = false,
                    canTriggerElectrocute = false,
                    canTriggerArcaneComet = false,
                    canTriggerOnHit = false,
                    canTriggerEcho = false,
                    canTriggerCorruptingPotion = false,
                    canStackEcho = false,
                    canTriggerScorch = false
                };
                break;
            */
            default:
                Debug.LogError("SpellCastNotRecognized:" + spell);
                break;
        }

        if (rune.strStones.Contains("Electrocute") && !buffs.Contains(Debuff.ElectrocuteCD) && spellCastProperty.canTriggerElectrocute)
        {
            counter.intElectrocuteCount += 1;
        }

        if (rune.strStones.Contains("ArcaneComet") && spellCastProperty.canTriggerArcaneComet)
        {
            if (!buffs.Contains(Debuff.ArcaneCometCD))
            {
                spellcast.strAdditionalInfo.Add("ArcaneComet");
            }
            else
            {
                if (spellCastProperty.isSingleTarget)
                {
                    ReduceBuffTime("ArcaneCometCD", 0.2f);
                }
                else
                {
                    ReduceBuffTime("ArcaneCometCD", 0.1f);
                }
            }
        }

        if (RuntimeAttributes.ContainsKey("Unique_Passive_TouchOfCorruption") && spellCastProperty.canTriggerCorruptingPotion)
        {
            if (spellCastProperty.isSingleTarget)
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
                    source = this
                });

            }
            else
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
                    strName = "CorruptingPotion_Halved",
                    source = this
                });
            }
        }

        if (RuntimeAttributes.ContainsKey("Unique_Passive_Echo") && spellCastProperty.canTriggerEcho)
        {
            if (counter.EchoCount >= 100)
            {
                spellcast.strAdditionalInfo.Add("Echo");
            }
            else
            {
                if (spellCastProperty.canStackEcho)
                {
                    counter.EchoCount += 10;
                }
            }

        }

        if (RuntimeAttributes.ContainsKey("Unique_Passive_SpellBlade") && !buffs.Contains(Debuff.SpellBladeCD)&&spellCastProperty.canGiveSpellBlade)
        {
            ReceiveBuff(Buff.SpellBlade);
        }
        if (spellCastProperty.canTriggerOnHit)
        {
            if (buffs.Contains(Buff.Hextech) && !buffs.Contains(Debuff.HextechCD))
            {
                spellcast.strAdditionalInfo.Add("HextechRevolver");
            }

            if (buffs.Contains(Buff.SpellBlade) && !buffs.Contains(Debuff.SpellBladeCD))
            {
                if (RuntimeAttributes.ContainsKey("Unique_Passive_SpellBlade"))
                {
                    spellcast.strAdditionalInfo.Add("SpellBlade_LichBane");
                }
            }
        }
        if (rune.strStones.Contains("Scorch") && !buffs.Contains(Debuff.ScorchCD) && spellCastProperty.canTriggerScorch)
        {
            spellcast.strAdditionalInfo.Add("Scorch");
        }
        if(RuntimeAttributes.ContainsKey("Unique_Passive_Icy") && spellCastProperty.canTriggerEcho)
        {
            spellcast.listBuffs.Add(Debuff.Icy);
        }
        if (RuntimeAttributes.ContainsKey("Unique_Passive_Torment") && spellCastProperty.canTriggerEcho)
        {
            DoT dot = (DoT)DoT.Torment.MakeCopy();
            dot.source = this;
            spellcast.listBuffs.Add(dot);
        }
        if (spellCastProperty.isInCombat)
        {
            ReceiveBuff(Buff.InCombat);
        }
        if (rune.strStones.Contains("SuddenImpact") && spellCastProperty.isDisplacement && !buffs.Contains(Debuff.SuddentImpactCD))
        {
            ReceiveBuff(Buff.SuddenImpact);
        }


        if (counter.intElectrocuteCount == 3)
        {
            counter.intElectrocuteCount = 0;
            spellcast.strAdditionalInfo.Add("Electrocute");
        }

        spellcast.source = this;
        Debug.Log("Total price: " + RuntimeAttributes["price"]);
        GameDebugUtility.AddDebugMsg("Total price: " + RuntimeAttributes["price"]);
        return spellcast;
    }
}

public class Enemy_Test : Hero
{
    public SpellCast CastSpell(string spell)
    {

        SpellCastProperty spellCastProperty = null;

        int[] levels = new int[4] { heroInfo.GetLevel("Q"), heroInfo.GetLevel("W"), heroInfo.GetLevel("E"), heroInfo.GetLevel("R") };//0=Q 5=HeroLevel

        Amplifier amplifier = GetBaseAmplifier();

        GameDebugUtility.Debug_ShowDictionary("Cast Spell ", RuntimeAttributes);

        SpellCast spellcast = new SpellCast();

        string msg;

        switch (spell)
        {
            case "Q":
                if (!levels[0].Equals(0))
                {
                    spellcast.dDamage = 45 + 35 * levels[0] + 0.8 * RuntimeAttributes["AP"];
                    spellcast.strDmgType = "AP";
                    spellcast.fCooldown = 4f;
                    msg = "Casting Q of level " + levels[0] + ", Raw Damage is: " + spellcast.dDamage;
                    Debug.Log(msg);
                    GameDebugUtility.AddDebugMsg(msg, intTime);

                    spellCastProperty = new SpellCastProperty()
                    {
                        isInCombat = true,
                        isSingleTarget = true,
                        canGiveSpellBlade = true,
                        canTriggerElectrocute = true,
                        canTriggerArcaneComet = true,
                        canTriggerOnHit = false,
                        canTriggerEcho = true,
                        canTriggerCorruptingPotion = true,
                        canStackEcho = true,
                        canTriggerScorch = true
                    };
                }
                else
                {
                    Debug.Log("Q has not been learnt");
                }
                break;
            case "W":
                if (!levels[1].Equals(0))
                {
                    spellcast.dDamage = 25 + 45 * levels[1] + 0.85 * RuntimeAttributes["AP"];
                    spellcast.strDmgType = "AP";
                    spellcast.fCooldown = 6f;
                    msg = "Casting W of level " + levels[1] + ", Raw Damage is: " + spellcast.dDamage;
                    Debug.Log(msg);
                    GameDebugUtility.AddDebugMsg(msg, intTime);
                    spellCastProperty = new SpellCastProperty()
                    {
                        isInCombat = true,
                        isSingleTarget = false,
                        canGiveSpellBlade = true,
                        canTriggerElectrocute = true,
                        canTriggerArcaneComet = true,
                        canTriggerOnHit = false,
                        canTriggerEcho = true,
                        canTriggerCorruptingPotion = true,
                        canStackEcho = true,
                        canTriggerScorch = true
                    };

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
                    spellCastProperty = new SpellCastProperty()
                    {
                        isInCombat = false,
                        isSingleTarget = true,
                        canGiveSpellBlade = true,
                        canTriggerElectrocute = false,
                        canTriggerArcaneComet = false,
                        canTriggerOnHit = false,
                        canTriggerEcho = false,
                        canTriggerCorruptingPotion = false,
                        canStackEcho = true,
                        canTriggerScorch = false
                    };

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
                    spellcast.dDamage = 15 + 15 * levels[2] + 0.3 * RuntimeAttributes["AP"];
                    spellcast.strDmgType = "AP";
                    Debug.Log("Casting Edmg of level " + levels[2] + ", Raw Damage is: " + spellcast.dDamage);
                    spellCastProperty = new SpellCastProperty()
                    {
                        isInCombat = true,
                        isSingleTarget = true,
                        canGiveSpellBlade = false,
                        canTriggerElectrocute = true,
                        canTriggerArcaneComet = true,
                        canTriggerOnHit = false,
                        canTriggerEcho = true,
                        canTriggerCorruptingPotion = true,
                        canStackEcho = false,
                        canTriggerScorch = true
                    };


                }
                break;
            case "R":
                if (!levels[3].Equals(0))
                {
                    spellcast.dDamage = 75 + 80 * levels[3] + 0.65 * RuntimeAttributes["AP"];
                    spellcast.strDmgType = "AP";
                    msg = "Casting R of level " + levels[3] + ", Raw Damage is: " + spellcast.dDamage;
                    Debug.Log(msg);
                    GameDebugUtility.AddDebugMsg(msg, intTime);
                    spellCastProperty = new SpellCastProperty()
                    {
                        isInCombat = true,
                        isSingleTarget = false,
                        canGiveSpellBlade = true,
                        canTriggerElectrocute = true,
                        canTriggerArcaneComet = true,
                        canTriggerOnHit = false,
                        canTriggerEcho = true,
                        canTriggerCorruptingPotion = true,
                        canStackEcho = true,
                        canTriggerScorch = true
                    };
                    spellcast.listBuffs.Add(new DoT()
                    {
                        isDamage = true,
                        intDuration = 500,
                        intInterval = 100,
                        intTickNumber = 5,
                        strDmgType = "AP",
                        fDmgPerTick = (float)(5 + 5 * levels[3] + 0.1 * RuntimeAttributes["AP"]),
                        strDescription = "Tibbers Burn",
                        strName = "Tibbers Burn",
                        source = this
                    });


                }
                else
                {
                    Debug.Log("R has not been learnt");
                }
                break;
            case "RA":
                if (!levels[3].Equals(0))
                {
                    spellcast.dDamage = 25 + 25 * levels[3] + 0.15 * RuntimeAttributes["AP"];
                    spellcast.strDmgType = "AP";
                    msg = "Casting RA of level " + levels[3] + ", Raw Damage is: " + spellcast.dDamage;
                    Debug.Log(msg);
                    GameDebugUtility.AddDebugMsg(msg, intTime);
                    spellCastProperty = new SpellCastProperty()
                    {
                        isInCombat = true,
                        isSingleTarget = true,
                        canGiveSpellBlade = false,
                        canTriggerElectrocute = true,
                        canTriggerArcaneComet = false,
                        canTriggerOnHit = false,
                        canTriggerEcho = true,
                        canTriggerCorruptingPotion = true,
                        canStackEcho = false,
                        canTriggerScorch = true
                    };


                }
                else
                {
                    Debug.Log("R has not been learnt");
                }
                break;
            default:
                Debug.LogError("SpellCastNotRecognized:" + spell);
                break;
        }

        if (rune.strStones.Contains("Electrocute") && !buffs.Contains(Debuff.ElectrocuteCD) && spellCastProperty.canTriggerElectrocute)
        {
            counter.intElectrocuteCount += 1;
        }

        if (rune.strStones.Contains("ArcaneComet") && spellCastProperty.canTriggerArcaneComet)
        {
            if (!buffs.Contains(Debuff.ArcaneCometCD))
            {
                spellcast.strAdditionalInfo.Add("ArcaneComet");
            }
            else
            {
                if (spellCastProperty.isSingleTarget)
                {
                    ReduceBuffTime("ArcaneCometCD", 0.2f);
                }
                else
                {
                    ReduceBuffTime("ArcaneCometCD", 0.1f);
                }
            }
        }

        if (RuntimeAttributes.ContainsKey("Unique_Passive_TouchOfCorruption") && spellCastProperty.canTriggerCorruptingPotion)
        {
            if (spellCastProperty.isSingleTarget)
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
                    source = this
                });

            }
            else
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
                    strName = "CorruptingPotion_Halved",
                    source = this
                });
            }
        }

        if (RuntimeAttributes.ContainsKey("Unique_Passive_Echo") && spellCastProperty.canTriggerEcho)
        {
            if (counter.EchoCount >= 100)
            {
                spellcast.strAdditionalInfo.Add("Echo");
            }
            else
            {
                if (spellCastProperty.canStackEcho)
                {
                    counter.EchoCount += 10;
                }
            }

        }

        if (RuntimeAttributes.ContainsKey("Unique_Passive_SpellBlade") && !buffs.Contains(Debuff.SpellBladeCD) && spellCastProperty.canGiveSpellBlade)
        {
            ReceiveBuff(Buff.SpellBlade);
        }
        if (spellCastProperty.canTriggerOnHit)
        {
            if (buffs.Contains(Buff.Hextech) && !buffs.Contains(Debuff.HextechCD))
            {
                spellcast.strAdditionalInfo.Add("HextechRevolver");
            }

            if (buffs.Contains(Buff.SpellBlade) && !buffs.Contains(Debuff.SpellBladeCD))
            {
                if (RuntimeAttributes.ContainsKey("Unique_Passive_SpellBlade"))
                {
                    spellcast.strAdditionalInfo.Add("SpellBlade_LichBane");
                }
            }
        }
        if (rune.strStones.Contains("Scorch") && !buffs.Contains(Debuff.ScorchCD) && spellCastProperty.canTriggerScorch)
        {
            spellcast.strAdditionalInfo.Add("Scorch");
        }
        if (RuntimeAttributes.ContainsKey("Unique_Passive_Icy") && spellCastProperty.canTriggerEcho)
        {
            spellcast.listBuffs.Add(Debuff.Icy);
        }
        if (RuntimeAttributes.ContainsKey("Unique_Passive_Torment") && spellCastProperty.canTriggerEcho)
        {
            DoT dot = (DoT)DoT.Torment.MakeCopy();
            dot.source = this;
            spellcast.listBuffs.Add(dot);
        }
        if (spellCastProperty.isInCombat)
        {
            ReceiveBuff(Buff.InCombat);
        }
        if (rune.strStones.Contains("SuddenImpact") && spellCastProperty.isDisplacement && !buffs.Contains(Debuff.SuddentImpactCD))
        {
            ReceiveBuff(Buff.SuddenImpact);
        }


        if (counter.intElectrocuteCount == 3)
        {
            counter.intElectrocuteCount = 0;
            spellcast.strAdditionalInfo.Add("Electrocute");
        }

        spellcast.source = this;
        Debug.Log("Total price: " + RuntimeAttributes["price"]);
        GameDebugUtility.AddDebugMsg("Total price: " + RuntimeAttributes["price"]);
        return spellcast;
    }
}

public class SpellCastProperty {
    public bool isInCombat = false;
    public bool isSingleTarget = false;
    public bool canTriggerOnHit = false;
    public bool canTriggerElectrocute = false;
    public bool canTriggerArcaneComet =false;
    public bool canGiveSpellBlade=false;
    public bool canTriggerCorruptingPotion = false;
    public bool canTriggerEcho = false;
    public bool canStackEcho = true;
    public bool canTriggerScorch = false;
    public bool isDisplacement = false;
}
public class BaseSpell
{
    public string spellName;
    public static BaseSpell A = new BaseSpell { spellName = "A" };
    public static BaseSpell Electrocute = new BaseSpell { spellName = "Electrocute" };
    public static BaseSpell ArcaneComet = new BaseSpell { spellName = "ArcaneComet" };
    public static BaseSpell Scorch = new BaseSpell { spellName = "Scorch" };
    public static BaseSpell Echo = new BaseSpell { spellName = "Echo" };
    public static BaseSpell HextechRevolver = new BaseSpell { spellName = "HextechRevolver" };
    public static BaseSpell HextechProtobelt_01 = new BaseSpell { spellName = "HextechProtobelt_01" };
    public static BaseSpell HextechGunblade = new BaseSpell { spellName = "HextechGunblade" };
    public static BaseSpell SpellBlade_LichBane = new BaseSpell { spellName = "SpellBlade_LichBane" };
    public static BaseSpell Spellbinder = new BaseSpell { spellName = "Spellbinder" };
    public static BaseSpell Flash = new BaseSpell { spellName = "Flash" };
    public static BaseSpell Ignite = new BaseSpell { spellName = "Ignite" };
}