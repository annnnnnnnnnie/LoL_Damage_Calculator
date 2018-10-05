using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.IO;
using System;

public class HeroInfo : MonoBehaviour {

    /*
     * prefab for selection of hero
     * prefab for heroLevel{display, levelup leveldown btn}
     * prefab for spellLevels{display, levelup leveldown btn}
     * prefab for displaying info such as MR AD AP
     * prefab for selection of runes;
     * prefab for selection of items;
     */
    public string heroName;

    public GameObject inventoryPrefab;
    public Inventory inventory;
    public GameObject spellPanelPrefab;
    public SpellPanel spellPanel;
    public GameObject runePagePrefab;
    public RunePage runePage;
    public Text heroNameLabel;

    public Transform levelComponentHolder;

    public GameObject openRunePageBtnPrefab;
    public GameObject levelCompornentPrefab;

    public bool isShowingHeroLevelOnly;
    
    public List<int> initialLevels = new List<int> { 1, 0, 0, 0, 0 };

    private RuneInfo runeInfo = null;
    private LevelComponent[] levelComponentsScripts = new LevelComponent[5];
    private List<string> levelNames = new List<string> { "Level", "Q", "W", "E", "R" };
    private readonly List<int> leastLevels = new List<int> { 1, 0, 0, 0, 0 };
    private readonly List<int> mostLevels = new List<int> { 18, 5, 5, 5, 3 };

    public void OpenRunePage()
    {
        runePage.gameObject.SetActive(true);
        runePage.Initialize(this);
    }

    public RuneInfo LoadRuneInfo(string heroName)
    {
        RuneInfo runeInfo = new RuneInfo();

        string priP = "";
        string secP = "";
        string keyStone = "";
        List<string> runeStones = new List<string>();

        string path_pri;
        string path_sec;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        path_pri = Application.streamingAssetsPath +"/SavedRunePages/" + heroName + "/PrimaryPath0.json";
        if (File.Exists(path_pri))
        {
            string dataAsJson;
            dataAsJson = File.ReadAllText(path_pri);
            RunePathData runePathData = new RunePathData();
            runePathData = JsonUtility.FromJson<RunePathData>(dataAsJson);
            Debug.Log("Found Saved RunePage at " + path_pri);
            GameDebugUtility.AddDebugMsg("Found Saved RunePage at " + path_pri);
            priP = runePathData.pathName;
            keyStone = runePathData.keyStone.stoneChoiced;
            foreach (RuneStone rs in runePathData.runeStones)
            {
                runeStones.Add(rs.stoneChoiced);
            }
        }
        else
        {
            Debug.LogError("No saved RunePage");
        }
        path_sec = Application.streamingAssetsPath + "/SavedRunePages/" + heroName + "/SecondaryPath0.json";
        if (File.Exists(path_sec))
        {
            string dataAsJson;
            dataAsJson = File.ReadAllText(path_sec);
            RunePathData runePathData = new RunePathData();
            runePathData = JsonUtility.FromJson<RunePathData>(dataAsJson);
            Debug.Log("Found Saved RunePage at " + path_sec);
            GameDebugUtility.AddDebugMsg("Found Saved RunePage at " + path_sec);
            secP = runePathData.pathName;
            foreach (RuneStone rs in runePathData.runeStones)
            {
                runeStones.Add(rs.stoneChoiced);
            }
        }
        else
        {
            Debug.LogError("No saved RunePage");
        }
#endif

#if UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android)
        {
            string dataAsJson;
            path_pri = Path.Combine(Application.streamingAssetsPath + "/", heroName + "/PrimaryPath0.json");
            WWW reader = new WWW(path_pri);
            while (!reader.isDone) { }
            dataAsJson = reader.text;
            RunePathData runePathData = new RunePathData();
            runePathData = JsonUtility.FromJson<RunePathData>(dataAsJson);
            Debug.Log("Found Saved RunePage at " + path_pri);

            priP = runePathData.pathName;
            keyStone = runePathData.keyStone.stoneChoiced;
            foreach (RuneStone rs in runePathData.runeStones)
            {
                runeStones.Add(rs.stoneChoiced);
            }


            path_sec = Path.Combine(Application.streamingAssetsPath + "/", heroName + "/SecondaryPath0.json");
            reader = new WWW(path_sec);
            while (!reader.isDone) { }
            dataAsJson = reader.text;
            runePathData = new RunePathData();
            runePathData = JsonUtility.FromJson<RunePathData>(dataAsJson);
            Debug.Log("Found Saved RunePage at " + path_sec);

            secP = runePathData.pathName;
            foreach (RuneStone rs in runePathData.runeStones)
            {
                runeStones.Add(rs.stoneChoiced);
            }
        }
#endif
        runeInfo.Initialize(priP, secP);
        runeInfo.AddRuneStone(keyStone);
        foreach(string s in runeStones)
        {
            runeInfo.AddRuneStone(s);
        }

        return runeInfo;
    } 
    
