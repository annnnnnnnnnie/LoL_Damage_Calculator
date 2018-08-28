using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Processor : MonoBehaviour//Attached to HomeScreen
{
    public ItemList itemList;
    public SpellPanel spellPanel;
    private float fTime;
    private ItemSlot currentItemSlot;
    private Annie annie;
    private Enemy enemy;
    private List<string> strSpellList;

    private List<DoT> dotDamages;

    public void Start()
    {
        annie = new Annie();
        annie.Initialize();
        enemy = new Enemy();
        enemy.Initialize();
        
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
        itemList.CloseItemList();
    }

    public void UnequipItem()
    {
        Debug.Log("Unequipping item: ");
        currentItemSlot.UnequipItem();
        itemList.CloseItemList();
    }

    public void Calculate()//Intake a List of Skillcast 
    {
        fTime = 0f;
        annie.PrepareToCastSpells();
        enemy.PrepareToReceiveSpells();
        strSpellList = spellPanel.SubmitSpells();

        Debug.Log("----------------Calculating... ----------------");
        for (int i = 0; i < 10; i++)
        {

        }
        if (strSpellList.Count > 0)
        {
            double totaldmg = 0;
            foreach (string spell in strSpellList)
            {
                Debug.Log("Next Spell: " + spell);
                SpellCast spellcast = annie.CastSpell(spell);
                //Debug.Log("Damage is " + skillcast.dDamage);
                totaldmg += enemy.ReceiveSpell(spellcast);
                if (spellcast.strAdditionalInfo.Equals("Electrocute"))
                {
                    spellcast = annie.CastSpell("Electrocute");
                    totaldmg += enemy.ReceiveSpell(spellcast);
                }

            }
            Debug.Log("Total damage dealt: " + totaldmg.ToString());
        }
        else
        {
            Debug.Log("No Spell Selected!");
        }
        
    }

    public void Calculate(List<SpellCast> SpellCasts)
    {
        Debug.Log("Calculating...Really... ");
    }

}
public class SpellCast
{
    public double dDamage;
    public Amplifier amplifier = new Amplifier();
    public string strAdditionalInfo = "";
    public float fCooldown = 0f;
    public Debuff debuff = new Debuff();
    public string strDmgType;
}
public class Buff
{
    public string strName;
    public string strDescription;
    public float fDuration;
}
public class Debuff : Buff
{
}
public class DoT : Debuff
{
    public bool isDamage;
    public float fInterval;
    public int intTickNumber;
    public float fDmgPerTick;
    public string strDmgType;
}
public class Amplifier
{
    public float fMRpenetration = 0f;
    public float fMRpercentagePenetration = 0f;
    public float fMRreduction = 0f;
    public List<float> fPercentageDmgModifiers = new List<float>();
}
