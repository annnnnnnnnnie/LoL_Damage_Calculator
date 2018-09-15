using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using UnityEditor;

public class RunePage : MonoBehaviour {
    /// <summary>
    /// Each heroInfo would instantiate a runePage permanently for himself.
    /// 
    /// Upon Opening:
    /// Generate two columns
    /// Assign first coloumns with path name
    /// Assign the second column
    /// Generate IndividualRunes
    /// Generate IndividualRuneChoices
    /// 
    /// </summary>
    public string userName;

    public GameObject primaryPathSelector;
    public GameObject secondaryPathSelector;

    public RunePath primaryRunePath;
    public RunePath secondaryRunePath;

    public Dropdown runePageSeletor;

    private GameObject priPS;
    private GameObject secPS;
    private List<IndividualRune> individualRunes;
    private HeroInfo userInfo;
    private readonly List<string> strPathNames = new List<string>() { "Precision", "Domination", "Sorcery", "Resolve", "Inspiration" };


    public void Start()
    {
        priPS = GameObjectUtility.CustomInstantiate(primaryPathSelector, transform);
        priPS.transform.SetAsFirstSibling();
        priPS.GetComponent<RunePathSelector>().Initialize(true, this);
    }

    public void Initialize(HeroInfo heroInfo)
    {
        Debug.Log("RunePageInitialized for " + heroInfo.heroName);

        userInfo = heroInfo;
        RunePathData runePathData_1;
        runePathData_1 = LoadRunePathData(true);
        RunePathData runePathData_2;
        runePathData_2 = LoadRunePathData(false);

        DisplaySecondaryRunePathSelector(runePathData_1.pathName);

        DisplayRunePath(runePathData_1, runePathData_2);
    }

    private void DisplaySecondaryRunePathSelector(string primaryPathName)
    {
        if (!secPS)
        {
            secPS = GameObjectUtility.CustomInstantiate(secondaryPathSelector, transform);
            secPS.GetComponent<RunePathSelector>().Initialize(false, this, primaryPathName);
        }
        else
        {
            secPS.GetComponent<RunePathSelector>().RegenearteSecondaryPathSelections(primaryPathName);
        }
    }

    private RunePathData LoadRunePathData(string pathName, bool isPrimary)//for generating an empty runePage
    {
        TextAsset dataAsJson = new TextAsset();
        string jsonPath = pathName;
        dataAsJson = Resources.Load<TextAsset>("RunePathInfo_" + jsonPath + "_Json");
        RunePathData runePathData = new RunePathData();
        runePathData = JsonUtility.FromJson<RunePathData>(dataAsJson.text);
        return runePathData;
    }

    private RunePathData LoadRunePathData(bool isPrimary)//for saved data
    {
        string path = "SavedRunePages/" + userName + "/" + (isPrimary ? "Primary" : "Secondary") + "Path" + runePageSeletor.value;
        if (System.IO.File.Exists("Assets/Resources/" + path + ".json"))
        {
            TextAsset dataAsJson = new TextAsset();
            dataAsJson = Resources.Load<TextAsset>(path);
            RunePathData runePathData = new RunePathData();
            runePathData = JsonUtility.FromJson<RunePathData>(dataAsJson.text);
            Debug.Log("Found Saved RunePage at " + path);
            Debug.Log(RunePathInfo.RunePathData_to_RunePathInfo(runePathData, isPrimary).ToString());
            return runePathData;
        }
        else
        {
            Debug.Log("Did not Find Save RunePage at " + path);
            return LoadRunePathData((isPrimary ? "Domination" : "Inspiration"), isPrimary);
        }
    }

    private void DisplayEmptyRunePage()
    {

    }
    private void DisplayRunePage(RunePageInfo runePageInfo)
    {

    }
    private void DisplayRunePath(RunePathData primaryRunePathData,RunePathData secondaryRunePathData, bool isEmpty = true)
    {
        if (isEmpty)
        {

            RunePathInfo rpInfo = RunePathInfo.RunePathData_to_RunePathInfo(primaryRunePathData, true);//for debug usee
            Debug.Log("Displaying RunePath: \n" + rpInfo.ToString());//for debug use

            primaryRunePath.Initialize(RunePathInfo.RunePathData_to_RunePathInfo(primaryRunePathData, true));
            secondaryRunePath.Initialize(RunePathInfo.RunePathData_to_RunePathInfo(secondaryRunePathData, false));
        }
    }

    public void ChangePath(string selectedPath, bool isPrimary)
    {
        if (isPrimary)
        {
            secPS.GetComponent<RunePathSelector>().RegenearteSecondaryPathSelections(selectedPath);
            primaryRunePath.Clear();
            secondaryRunePath.Clear();
            RunePathData primaryPathData = LoadRunePathData(selectedPath,true);
            RunePathData secondaryPathData = primaryPathData;
            primaryRunePath.Initialize(RunePathInfo.RunePathData_to_RunePathInfo(primaryPathData, true));
            secondaryRunePath.Initialize(RunePathInfo.RunePathData_to_RunePathInfo(secondaryPathData, false));
            secondaryRunePath.Clear();
        }
        else
        {
            secondaryRunePath.Clear();
            RunePathData secondaryPathData = LoadRunePathData(selectedPath, false);
            secondaryRunePath.Initialize(RunePathInfo.RunePathData_to_RunePathInfo(secondaryPathData, false));
        }
    }

