using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RunePath : MonoBehaviour, IRecycle {
    /// <summary>
    /// Attached to an empty panel layout group
    /// </summary>
    public GameObject individualRuneGameObject;
    public GameObject columnGameObject;
    public string strPathName { get; set; }

    private List<GameObject> runeStones;
    private RunePathInfo runePathInfo;

    public void Initialize(RunePathInfo runePathInfo)
    {
        this.runePathInfo = runePathInfo;
        strPathName = runePathInfo.pathName;
        runeStones = new List<GameObject>();
        GenerateRunePath(runePathInfo);
    }


    private void GenerateRunePath(RunePathInfo runePathInfo)
    {
        if (runePathInfo.isPrimary)
        {
            Debug.Log("Generating Primary Path");
            GameObject runeStone = GameObjectUtility.CustomInstantiate(individualRuneGameObject, columnGameObject.transform);
            runeStones.Add(runeStone);
            runeStone.GetComponent<IndividualRune>().Initialize(runePathInfo.keystone, strPathName);
            foreach (RuneStone rs in runePathInfo.runeStones)
            {
                GameObject rsGo = GameObjectUtility.CustomInstantiate(individualRuneGameObject, columnGameObject.transform);
                runeStones.Add(rsGo);
                rsGo.GetComponent<IndividualRune>().Initialize(rs, strPathName);
            }
        }
        else
        {
            Debug.Log("Generating Secondary Path");
            foreach (RuneStone rs in runePathInfo.runeStones)
            {
                GameObject rsGo = GameObjectUtility.CustomInstantiate(individualRuneGameObject, columnGameObject.transform);
                runeStones.Add(rsGo);
                rsGo.GetComponent<IndividualRune>().Initialize(rs, strPathName);
            }
        }
    }

    public string[] GetSelectedRunes(bool isPrimary = true)
    {
        if (isPrimary)
        {
            string[] strs = new string[4];
            for (int i = 0; i < 4; i++)
            {
                strs[i] = runeStones[i].GetComponent<IndividualRune>().GetSelectedRune();
                if (i == 0)
                {
                    runePathInfo.keystone.stoneChoiced = strs[i];
                }
                else
                {
                    runePathInfo.runeStones[i - 1].stoneChoiced = strs[i];
                }

            }
            return strs;
        }
        else
        {
            string[] strs = new string[3];
            for (int i = 0; i < 3; i++)
            {
                strs[i] = runeStones[i].GetComponent<IndividualRune>().GetSelectedRune();
                runePathInfo.runeStones[i].stoneChoiced = strs[i];
            }
            return strs;
        }
    }
    
    public string GetRunePathName()
    {
        return strPathName;
    }

    public RunePathInfo GetPathInfo(bool isPrimary)
    {
        GetSelectedRunes(isPrimary);
        return runePathInfo;
    }

    public void Clear()
    {
        if(runeStones != null)
        {
            foreach(GameObject rs in runeStones)
            {
                rs.GetComponent<IndividualRune>().Clear();
                GameObjectUtility.CustomDestroy(rs);
            }
        }
    }
    public void Restart()
    {
        
    }

    public void Shutdown()
    {
        
    }

}


