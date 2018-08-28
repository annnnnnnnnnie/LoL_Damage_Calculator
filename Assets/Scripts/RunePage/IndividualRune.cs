using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IndividualRune : MonoBehaviour {

    /// <summary>
    /// Every IndividualRune is a button with a child with a gridLayout.
    /// Initialize individualRune by giving it name and 3-4 individualRuneChoices.
    /// 
    /// Upon Clicking, individualRuneChoices are deselected.
    /// 
    /// When a selection is made, it is stored in individualRune.
    /// 
    /// </summary>
    public GameObject individualRuneChoice;
    [HideInInspector]
    
    private string strSelectedRuneName;
    private string pathName;
    private RuneStone runeStone;
    private bool isPrimary;
    private List<GameObject> individualRuneChoicesGameObjects;

    private IndividualRuneChoice currentSelectedRuneChoice;

    public void Initialize(RuneStone runeStone, string pathName)
    {
        this.pathName = pathName;
        this.runeStone = runeStone;
        strSelectedRuneName = runeStone.stoneChoiced;
        isPrimary = true;
        GetComponent<Button>().GetComponentInChildren<Text>().text = runeStone.strName;
        GeneratePrimaryRuneChoices(runeStone, pathName);

        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => { Deselect(); });
    }
    public void Initialize(RuneStone[] runeStones, string pathName)
    {
        this.pathName = pathName;
        isPrimary = false;
        GenerateSecondaryRuneChoices(runeStones);
    }

    private void GeneratePrimaryRuneChoices(RuneStone rs, string pathName)
    {
        individualRuneChoicesGameObjects = new List<GameObject>();
        foreach (string NameOfruneChoice in rs.stoneChoices)
        {
            GameObject runeChoiceGameObject = GameObjectUtility.CustomInstantiate(individualRuneChoice, this.transform);
            individualRuneChoicesGameObjects.Add(runeChoiceGameObject);
            runeChoiceGameObject.GetComponent<IndividualRuneChoice>().Initialize(NameOfruneChoice, this.gameObject, pathName);
            if (NameOfruneChoice.Equals(rs.stoneChoiced))
            {
                runeChoiceGameObject.GetComponent<IndividualRuneChoice>().HandleClick();
            }
        }
    }
    private void GenerateSecondaryRuneChoices(RuneStone[] runes)
    {
        individualRuneChoicesGameObjects = new List<GameObject>();
    }

    public void MakeSelection_Primary(string strName, IndividualRuneChoice individualRuneChoice)
    {
        Deselect();
        strSelectedRuneName = strName;
        Debug.Log(strName + " is selected");
        currentSelectedRuneChoice = individualRuneChoice;
        currentSelectedRuneChoice.SelectThisRune();
    }

    public void Deselect()
    {
        if (currentSelectedRuneChoice != null)
        {
            currentSelectedRuneChoice.DeselectThisRune();
            strSelectedRuneName = "";
        }
    }

    public void MakeSelection_Secondary(string strName, IndividualRuneChoice individualRuneChoice)
    {

    }


    private void ClearRuneChoices(List<GameObject> gameObjects)
    {
        if (gameObjects != null)
        {
            foreach (GameObject go in gameObjects)
            {
                GameObjectUtility.CustomDestroy(go);
            }
        }
    }

    public string GetSelectedRune()
    {

        if (strSelectedRuneName.Length > 2)
        {
            return strSelectedRuneName;
        }
        else
        {
            return "";
        }
    }
    public void Restart()
    {

    }

    public void Shutdown()
    {
        individualRuneChoicesGameObjects = new List<GameObject>();
        currentSelectedRuneChoice = null;
        strSelectedRuneName = "";
    }

    public void Clear()
    {
        ClearRuneChoices(individualRuneChoicesGameObjects);
    }
    
}
