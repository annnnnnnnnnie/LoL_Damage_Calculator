using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellPanel : MonoBehaviour {

    public Button skillButton;
    public Button BackSpaceBtn;
    public Button ClearAllBtn;
    public GameObject DisplayPanel;
    public GameObject DisplayText;
    private List<GameObject> DisplayedText;
    private List<string> spellList;
    [HideInInspector]
    public List<string> skillList;

	public void Initialize(List<string> skills)
    {
        DisplayedText = new List<GameObject>();
        spellList = new List<string>();
        foreach (string skill in skills)
        {
            GameObject sklBtnGO = GameObjectUtility.CustomInstantiate(skillButton.gameObject, transform);
            SpellButton sklBtn = sklBtnGO.GetComponent<SpellButton>();
            sklBtn.Initialize(skill, this);
        }


        GameObject BackBtnGO = GameObjectUtility.CustomInstantiate(BackSpaceBtn.gameObject, transform);
        BackBtnGO.GetComponent<Button>().onClick.RemoveAllListeners();
        BackBtnGO.GetComponent<Button>().onClick.AddListener(() => { BackSpace(); });
        GameObject ClearBtnGO = GameObjectUtility.CustomInstantiate(ClearAllBtn.gameObject, transform);
        ClearBtnGO.GetComponent<Button>().onClick.RemoveAllListeners();
        ClearBtnGO.GetComponent<Button>().onClick.AddListener(() => { ClearAll(); });
    }

    private void BackSpace()
    {
        Debug.Log("BackSpace");
        GameObjectUtility.CustomDestroy(DisplayedText[DisplayedText.Count - 1]);
        DisplayedText.RemoveAt(DisplayedText.Count - 1);
        spellList.RemoveAt(spellList.Count - 1);
    }

    private void ClearAll()
    {
        Debug.Log("ClearAll");
        foreach (GameObject dispText in DisplayedText)
        {
            GameObjectUtility.CustomDestroy(dispText);
        }
        DisplayedText = new List<GameObject>();
        spellList = new List<string>();
    }

    public void AddSpell(string spell)
    {
        GameObject displayText = GameObjectUtility.CustomInstantiate(DisplayText, DisplayPanel.transform);
        //Debug.Log("Adding Spell (SkillPanel)");
        displayText.GetComponent<Text>().text = spell;
        DisplayedText.Add(displayText);
        spellList.Add(spell);
    }

    public List<string> SubmitSpells()
    {
        return spellList;
    }
}
