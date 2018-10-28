using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameStatsUtility
{
    public static float CalculateStats(float fBase, float fGrowth, int intLevel)
    {
        return (float)(fBase + fGrowth * (intLevel - 1) * (0.7025 + (0.0175 * (intLevel - 1))));
    }

    public static float LethalityToAMPenetration(float fLethality, int intLevel)
    {
        return (float)(fLethality * (0.6 + 0.4 * intLevel / 18));
    }

    public static Dictionary<string, float> CombineAttributes(Dictionary<string, float> d1, Dictionary<string, float> d2)
    {
        foreach (KeyValuePair<string, float> kvPair in d2)
        {
            if (d1.ContainsKey(kvPair.Key))
            {
                d1[kvPair.Key] = d1[kvPair.Key] + kvPair.Value;

            }
            else
            {
                d1[kvPair.Key] = kvPair.Value;
            }
        }
        GameDebugUtility.Debug_ShowDictionary("Combined dic:\n",d1);
        return d1;
    }

    public static Dictionary<string, float> CalculateEffectiveAttributes(Dictionary<string, float> d0, Dictionary<string, int> dExtras)
    {
        Dictionary<string, float> d1 = new Dictionary<string, float>();
        float ip = 0f;
        if (d0.ContainsKey("price"))
        {
            ip = d0["price"];
            Debug.Log("Total price: " + ip);
        }
        else
        {
            Debug.Log("Total price is 0");
        }
        //---AP------------------------------------------
        //Item AP
        float iap = 0f;
        if (d0.TryGetValue("AP", out iap))
        {
            Debug.Log("Item AP: " + iap);
        }
        else
        {
            Debug.Log("No AP From items");
        }

        

        //Base AP
        float bap = 0f;
        //---AP-----------------------------------------------------


        //---AD------------------------------------------------------
        //Item AD
        float iad = 0f;
        if (d0.TryGetValue("AD", out iad))
        {
            Debug.Log("Item AD: " + iad);
        }
        else
        {
            Debug.Log("No AD From items");
        }
        //Base AD
        float bad = 0f;
        float baseAD;
        float ADGrowth;
        if (d0.TryGetValue("fBaseAD", out baseAD))
        {
            if (d0.TryGetValue("fADGrowth", out ADGrowth))
            {
                bad = GameStatsUtility.CalculateStats(baseAD, ADGrowth, (int)d0["level"]);
                Debug.Log("Base AD: " + (bad + iad).ToString());
            }
        }
        else
        {
            Debug.Log("No AD From BaseAttributes");
        }
        //---AD-------------------------------------------------------


       

        //---MR-------------------------------------------------------
        float imr = 0f;
        if (d0.TryGetValue("MR", out imr))
        {
            Debug.Log("Item MR: " + imr);
        }
        else
        {
            Debug.Log("No MR From items");
        }

        float bmr = 0f;
        float baseMR;
        float MRGrowth;
        if (d0.TryGetValue("fBaseMR", out baseMR))
        {
            if (d0.TryGetValue("fMRGrowth", out MRGrowth))
            {
                bmr = GameStatsUtility.CalculateStats(baseMR, MRGrowth, (int)d0["level"]);
                Debug.Log("Base MR: " + bmr);
            }
        }
        else
        {
            Debug.Log("No MR From BaseAttributes");
        }
        Debug.Log("Total MR: " + (bmr + imr).ToString());
        //---MR-------------------------------------------------------
        //---Armor-----------------------------------------------------
        float iam = 0f;
        if (d0.TryGetValue("Armor", out iam))
        {
            Debug.Log("Item Armor: " + iam);
        }
        else
        {
            Debug.Log("No Armor From items");
        }

        float bam = 0f;
        float baseArmor;
        float ArmorGrowth;
        if (d0.TryGetValue("fBaseArmor", out baseArmor))
        {
            if (d0.TryGetValue("fArmorGrowth", out ArmorGrowth))
            {
                bam = GameStatsUtility.CalculateStats(baseArmor, ArmorGrowth, (int)d0["level"]);
                Debug.Log("Base Armor: " + bam);
            }
        }
        else
        {
            Debug.Log("No Armor From BaseAttributes");
        }
        float ram = 0f;
        //---Armor-----------------------------------------------------
        //---Health-------------------------------------------------------
        float ihp = 0f;
        if (d0.TryGetValue("health", out ihp))
        {
            Debug.Log("Item health: " + ihp);
        }
        else
        {
            Debug.Log("No health From items");
        }

        float bhp = 0f;
        float baseHP;
        float HPGrowth;
        if (d0.TryGetValue("fBaseHP", out baseHP))
        {
            if (d0.TryGetValue("fHPGrowth", out HPGrowth))
            {
                bhp = GameStatsUtility.CalculateStats(baseHP, HPGrowth, (int)d0["level"]);
                Debug.Log("Base HP: " + bhp);
            }
        }
        else
        {
            Debug.Log("No HP From BaseAttributes");
        }

        float rhp = 0f;
        float rhpBase;
        float rhpIncrement;
        if (d0.TryGetValue("ScalingHealthBase", out rhpBase))
        {
            rhpIncrement = d0["ScalingHealthIncrement"];
            rhp = rhpBase + rhpIncrement * d0["level"];
            Debug.Log("Rune HP: " + rhp);
        }
        else
        {
            Debug.Log("No health from rune");
        }
        Debug.Log("Total HP: " + (bhp + ihp + rhp).ToString());
        //---Health-------------------------------------------------------
        //---HealthRegen---------------------------------------------------
        float ihr = 0f;
        if (d0.TryGetValue("healthRegen", out ihr))
        {
            Debug.Log("Item healthRegen: " + ihr);
        }
        else
        {
            Debug.Log("No healthRegen From items");
        }
        float bhr = 0f;
        float bhrBase;
        float bhrIncrement;
        if (d0.TryGetValue("fBaseHPRegen", out bhrBase))
        {
            if (d0.TryGetValue("fHPRegenGrowth", out bhrIncrement))
            {
                bhr = GameStatsUtility.CalculateStats(bhrBase, bhrIncrement, (int)d0["level"]);
                Debug.Log("Base health regen: " + bhr.ToString());
            }
        }
        else
        {
            Debug.Log("No HPRegen From BaseAttributes");
        }

        float rhr = 0f;
        //---HealthRegen---------------------------------------------------
        //---mana--------------------------------------------------------
        float imn = 0f;
        if (d0.TryGetValue("mana", out imn))
        {
            Debug.Log("Item mana: " + imn);
        }
        else
        {
            Debug.Log("No mana From items");
        }
        float bmn = 0f;
        float baseMana;
        float ManaGrowth;
        if (d0.TryGetValue("fBaseMana", out baseMana))
        {
            if (d0.TryGetValue("fManaGrowth", out ManaGrowth))
            {
                bmn = GameStatsUtility.CalculateStats(baseMana, ManaGrowth, (int)d0["level"]);
                Debug.Log("Base Mana: " + bmn);
            }
        }
        else
        {
            Debug.Log("No mana BaseAttributes");
        }

        float rmn = 0f;//from runes

        //---mana--------------------------------------------------------
        //---CDR---------------------------------------------------------
        int icd = 0;
        if (d0.ContainsKey("Unique_Passive_Haste"))
        {
            Debug.Log("Unique_Passive_Haste Detected");
            icd += 10;
        }
        if (d0.ContainsKey("Unique_CDR_FiendishCodex"))
        {
            Debug.Log("Unique_CDR_FiendishCodex Detected");
            icd += 10;
        }
        if (d0.ContainsKey("Unique_CDR_Frostfang"))
        {
            Debug.Log("Unique_CDR_Frostfang Detected");
            icd += 10;
        }
        if (d0.ContainsKey("Unique_CDR_EyeOfFrost"))
        {
            Debug.Log("Unique_CDR_EyeOfFrost Detected");
            icd += 10;
        }
        if (d0.ContainsKey("Unique_CDR_LostChapter"))
        {
            Debug.Log("Unique_CDR_LostChapter Detected");
            icd += 10;
        }
        if (d0.ContainsKey("CDR"))
        {
            Debug.Log("CDR items Detected");
            icd += (int)d0["CDR"];
        }
        Debug.Log("Item CDR: " + icd + "%");
        
        //---CDR---------------------------------------------------------
        //Item passives----------------------------------------------------
        int doOrDiePassive = 0;
        int i0 = 0;
        if (d0.ContainsKey("Unique_Passive_Dread_MejaisSoulstealer"))
        {
            doOrDiePassive = dExtras["Unique_Passive_DoOrDie_MejaisSoulstealer"] * 5;
            Debug.Log("MejaisSoulstealer AP: " + doOrDiePassive);
        }
        else if (d0.ContainsKey("Unique_Passive_Dread_TheDarkSeal"))
        {
            doOrDiePassive = dExtras["Unique_Passive_DoOrDie_TheDarkSeal"] * 3;
            Debug.Log("The Dark Seal AP: " + doOrDiePassive);
        }
        else
        {
            Debug.Log("No Do Or Die Items");
        }
        iap += doOrDiePassive;

        int rodOfAgesPassiveAP = 0;
        int rodOfAgesPassiveHP = 0;
        int rodOfAgesPassiveMana = 0;
        if (dExtras.TryGetValue("Unique_Passive_RodOfAges", out i0))
        {
            rodOfAgesPassiveAP = i0 * 4;
            rodOfAgesPassiveHP = i0 * 20;
            rodOfAgesPassiveMana = i0 * 10;
            Debug.Log("Rod Of Ages AP: " + rodOfAgesPassiveAP);
            Debug.Log("Rod Of Ages HP: " + rodOfAgesPassiveHP);
            Debug.Log("Rod Of Ages Mana: " + rodOfAgesPassiveMana);
        }
        else
        {
            Debug.Log("No Rod Of Ages");
        }
        iap += rodOfAgesPassiveAP;
        ihp += rodOfAgesPassiveHP;
        imn += rodOfAgesPassiveMana;

        int aStaffMana = 0;
        float aStaffAP = 0;
        if (dExtras.TryGetValue("Unique_Passive_ManaCharge", out i0))
        {
            aStaffMana = i0;
            Debug.Log("aStaff Mana: " + aStaffMana);
        }
        else
        {
            Debug.Log("No aStaff Mana");
        }
        imn += aStaffMana;
        float totalMana = imn + bmn + rmn;

        if (d0.ContainsKey("Unique_Passive_AweAP"))
        {
            aStaffAP = (float)0.01 * totalMana;
            Debug.Log("aStaffAP: " + aStaffAP);
        }
        iap += aStaffAP;

        float seekersArmguardAP = 0f;
        float seekersArmguardArmor = 0f;
        if (dExtras.TryGetValue("Unique_Passive_SeekersArmguard", out i0))
        {
            seekersArmguardAP = (float)0.5 * i0;
            seekersArmguardArmor = (float)0.5 * i0;
            Debug.Log("seekersArmguard AP : " + seekersArmguardAP);
            Debug.Log("seekersArmguard Armor : " + seekersArmguardArmor);
        }
        else
        {
            Debug.Log("No SeekersArmguard");
        }
        iap += seekersArmguardAP;
        imr += seekersArmguardArmor;

        if (d0.ContainsKey("Unique_Passive_RabadonsDeathcap"))//Assume that only item ap are increased
        {
            Debug.Log("Unique_Passive_RabadonsDeathcap detected");
            iap = iap * 1.4f;
        }


        if (d0.ContainsKey("Unique_Passive_Echo"))
        {
            Debug.Log("Unique_Passive_Echo detected");
            d1.Add("Unique_Passive_Echo", 0);
        }
        if (d0.ContainsKey("Unique_Passive_MagicBolt"))
        {
            Debug.Log("Unique_Passive_MagicBolt detected");
            d1.Add("Unique_Passive_MagicBolt", 0);
        }
        if (d0.ContainsKey("Unique_Active_FireBolt"))
        {
            Debug.Log("Unique_Active_FireBolt detected: " + dExtras["Unique_Active_FireBolt"]);
            d1.Add("Unique_Active_FireBolt", dExtras["Unique_Active_FireBolt"]);
        }
        if (d0.ContainsKey("Unique_Active_Spellbinder"))
        {
            Debug.Log("Unique_Active_Spellbinder detected: " + dExtras["Unique_Active_Spellbinder"]);
            d1.Add("Unique_Active_Spellbinder", dExtras["Unique_Active_Spellbinder"]);
        }
        if(d0.ContainsKey("Unique_Passive_TouchOfCorruption"))
        {
            Debug.Log("CorruptingPotion Detected");
            d1.Add("Unique_Passive_TouchOfCorruption", 0);
        }
        if (d0.ContainsKey("Unique_Passive_SpellBlade"))
        {
            Debug.Log("Unique_Passive_SpellBlade Detected");
            d1.Add("Unique_Passive_SpellBlade", 0);
        }
        if (d0.ContainsKey("Unique_Passive_Icy"))
        {
            Debug.Log("Unique_Passive_Icy Detected");
            d1.Add("Unique_Passive_Icy", 0);
        }
        if (d0.ContainsKey("Unique_Passive_Torment"))
        {
            Debug.Log("Unique_Passive_Torment Detected");
            d1.Add("Unique_Passive_Torment", 0);
        }
        if (d0.ContainsKey("Unique_Passive_Madness"))
        {
            Debug.Log("Unique_Passive_Madness Detected");
            d1.Add("Unique_Passive_Madness", 0);
        }


        //AP penetration from item
        float apPene = 0f;
        if (d0.ContainsKey("Unique_Passive_TouchOfDeath"))
        {
            Debug.Log("Unique_Passive_TouchOfDeath detected");
            apPene += 15f;
        }
        if (d0.ContainsKey("Unique_Passive_SorcerersShoes"))
        {
            Debug.Log("Unique_Passive_SorcerersShoes detected");
            apPene += 18f;
        }

        float apPercentPene = 0f;
        if (d0.ContainsKey("Unique_Passive_VoidStaff"))
        {
            Debug.Log("Unique_Passive_VoidStaff detected");
            apPercentPene = 0.4f;
        }

        //Adaptive
        float aap = 0f;
        float aad = 0f;
        if (iap >= iad)
        {
            if (d0.TryGetValue("AdaptiveAP", out aap))
            {
                Debug.Log("Adaptive AP: " + aap);
            }
            else
            {
                Debug.Log("No AdaptiveAP From runes");
            }
        }
        else
        {
            if (d0.TryGetValue("AdaptiveAD", out aad))
            {
                Debug.Log("Adaptive AD: " + aad);
            }
            else
            {
                Debug.Log("No AdaptiveAD From runes");
            }
        }

        float totalAP = iap + bap + aap;
        
        d1.Add("AP", totalAP);
        d1.Add("AD", bad + iad + aad);
        d1.Add("BAD", bad);
        d1.Add("IAD", iad);
        d1.Add("IAP", iap);
        d1.Add("ICD", icd);
        d1.Add("CDR", 0);
        d1.Add("MR", bmr + imr);
        d1.Add("Armor", iam + bam + ram);
        d1.Add("HP", bhp + ihp + rhp);
        d1.Add("MaxHP", bhp + ihp + rhp); 
        d1.Add("Mana", totalMana);
        d1.Add("APPenetration", apPene);
        d1.Add("APPPenetration", apPercentPene);
        d1.Add("Lethality", 0f);
        d1.Add("CurrentHealth", bhp + ihp);
        d1.Add("HealthRegen", ihr + bhr + rhr);
        d1.Add("price", ip);
        GameDebugUtility.Debug_ShowDictionary("d0", d0);
        GameDebugUtility.Debug_ShowDictionary("d1", d1);
        return d1;
    }


}

public class Counter
{
    public int intElectrocuteCount;
    public int EchoCount;
    public int MadnessCount;

    public void Reset()
    {
        intElectrocuteCount = 0;
        EchoCount = 0;
        MadnessCount = 0;
    }
}

