using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class Processor : MonoBehaviour//Attached to HomeScreen
{
    public ItemList itemList;
    public SpellPanel spellPanel;
    public AdvancedSetting advancedSetting;
    public Text logText;

    public int totalTime;//Set in inspector
    private int intTime;
    private ItemSlot currentItemSlot;
    private Annie_Test annie;
    private Annie_Test enemy;
    private List<string> strSpellList;

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
        annie.Initialize("Annie");
        enemy = new Annie_Test();
        enemy.Initialize("Enemy");
    }

    public void Update()
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
        currentItemSlot.EquipItem(item.MakeCopy());//deep copying

        List<string> possibleActives = new List<string>() {
            "CorruptingPotion", "SkirmishersSabre", "GargoyleStonePlate",
            "HextechGLP_800", "HextechGunblade","HextechProtobelt_01","ShurelyasReverie","Spellbinder" };

        foreach (string active in possibleActives)
        {
            if (item.strName == active)
            {
                annie.LearnNewSpell(active);
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
            annie.UnlearnSpell(unequippedItem.strName);
        }
        itemList.CloseItemList();

    }

    public void Calculate()
    {
        intTime = 0;
        annie.PrepareToCastSpells();
        enemy.PrepareToReceiveSpells();
        strSpellList = spellPanel.SubmitSpells();
        Debug.Log("----------------Calculating... ----------------");

        Dictionary<int, string> spellCastSequence = new Dictionary<int, string>();
        for (int i = 0; i < 100; i++)
        {
            if (i < strSpellList.Count)
            {
                spellCastSequence.Add((intTime + i * 0), strSpellList[i]);
            }
            intTime += 10;
        }

        Calculate(spellCastSequence);
        return;
    }

    public void Calculate(Dictionary<int, string> spellCastsSequence)
    {
        Debug.Log("Calculating using advanced technologies... ");
        GameDebugUtility.AddDebugMsg("--------------Calculating using advanced technologies...--------- ");
        intTime = 0;
        for (int i = 0; i < totalTime*10; i++)
        {
            if (spellCastsSequence.ContainsKey(intTime))
            {
                List<SpellCast> spellCasts=  new List<SpellCast>();
                SpellCast spellCast = annie.CastSpell(spellCastsSequence[intTime]);
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
    public void ClearLogText()
    {
        GameDebugUtility.Initialize();
        logText.text = "";
    }
}
public class SpellCast
{
    public double dDamage;
    public Amplifier amplifier;
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
public class Buff
{
    public int intTimeOfStart = 0;
    public string strName;
    public string strDescription;
    public int intDuration;

    public static Buff Hextech = new Buff { strName = "Hextech", intDuration = 9999 };
    public static Buff SpellBlade = new Buff { strName = "SpellBlade", intDuration = 9999 };
    public static Buff InCombat = new Buff { strName = "InCombat", intDuration = 500};
    public virtual Buff MakeCopy()
    {
        return new Buff()
        {
            intTimeOfStart = intTimeOfStart,
            strName = strName,
            strDescription = strDescription,
            intDuration = intDuration
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
public class Debuff : Buff
{
    public static Debuff ElectrocuteCD = new Debuff { strName = "ElectrocuteCD" };
    public static Debuff ArcaneCometCD = new Debuff { strName = "ArcaneCometCD" };
    public static Debuff HextechCD = new Debuff { strName = "HextechCD", intDuration = 400, strDescription = "Hextech Item CD"};
    public static Debuff SpellBladeCD = new Debuff { strName = "SpellBladeCD", intDuration = 150, strDescription = "SpellBladeCD" };
    public static Debuff CoupDeGrace = new Debuff { strName = "CoupDeGrace" };
    public static Debuff ScorchCD = new Debuff { strName = "ScorchCD", intDuration = 100 };
    public static Debuff Icy = new Debuff { strName = "Icy", intDuration = 100 };
    public static Debuff Stun = new Debuff { strName = "Icy"};


    public override Buff MakeCopy()
    {
        return new Debuff()
        {
            intTimeOfStart = intTimeOfStart,
            intDuration = intDuration,
            strName = strName,
            strDescription = strDescription
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
    public Amplifier amplifier;

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
            intInterval=intInterval,
            intTickNumber = intTickNumber,
            fDmgPerTick=fDmgPerTick,
            strDmgType=strDmgType,
            amplifier=amplifier
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
