using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class Processor : MonoBehaviour//Attached to HomeScreen
{
    public ItemList itemList;
    public DisplayPanel displayPanel;
    public AdvancedSetting advancedSetting;
    public Text logText;

    public GameObject heroInfoPrefab;
    public Transform heroInfoHolderTransform;

    public int totalTime;//Set in inspector
    private int intTime;
    private ItemSlot currentItemSlot;
    private Annie_Test annie;
    private Enemy_Test enemy;
    private List<SpellListItem> spellList;

    private List<DoT> dotDamages;
    private int intUpdateInterval = 10;

    public void Awake()
    {
        GameDebugUtility.Initialize();
        ClearLogText();
    }

    public void Start()
    {
        annie = new Annie_Test();
        annie.Initialize("Annie", heroInfoPrefab, heroInfoHolderTransform, this);
        
        enemy = new Enemy_Test();
        enemy.Initialize("Enemy", heroInfoPrefab, heroInfoHolderTransform, this);

        displayPanel.Initialize();
    }

    public void Update()// for debugging
    {
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log(annie.GetRune().ToString());
            GameDebugUtility.ShowAllDebugMsg();
        }
    }

    public void OpenItemList(ItemSlot itemSlot)
    {
        itemList.gameObject.SetActive(true);
        itemList.OpenItemList();
        currentItemSlot = itemSlot;
    }

    public void OpenAdvanceSetting(Item item)
    {
        advancedSetting.gameObject.SetActive(true);
        advancedSetting.Initialize(item);
    }

    public void EquipItem(Item item)
    {
        Debug.Log("Equipping item: item in itemSlot " + currentItemSlot.itemSlotNumber);
        UnequipItem();
        currentItemSlot.EquipItem(item.MakeCopy());//deep copying
        List<string> possibleActives = new List<string>() {
            "SkirmishersSabre", "GargoyleStonePlate",
            "HextechGLP_800", "HextechGunblade","HextechProtobelt_01","ShurelyasReverie","Spellbinder" };

        foreach (string active in possibleActives)
        {
            if (item.strName == active)
            {
                currentItemSlot.owner.LearnNewSpell(active);
            }
        }

        itemList.CloseItemList();
    }

    public void UnequipItem()
    {
        Debug.Log("Unequipping item");
        Item unequippedItem = currentItemSlot.UnequipItem();
        if (unequippedItem != null)
        {
            Debug.Log("Item unequipped: " + unequippedItem.strName);
            currentItemSlot.owner.UnlearnSpell(unequippedItem.strName);
        }
        itemList.CloseItemList();

    }

    

    public void Calculate()
    {
        intTime = 0;
        annie.PrepareToCastSpells();
        enemy.PrepareToReceiveSpells();
        spellList = displayPanel.SubmitSpells();
        Debug.Log("----------------Calculating... ----------------");

        Dictionary<int, SpellListItem> spellCastSequence = new Dictionary<int, SpellListItem>();
        for (int i = 0; i < 100; i++)
        {
            if (i < spellList.Count)
            {
                spellCastSequence.Add((intTime + i * 0), spellList[i]);
            }
            intTime += 10;
        }

        Calculate(spellCastSequence);
        return;
    }

    public void Calculate(Dictionary<int, string> spellCastsSequence)//Not in use
    {
        Debug.Log("Calculating using advanced technologies... ");
        GameDebugUtility.AddDebugMsg("--------------Calculating using advanced technologies...--------- ");
        intTime = 0;
        for (int i = 0; i < totalTime*10; i++)
        {
            if (spellCastsSequence.ContainsKey(intTime))
            {
                List<SpellCast> spellCasts =  new List<SpellCast>();
                SpellCast spellCast;
                switch (spellCastsSequence[intTime])
                {
                    case "A":
                        spellCast = annie.CastSpell(BaseSpell.A);
                        break;
                    case "HextechProtobelt_01":
                        spellCast = annie.CastSpell(BaseSpell.HextechProtobelt_01);
                        break;
                    case "HextechGunblade":
                        spellCast = annie.CastSpell(BaseSpell.HextechGunblade);
                        break;
                    case "Spellbinder":
                        spellCast = annie.CastSpell(BaseSpell.Spellbinder);
                        break;
                    case "Flash":
                        spellCast = annie.CastSpell(BaseSpell.Flash);
                        break;
                    case "Ignite":
                        spellCast = annie.CastSpell(BaseSpell.Ignite);
                        break;
                    default:
                        Debug.Log("Current casting: " + spellCastsSequence[intTime]);
                        spellCast = annie.CastSpell(spellCastsSequence[intTime]);
                        break;
                }

                spellCasts.Add(spellCast);

                foreach (string addInfo in spellCast.strAdditionalInfo)
                {
                    if (addInfo.Equals("Electrocute"))
                    {
                        spellCasts.Add(annie.CastSpell("Electrocute"));
                    }
                    if (addInfo.Equals("ArcaneComet"))
                    {
                        spellCasts.Add(annie.CastSpell("ArcaneComet"));
                    }
                    if (addInfo.Equals("Echo"))
                    {
                        spellCasts.Add(annie.CastSpell("Echo"));
                    }
                    if (addInfo.Equals("HextechRevolver"))
                    {
                        spellCasts.Add(annie.CastSpell("HextechRevolver"));
                    }
                    if (addInfo.Equals("SpellBlade_LichBane"))
                    {
                        spellCasts.Add(annie.CastSpell("SpellBlade_LichBane"));
                    }
                    if (addInfo.EndsWith("Scorch"))
                    {
                        spellCasts.Add(annie.CastSpell("Scorch"));
                    }
                }
                enemy.Update(spellCasts);

            }
            else
            {
                enemy.Update();
            }

            annie.Update();
            
            intTime += intUpdateInterval;
        }
        logText.text = GameDebugUtility.ShowAllDebugMsg();
    }

    public void Calculate(Dictionary<int,SpellListItem> spellCastsSequence)
    {
        Debug.Log("Calculating using advanced technologies... ");
        GameDebugUtility.AddDebugMsg("--------------Calculating using advanced technologies...--------- ");
        intTime = 0;
        for (int i = 0; i < totalTime * 10; i++)
        {
            if (spellCastsSequence.ContainsKey(intTime))
            {
                if (spellCastsSequence[intTime].caster.heroName.Equals("Annie"))
                {
                    List<SpellCast> spellCasts = new List<SpellCast>();
                    SpellCast spellCast;
                    switch (spellCastsSequence[intTime].strSpell)
                    {
                        case "A":
                            spellCast = annie.CastSpell(BaseSpell.A);
                            break;
                        case "HextechProtobelt_01":
                            spellCast = annie.CastSpell(BaseSpell.HextechProtobelt_01);
                            break;
                        case "HextechGunblade":
                            spellCast = annie.CastSpell(BaseSpell.HextechGunblade);
                            break;
                        case "Spellbinder":
                            spellCast = annie.CastSpell(BaseSpell.Spellbinder);
                            break;
                        case "Flash":
                            spellCast = annie.CastSpell(BaseSpell.Flash);
                            break;
                        case "Ignite":
                            spellCast = annie.CastSpell(BaseSpell.Ignite);
                            break;
                        default:
                            Debug.Log("Current casting: " + spellCastsSequence[intTime]);
                            spellCast = annie.CastSpell(spellCastsSequence[intTime].strSpell);
                            break;
                    }
                    spellCasts.Add(spellCast);
                    foreach (string addInfo in spellCast.strAdditionalInfo)
                    {
                        if (addInfo.Equals("Electrocute"))
                        {
                            spellCasts.Add(annie.CastSpell(BaseSpell.Electrocute));
                        }
                        if (addInfo.Equals("ArcaneComet"))
                        {
                            spellCasts.Add(annie.CastSpell(BaseSpell.ArcaneComet));
                        }
                        if (addInfo.Equals("Echo"))
                        {
                            spellCasts.Add(annie.CastSpell(BaseSpell.Echo));
                        }
                        if (addInfo.Equals("HextechRevolver"))
                        {
                            spellCasts.Add(annie.CastSpell(BaseSpell.HextechRevolver));
                        }
                        if (addInfo.Equals("SpellBlade_LichBane"))
                        {
                            spellCasts.Add(annie.CastSpell(BaseSpell.SpellBlade_LichBane));
                        }
                        if (addInfo.EndsWith("Scorch"))
                        {
                            spellCasts.Add(annie.CastSpell(BaseSpell.Scorch));
                        }
                    }
                    enemy.Update(spellCasts);
                }
                else if (spellCastsSequence[intTime].caster.heroName.Equals("Enemy"))
                {
                    List<SpellCast> spellCasts = new List<SpellCast>();
                    SpellCast spellCast;
                    switch (spellCastsSequence[intTime].strSpell)
                    {
                        case "A":
                            spellCast = enemy.CastSpell(BaseSpell.A);
                            break;
                        case "HextechProtobelt_01":
                            spellCast = enemy.CastSpell(BaseSpell.HextechProtobelt_01);
                            break;
                        case "HextechGunblade":
                            spellCast = enemy.CastSpell(BaseSpell.HextechGunblade);
                            break;
                        case "Spellbinder":
                            spellCast = enemy.CastSpell(BaseSpell.Spellbinder);
                            break;
                        case "Flash":
                            spellCast = enemy.CastSpell(BaseSpell.Flash);
                            break;
                        case "Ignite":
                            spellCast = enemy.CastSpell(BaseSpell.Ignite);
                            break;
                        default:
                            Debug.Log("Current casting: " + spellCastsSequence[intTime]);
                            spellCast = enemy.CastSpell(spellCastsSequence[intTime].strSpell);
                            break;
                    }
                    spellCasts.Add(spellCast);
                    foreach (string addInfo in spellCast.strAdditionalInfo)
                    {
                        if (addInfo.Equals("Electrocute"))
                        {
                            spellCasts.Add(enemy.CastSpell(BaseSpell.Electrocute));
                        }
                        if (addInfo.Equals("ArcaneComet"))
                        {
                            spellCasts.Add(enemy.CastSpell(BaseSpell.ArcaneComet));
                        }
                        if (addInfo.Equals("Echo"))
                        {
                            spellCasts.Add(enemy.CastSpell(BaseSpell.Echo));
                        }
                        if (addInfo.Equals("HextechRevolver"))
                        {
                            spellCasts.Add(enemy.CastSpell(BaseSpell.HextechRevolver));
                        }
                        if (addInfo.Equals("SpellBlade_LichBane"))
                        {
                            spellCasts.Add(enemy.CastSpell(BaseSpell.SpellBlade_LichBane));
                        }
                        if (addInfo.EndsWith("Scorch"))
                        {
                            spellCasts.Add(enemy.CastSpell(BaseSpell.Scorch));
                        }
                    }
                    annie.Update(spellCasts);
                }
            }
            else
            {
                enemy.Update();
            }

            annie.Update();

            intTime += intUpdateInterval;
        }
        logText.text = GameDebugUtility.ShowAllDebugMsg();
    }

    public void ClearLogText()
    {
        GameDebugUtility.Initialize();
        logText.text = "";
    }
}
public class SpellCast
{
    public double dDamage;
    public Hero source;
    
