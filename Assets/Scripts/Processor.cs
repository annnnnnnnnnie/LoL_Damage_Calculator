using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class Processor : MonoBehaviour//Attached to HomeScreen
{
    public ItemList itemList;
    public SpellPanel spellPanel;

    public int totalTime = 10;
    private int intTime;
    private ItemSlot currentItemSlot;
    private Annie_Test annie;
    private Annie_Test enemy;
    private List<string> strSpellList;

    private List<DoT> dotDamages;

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
        }
    }

    public void OpenItemList(ItemSlot itemSlot)
    {
        itemList.gameObject.SetActive(true);
        itemList.OpenItemList();
        currentItemSlot = itemSlot;
    }

    public void EquipItem(Item item)
    {
        Debug.Log("Equipping item: item in itemSlot " + currentItemSlot.itemSlotNumber);
        currentItemSlot.EquipItem(item.MakeCopy());//Beta: deep copying

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

    public void Calculate()//Intake a List of Skillcast 
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
                spellCastSequence.Add((intTime + i*0), strSpellList[i]);
            }
            intTime += 10;
        }

        Calculate(spellCastSequence);
        return;
    }

    public void Calculate(Dictionary<int, string> spellCastsSequence)
    {
        Debug.Log("Calculating using advanced technologies... ");
        intTime = 0;
        for (int i = 0; i < totalTime*100; i++)
        {
            if (spellCastsSequence.ContainsKey(intTime))
            {
                SpellCast spellCast = annie.CastSpell(spellCastsSequence[intTime]);
                enemy.Update(spellCast);
                foreach (string addInfo in spellCast.strAdditionalInfo)
                {
                    if (addInfo.Equals("Electrocute"))
                    {
                        spellCast = annie.CastSpell("Electrocute");
                        enemy.Update(spellCast);
                    }
                    if (addInfo.Equals("ArcaneComet"))
                    {
                        spellCast = annie.CastSpell("ArcaneComet");
                        enemy.Update(spellCast);
                    }
                    

                }

            }
            else
            {
                enemy.Update();
            }

            annie.Update();
            
            intTime += 10;
        }
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

    public string ToString()
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
    public static Debuff CoupDeGrace = new Debuff { strName = "CoupDeGrace" };
}
public class DoT : Debuff
{
    public bool isDamage = true;
    public int intInterval;
    public int intTickNumber;
    public float fDmgPerTick;
    public string strDmgType;
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
}
public class Amplifier
{
    public float fMRpenetration = 0f;
    public float fMRpercentagePenetration = 0f;
    public float fMRreduction = 0f;
    public List<float> fPercentageDmgModifiers = new List<float>();
}
