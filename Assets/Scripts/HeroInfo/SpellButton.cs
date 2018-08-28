using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellButton : MonoBehaviour {

    private Text text;
    private string strSkill;
    private Button btn;
    private SpellPanel spellPanel;

    public void Initialize(string skill, SpellPanel spellPanel)
    {
        text = GetComponentInChildren<Text>();
        text.text = skill;
        strSkill = skill;
        btn = GetComponent<Button>();
        btn.onClick.AddListener(()=> { Click(); });
        this.spellPanel = spellPanel;
    }

    private void Click()
    {
        Debug.Log(text.text + " is clicked");
        spellPanel.AddSpell(strSkill);
    }
}