    public List<string> strAdditionalInfo = new List<string>(){""};
    public float fCooldown = 0f;
    public List<Buff> listBuffs = new List<Buff>();
    public string strDmgType;

    public static SpellCast endOfSequence = new SpellCast() { strDmgType = "End" };
    public new string ToString()
    {
        StringBuilder str = new StringBuilder();
        str.Append("spellcast info");
        str.Append("\ndDamage = ");
        str.Append(dDamage);
        str.Append("\nstrDamageType = ");
        str.Append(strDmgType);
        str.Append("\nstrAdditionalInfo = ");
        str.Append(strAdditionalInfo);
        str.Append("End of spellcast Info");
        return str.ToString();
    }
}
public class Buff //Remember to override the MakeCopy() method
{
    public int intTimeOfStart = 0;
    public string strName;
    public string strDescription;
    public int intDuration;
    public Hero source;

    public static Buff Hextech = new Buff { strName = "Hextech", intDuration = 9999 };
    public static Buff SpellBlade = new Buff { strName = "SpellBlade", intDuration = 9999 };
    public static Buff InCombat = new Buff { strName = "InCombat", intDuration = 500};
    public static Buff Spellbinder_Active = new Buff { strName = "Spellbinder_Active" };
    public static Buff SuddenImpact = new Buff { strName = "SuddenImpact", intDuration =500};
    public static Buff CheapShot = new Buff { strName = "CheapShot" };//6.12 + 1.88 * level
    public static Buff NullifyingOrb = new Buff { strName = "NullifyingOrb", intDuration = 9999 };

