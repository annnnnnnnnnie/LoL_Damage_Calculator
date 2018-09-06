using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellButton : MonoBehaviour {

    private Text text;
    private string strSpell;
    private Button btn;
    private SpellPanel spellPanel;

    public void Initialize(string spell, SpellPanel spellPanel)
    {
        text = GetComponentInChildren<Text>();
        text.text = spell;
        strSpell = spell;
        btn = GetComponent<Button>();
        btn.onClick.AddListener(()=> { Click(); });
        this.spellPanel = spellPanel;
    }

    private void Click()
    {
        Debug.Log(text.text + " is clicked");
        spellPanel.AddSpell(strSpell);
    }
}
