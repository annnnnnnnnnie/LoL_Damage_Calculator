using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HeroInfo : MonoBehaviour {

    /*
     * prefab for selection of hero
     * prefab for heroLevel{display, levelup leveldown btn}
     * prefab for spellLevels{display, levelup leveldown btn}
     * prefab for displaying info such as MR AD AP
     * prefab for selection of runes;
     * prefab for selection of items;
     */
    
    public bool isShowingHeroLevelOnly; 
    public GameObject levelComponent;
    public Toggle isRanged;
    public List<int> initialLevels = new List<int> { 1, 0, 0, 0, 0 };

    public RunePage runePage;
    private RuneInfo runeInfo = null;


    private Dictionary<string, float> Attributes = new Dictionary<string, float>();


    private LevelComponent[] levelComponentsScript = new LevelComponent[5]; 
    private List<string> levelNames = new List<string> { "Level", "Q", "W", "E", "R" };
    private readonly List<int> leastLevels = new List<int> { 1, 0, 0, 0, 0 };
    private readonly List<int> mostLevels = new List<int> { 18, 5, 5, 5, 5 };

    public void Start()
    {
        GenerateSpellButtons(isShowingHeroLevelOnly);
        
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(1))
            DisplayRuneInfo();//for debug use
    }

    public void GenerateSpellButtons(bool isShowingHeroLevelOnly)
    {
        for (int i = 0; i < 5; i++)
        {
            if (i > 0 && isShowingHeroLevelOnly) return;

            GameObject gO = GameObjectUtility.CustomInstantiate(levelComponent, transform);
            levelComponentsScript[i] = gO.GetComponent<LevelComponent>();
            levelComponentsScript[i].Initialize(levelNames[i], initialLevels[i], leastLevels[i], mostLevels[i]);
        }
    }
    public void SetRuneInfo(RuneInfo runeInfo)
    {
        this.runeInfo = runeInfo;
    }

    private void DisplayRuneInfo()
    {
        if(runeInfo == null)
        {
            Debug.LogError("no saved runeInfo");
            return;
        }
        Debug.Log(runeInfo.ToString());
    }

    public int GetLevel(string levelName)
    {
        return levelComponentsScript[levelNames.IndexOf(levelName)].GetLevel();
    }

    public bool GetEnemyType()
    {
        return isRanged.isOn;
    }
    
    public Dictionary<string,float> GetBaseAttributes(string heroName)
    {
        Dictionary<string, float> baseAttributes = new Dictionary<string, float>();
        foreach (KeyValuePair<string, float> kvPair in CalculateHeroStat(heroName))
        {
            baseAttributes[kvPair.Key] = GameStatsUtility.CalculateStats(kvPair.Value, 0f, 1);//TODO
        }
        foreach (KeyValuePair<string, float> kvPair in runeInfo.GetAttributes())
        {
            if (baseAttributes.ContainsKey(kvPair.Key))
            {
                baseAttributes[kvPair.Key] += kvPair.Value;
            }
            else
            {
                baseAttributes[kvPair.Key] = kvPair.Value;
            }
        }
        GameDebugUtility.Debug_ShowDictionary("Base attributes \n", baseAttributes);
        return baseAttributes;
    }

    public List<string> GetSelectedRunes()
    {
        List<string> strs = new List<string>();
        try
        {
            foreach (string rs in runeInfo.GetRuneStones())
            {
                if (rs.Length > 25) continue;
                strs.Add(rs);
            }
            return strs;

        }
        catch (System.NullReferenceException)
        {

            throw new System.Exception("No rune page saved");
        }
    }

    private Dictionary<string, float> CalculateHeroStat(string heroName)
    {
        Dictionary<string, float> dicHeroStat = new Dictionary<string, float>();
        HeroStats heroStats = LoadHeroStat(heroName);
        dicHeroStat.Add("level", levelComponentsScript[0].GetLevel());
        dicHeroStat.Add("fBaseHP", heroStats.fBaseHP);
        dicHeroStat.Add("fHPGrowth", heroStats.fHPGrowth);

        dicHeroStat.Add("fCurrentHealth", 0f);

        dicHeroStat.Add("fBaseAD", heroStats.fBaseAD);
        dicHeroStat.Add("fADGrowth", heroStats.fADGrowth);
        dicHeroStat.Add("fBaseHPRegen", heroStats.fBaseHPRegen);
        dicHeroStat.Add("fHPRegenGrowth", heroStats.fHPRegenGrowth);
        dicHeroStat.Add("fBaseAttackSpeed", heroStats.fBaseAttackSpeed);
        dicHeroStat.Add("fAttackSpeedGrowth", heroStats.fAttackSpeedGrowth);
        dicHeroStat.Add("fBaseMana", heroStats.fBaseMana);
        dicHeroStat.Add("fManaGrowth", heroStats.fManaGrowth);
        dicHeroStat.Add("fBaseMR", heroStats.fBaseMR);
        dicHeroStat.Add("fMRGrowth", heroStats.fMRGrowth);
        dicHeroStat.Add("fBaseMS", heroStats.fBaseMS);
        dicHeroStat.Add("fMSGrowth", heroStats.fMSGrowth);
        dicHeroStat.Add("fBaseAP", 0f);
        return dicHeroStat;
    }

    private HeroStats LoadHeroStat(string heroName)
    {
        TextAsset dataAsJson = Resources.Load<TextAsset>("HeroStatsJson");
        HeroStatsData heroStatsData = JsonUtility.FromJson<HeroStatsData>(dataAsJson.text);
        HeroStats[] heroStats = heroStatsData.heroStatsData;

        foreach (HeroStats currentStat in heroStats)
        {
            if (currentStat.strHeroName == heroName)
            {
                Debug.Log("Found heroStat for "+ heroName);
                return currentStat;
            }
        }
        throw new System.Exception("Did not find heroStats for " + heroName);
    }
}



[System.Serializable]
public class HeroStatsData
{
    public HeroStats[] heroStatsData;
}

[System.Serializable]
public class HeroStats
{
    public string strHeroName;
    public float fBaseHP;
    public float fHPGrowth;
    public float fBaseAD;
    public float fADGrowth;
    public float fBaseHPRegen;
    public float fHPRegenGrowth;
    public float fBaseAttackSpeed;
    public float fAttackSpeedGrowth;
    public float fBaseMana;
    public float fManaGrowth;
    public float fBaseMR;
    public float fMRGrowth;
    public float fBaseMS;
    public float fMSGrowth;

}