    public virtual Buff MakeCopy()
    {
        return new Buff()
        {
            intTimeOfStart = intTimeOfStart,
            strName = strName,
            strDescription = strDescription,
            intDuration = intDuration,
            source = source
        };
    }

    public override bool Equals(object obj)
    {
        var buff = obj as Buff;
        return buff != null &&
               strName == buff.strName;
    }

    public override int GetHashCode()
    {
        return 1581483051 + EqualityComparer<string>.Default.GetHashCode(strName);
    }
}

public class Shield : Buff
{
    public float fStrength;
    public bool isWhiteShield = true;
    public bool isDecaying = false;
    public float fDecayingConstant = 0f;

    public override Buff MakeCopy()
    {
        return new Shield()
        {
            intTimeOfStart = intTimeOfStart,
            strName = strName,
            strDescription = strDescription,
            intDuration = intDuration,
            source = source,
            fStrength =fStrength,
            isWhiteShield = isWhiteShield,
            isDecaying = isDecaying,
            fDecayingConstant = fDecayingConstant
        };
    }
}

public class Debuff : Buff
{
    public static Debuff ElectrocuteCD = new Debuff { strName = "ElectrocuteCD" };
    public static Debuff ArcaneCometCD = new Debuff { strName = "ArcaneCometCD" };
    public static Debuff HextechCD = new Debuff { strName = "HextechCD", intDuration = 400, strDescription = "Hextech Item CD"};
    public static Debuff SpellBladeCD = new Debuff { strName = "SpellBladeCD", intDuration = 150, strDescription = "SpellBladeCD" };
    public static Debuff CoupDeGrace = new Debuff { strName = "CoupDeGrace" };
    public static Debuff ScorchCD = new Debuff { strName = "ScorchCD", intDuration = 100 };
    public static Debuff CheapShotCD = new Debuff { strName = "CheapShotCD", intDuration = 400 };
    public static Debuff SuddentImpactCD = new Debuff { strName = "SuddentImpactCD", intDuration = 400 };
    public static Debuff NullifyingOrbCD = new Debuff { strName = "NullifyingOrbCD", intDuration = 6000};
    public static Debuff Stun = new Debuff { strName = "Stun"};
    public static Debuff Icy = new Debuff { strName = "Icy", intDuration = 100 };
    public static Debuff CheapShotDebuff = new Debuff { strName = "CheapShotDebuff", intDuration = 9999 };