    public void DisplayRunePageInfo()
    {
        string[] strs = primaryRunePath.GetSelectedRunes();
        foreach(string str in strs)
        {
            Debug.Log(str+" is selected");
        }
    }



    public void Save()
    {
        RuneInfo rInfo = GetRuneInfo();
        userInfo.SetRuneInfo(rInfo);

        RunePathInfo rp_1 = primaryRunePath.GetPathInfo(true);
        RunePathInfo rp_2 = secondaryRunePath.GetPathInfo(false);

        string jsonStr_1 = JsonUtility.ToJson(RunePathInfo.RunePathInfo_to_RunePathData(rp_1, true), true);
        System.IO.File.WriteAllText("Assets/Resources/SavedRunePages/"+userName+"/" + "PrimaryPath" + runePageSeletor.value + ".json", jsonStr_1);
        string jsonStr_2 = JsonUtility.ToJson(RunePathInfo.RunePathInfo_to_RunePathData(rp_2, true), true);
        System.IO.File.WriteAllText("Assets/Resources/SavedRunePages/"+userName+"/" + "SecondaryPath" + runePageSeletor.value + ".json", jsonStr_2);

        Debug.Log("Saved RunePage exists: "+System.IO.File.Exists("Assets/Resources/" + "SavedRunePages/Annie/PrimaryPath.json"));
        Debug.Log("RunePage Saved");
    }

    public void Close()
    {
        ClearRunePage();
        AssetDatabase.Refresh(ImportAssetOptions.Default);
        gameObject.SetActive(false);
    }

    public void ChangeRunePage()
    {
        Close();
        gameObject.SetActive(true);
        Initialize(userInfo);
        Save();
    }

    private void ClearRunePage()
    {
        primaryRunePath.Clear();
        secondaryRunePath.Clear();
    }

    private RuneInfo GetRuneInfo()
    {
        RuneInfo rInfo = new RuneInfo();
        rInfo.Initialize(primaryRunePath.GetRunePathName(),secondaryRunePath.GetRunePathName());
        string[] strs = primaryRunePath.GetSelectedRunes();
        foreach (string str in strs)
        {
            rInfo.AddRuneStone(str);
        }
        strs = secondaryRunePath.GetSelectedRunes(false);
        foreach (string str in strs)
        {
            rInfo.AddRuneStone(str);
        }
        return rInfo;
    }
}


public class RunePageInfo
{
    /// <summary>
    /// For storing runePageInfo at runtime
    /// </summary>
    public RunePathInfo Path_1;
    public RunePathInfo Path_2;
    public RuneStone[] runeStones;

    public new string ToString()
    {
        StringBuilder str = new StringBuilder("");
        
        str.Append("RunePageInfo");
        str.Append(System.Environment.NewLine);
        str.Append(Path_1.ToString());
        str.Append(Path_2.ToString());
        return str.ToString();
    }
}

public class RunePathInfo
{
    /// <summary>
    /// For storing runePathInfo at runtime
    /// </summary>
    public bool isPrimary = false;
    public string pathName = "";
    public RuneStone keystone = null;
    public RuneStone[] runeStones = null;


    public static RunePathInfo RunePathData_to_RunePathInfo(RunePathData runePathData, bool isPri)
    {
        return new RunePathInfo
        {
            isPrimary = isPri,
            pathName = runePathData.pathName,
            keystone = runePathData.keyStone,
            runeStones = runePathData.runeStones
        };
    }

    public static RunePathData RunePathInfo_to_RunePathData(RunePathInfo rpInfo, bool isPri)
    {
        return new RunePathData
        {
            pathName = rpInfo.pathName,
            keyStone = rpInfo.keystone,
            runeStones = rpInfo.runeStones
        };
    }
    public new string ToString()
    {
        StringBuilder str = new StringBuilder();
        str.Append("Path: " + pathName);
        str.Append(System.Environment.NewLine);
        if (isPrimary)
        {
            str.Append("This is a primary path");
            str.Append(System.Environment.NewLine);
        }
        else
        {
            str.Append("This is a secondary path");
            str.Append(System.Environment.NewLine);
        }

       
        str.Append("keyStone: " + keystone.strName);
        str.Append(System.Environment.NewLine);
        str.Append("exampleStone: " + runeStones[0].strName);
        str.Append(System.Environment.NewLine);
        return str.ToString();
    }
}

[System.Serializable]
public class RunePathData
{
    /// <summary>
    /// For Loading an empty rune page
    /// </summary>
    public string pathName = "";
    public RuneStone keyStone = null;
    public RuneStone[] runeStones = null;
}

[System.Serializable]
public class RuneStone
{
    public string strName = "";
    public string[] stoneChoices = null;
    public string stoneChoiced = "";
}
