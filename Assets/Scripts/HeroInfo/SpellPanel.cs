using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellPanel : MonoBehaviour {

    public Button spellButton;
    public Button BackSpaceBtn;
    public Button ClearAllBtn;
    public GameObject DisplayPanel;
    public GameObject DisplayText;
    private List<GameObject> DisplayedText;
    private List<string> spellList;
    private Dictionary<string, GameObject> newSpellBtns;

	public void Initialize(List<string> spells)
    {
        DisplayedText = new List<GameObject>();
        spellList = new List<string>();
        newSpellBtns = new Dictionary<string, GameObject>();
        foreach (string spell in spells)
        {
            GameObject splBtnGo = GameObjectUtility.CustomInstantiate(spellButton.gameObject, transform);
            SpellButton splBtn = splBtnGo.GetComponent<SpellButton>();
            splBtn.Initialize(spell, this);
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

    public void NewSpell(string spell)
    {
        GameObject splBtnGo = GameObjectUtility.CustomInstantiate(spellButton.gameObject, transform);
        SpellButton splBtn = splBtnGo.GetComponent<SpellButton>();
        splBtn.Initialize(spell, this);
        newSpellBtns.Add(spell, splBtnGo);
    }

    public void RemoveSpell(string spell)
    {
        GameObject spellToBeRemoved = newSpellBtns[spell];
        newSpellBtns.Remove(spell);
        GameObjectUtility.CustomDestroy(spellToBeRemoved);
    }

    public List<string> SubmitSpells()
    {
        return spellList;
    }
}