    public override Buff MakeCopy()
    {
        return new Debuff()
        {
            intTimeOfStart = intTimeOfStart,
            intDuration = intDuration,
            strName = strName,
            strDescription = strDescription,
            source = source
        };
    }
}
public class DoT : Debuff
{
    public bool isDamage = true;
    public int intInterval;
    public int intTickNumber;
    public float fDmgPerTick;
    public string strDmgType;

    public static DoT Torment = new DoT { strName = "Torment", intDuration = 300, intInterval = 50, isDamage = true, strDmgType = "AP" };
    public new string ToString()
    {
        StringBuilder str = new StringBuilder();
        str.Append("\nDoT: ");
        str.Append(strName);
        str.Append("\nTime of start");
        str.Append(intTimeOfStart.ToString());
        str.Append("\nRemaining duration");
        str.Append((intDuration).ToString());
        str.Append("\nInterval");
        str.Append(intInterval.ToString());
        str.Append("\n---");
        return str.ToString();
    }

    public override Buff MakeCopy()
    {
        return new DoT()
        {
            intTimeOfStart = intTimeOfStart,
            intDuration = intDuration,
            strName = strName,
            strDescription = strDescription,
            isDamage = isDamage,
            intInterval = intInterval,
            intTickNumber = intTickNumber,
            fDmgPerTick = fDmgPerTick,
            strDmgType = strDmgType,
            source = source
        };
    }
}
public class Amplifier
{
    public float fMRpenetration = 0f;
    public float fMRpercentagePenetration = 0f;
    public float fMRreduction = 0f;
    public List<string> otherAmplifers = new List<string>();
    public List<float> fPercentageDmgModifiers = new List<float>();
    public float fAMPenetration = 0f;

    public Amplifier MakeCopy()
    {
        return new Amplifier()
        {
            fMRpenetration = this.fMRpenetration,
            fMRpercentagePenetration = this.fMRpercentagePenetration,
            fMRreduction = this.fMRreduction,
            otherAmplifers = this.otherAmplifers,
            fPercentageDmgModifiers = this.fPercentageDmgModifiers
        };
    }
}