    public void Initialize(bool isShowingHeroLevelOnly, Processor processor, Hero owner)
    {
        this.isShowingHeroLevelOnly = isShowingHeroLevelOnly;
        heroName = owner.heroName;
        GenerateSpellButtons(isShowingHeroLevelOnly);//Generate buttons on the attached panel
        SetRuneInfo(LoadRuneInfo(heroName));
        Button btn = GameObjectUtility.CustomInstantiate(openRunePageBtnPrefab, transform).GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => { OpenRunePage(); });
        inventory = GameObjectUtility.CustomInstantiate(inventoryPrefab, this.transform).GetComponent<Inventory>();
        inventory.Initialize(processor, owner);
        spellPanel = GameObjectUtility.CustomInstantiate(spellPanelPrefab, this.transform).GetComponent<SpellPanel>();
        runePage = GameObjectUtility.CustomInstantiate(runePagePrefab, GameObject.Find("HomeScreen").transform).GetComponent<RunePage>();
        runePage.userName = heroName;
        heroNameLabel.text = heroName;
    }

    public void GenerateSpellButtons(bool isShowingHeroLevelOnly)
    {
        for (int i = 0; i < 5; i++)
        {
            if (i > 0 && isShowingHeroLevelOnly) return;

            GameObject gO = GameObjectUtility.CustomInstantiate(levelCompornentPrefab, levelComponentHolder);
            levelComponentsScripts[i] = gO.GetComponent<LevelComponent>();
            levelComponentsScripts[i].Initialize(levelNames[i], initialLevels[i], leastLevels[i], mostLevels[i]);
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
        return levelComponentsScripts[levelNames.IndexOf(levelName)].GetLevel();
    }
    public Dictionary<string, float> CalculateAttributes()
    {
        Dictionary<string, int> tempDicExtra = CalculateItemExtras();
        Dictionary<string, float> tempDicAttri = GameStatsUtility.CombineAttributes(CalculateBaseAttributes(heroName), CalculateItemAttributes());
        return GameStatsUtility.CalculateEffectiveAttributes(tempDicAttri, tempDicExtra);
    }

    public Dictionary<string,float> CalculateBaseAttributes(string heroName)
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

    private Dictionary<string, float> CalculateItemAttributes()
    {
        List<Item> items = inventory.GetItems();
        Dictionary<string, float> Attributes = new Dictionary<string, float>();
        StringBuilder debugMsg = new StringBuilder();
        foreach (Item item in items)
        {
            if (item != null && item.strName != null)
            {
                debugMsg.Append("Calculating item attribute: " + item.strName + "\n");
                foreach (KeyValuePair<string, float> attribute in item.Attributes)
                {
                    if (Attributes.ContainsKey(attribute.Key))
                    {
                        Attributes[attribute.Key] += attribute.Value;
                    }
                    else
                    {
                        Attributes.Add(attribute.Key, attribute.Value);
                    }
                    debugMsg.Append("Current " + attribute.Key + " is " + Attributes[attribute.Key] + "\n");
                }
            }
            else
            {
                debugMsg.Append("Current itemSlot is empty\n");
            }
        }
        Debug.Log(debugMsg.ToString());
        return Attributes;
    }

    private Dictionary<string, int> CalculateItemExtras()
    {
        List<Item> items = inventory.GetItems();
        Dictionary<string, int> Extras = new Dictionary<string, int>();
        StringBuilder debugMsg = new StringBuilder();
        foreach (Item item in items)
        {
            if (item != null && item.strName != null)
            {
                debugMsg.Append("Calculating item extra: " + item.strName + "\n");
                foreach (KeyValuePair<string, int> extra in item.Extras)
                {
                    if (Extras.ContainsKey(extra.Key))
                    {
                        Extras[extra.Key] = Mathf.Max(Extras[extra.Key], extra.Value);//Only keep the highest one as they Unique
                    }
                    else
                    {
                        Extras.Add(extra.Key, extra.Value);
                    }
                    debugMsg.Append("Current " + extra.Key + " is " + Extras[extra.Key] + "\n");
                }
            }
            else
            {
                debugMsg.Append("Current itemSlot is empty\n");
            }
        }
        Debug.Log(debugMsg.ToString());
        return Extras;
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
        dicHeroStat.Add("level", levelComponentsScripts[0].GetLevel());
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
        dicHeroStat.Add("fBaseArmor", heroStats.fBaseArmor);
        dicHeroStat.Add("fArmorGrowth", heroStats.fArmorGrowth);
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

    public void Update()
    {
        if (Input.GetMouseButtonDown(1))
            DisplayRuneInfo();//for debug use
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
    public float fBaseArmor;
    public float fArmorGrowth;
    public float fBaseMS;
    public float fMSGrowth;

}




