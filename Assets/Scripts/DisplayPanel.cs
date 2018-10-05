using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayPanel : MonoBehaviour {

    public GameObject spellListDisplay;

    public GameObject displayTextPrefab;
    public List<GameObject> displayedTexts;
    public List<SpellListItem> spellList;

    public void Initialize()
    {
        displayedTexts = new List<GameObject>();
        spellList = new List<SpellListItem>();
    }
    public void AddSpell(SpellListItem spell)
    {
        spellList.Add(spell);
        UpdateDisplay();
    }
    public void BackSpace()
    {
        Debug.Log("BackSpace");
        spellList.RemoveAt(spellList.Count - 1);
        UpdateDisplay();
    }
    public void BackSpace(SpellListItem spell)
    {
        while (spellList.Remove(spell))
        {
            Debug.Log("Removing " + spell.strSpell);
        }
        UpdateDisplay();
    }

    public void ClearAll()
    {
        Debug.Log("ClearAll");
        spellList = new List<SpellListItem>();
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        foreach (GameObject text in displayedTexts)
        {
            GameObjectUtility.CustomDestroy(text);
        }
        displayedTexts = new List<GameObject>();
        foreach (SpellListItem item in spellList)
        {
            GameObject displayText = GameObjectUtility.CustomInstantiate(displayTextPrefab, spellListDisplay.transform);
            displayText.GetComponent<Text>().text = item.strSpell;
            displayedTexts.Add(displayText);
        }
    }
    public List<SpellListItem> SubmitSpells()
    {
        return spellList;
    }
    public List<string> SubmitSpells(bool isString)
    {
        return new List<string>() { "Q", "W" };
    }

}
