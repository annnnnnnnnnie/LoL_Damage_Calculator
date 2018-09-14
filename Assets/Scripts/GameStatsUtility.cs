using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameStatsUtility
{
    public static float CalculateStats(float fBase, float fGrowth, int intLevel)
    {
        return (float)(fBase + fGrowth * (intLevel - 1) * (0.7025 + (0.0175 * (intLevel - 1))));
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

    public static Dictionary<string, float> CalculateEffectiveAttributes(Dictionary<string, float> d0)
    {
        Dictionary<string, float> d1 = new Dictionary<string, float>();
        //---AP------------------------------------------
        //Item AP
        float iap=0f;
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
        if(d0.TryGetValue("ScalingHealthBase", out rhpBase))
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
            Debug.Log("No HP mana BaseAttributes");
        }

        float rmn = 0f;//from runes
        float totalMana = imn + bmn + rmn;
        //---mana--------------------------------------------------------

        //Item passives----------------------------------------------------
        if (d0.ContainsKey("Unique_Passive_RabadonsDeathcap"))
        {
            Debug.Log("Unique_Passive_RabadonsDeathcap detected");
            totalAP = totalAP * 1.4f;
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

        d1.Add("AP", totalAP);
        d1.Add("AD", bad + iad + aad);
        d1.Add("MR", bmr + imr);
        d1.Add("HP", bhp + ihp + rhp);
        d1.Add("Mana", totalMana);
        d1.Add("APPenetration", apPene);
        d1.Add("APPPenetration", apPercentPene);
        d1.Add("CurrentHealth", bhp + ihp);
        d1.Add("HealthRegen", ihr + bhr + rhr);
        GameDebugUtility.Debug_ShowDictionary("d0", d0);
        GameDebugUtility.Debug_ShowDictionary("d1", d1);
        return d1;
    }


}

public class Counter
{
    public int intElectrocuteCount;

    public void Reset()
    {
        intElectrocuteCount = 0;
    }
}

