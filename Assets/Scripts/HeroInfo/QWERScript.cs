using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QWERScript : MonoBehaviour {//Attached to individual spell level panel

    [HideInInspector]
    public int SpellLevel;
    [HideInInspector]
    public string SpellName;
    private Button levelUpBtn;
    private Button levelDownBtn;
    private Text QWERText;
    private Text levelText;

    public void Initialize(string strSpell)
    {
        SpellLevel = 0;
        SpellName = strSpell;
        QWERText = GetComponent<Text>();
        levelText = GetComponentInChildren<Identifier>().gameObject.GetComponent<Text>();
        QWERText.text = strSpell.ToString();
        levelText.text = SpellLevel.ToString();

        Button[] btns = GetComponentsInChildren<Button>();
        levelUpBtn = btns[0];
        levelDownBtn = btns[1];

        levelUpBtn.onClick.RemoveAllListeners();
        levelUpBtn.onClick.AddListener(() => { ChangeLevel(1); });
        levelDownBtn.onClick.RemoveAllListeners();
        levelDownBtn.onClick.AddListener(() => { ChangeLevel(-1); });
    }
    private void ChangeLevel(int intChange)
    {
        if (SpellLevel + intChange < 0 || SpellLevel + intChange > 5)
        {
            return;
        }
        else
        {
            SpellLevel += intChange;
            levelText.text = SpellLevel.ToString();
        }
    }
    public int GetLevel() { return SpellLevel; }
}
