using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellButton : MonoBehaviour {

    private Hero user;
    private string strSpell;
    private Button btn;
    private SpellPanel spellPanel;

    public void Initialize(string spell, SpellPanel spellPanel)
    {
        this.user = spellPanel.user;
        GetComponentInChildren<Text>().text = spell;
        strSpell = spell;
        btn = GetComponent<Button>();
        btn.onClick.AddListener(()=> { Click(); });
        this.spellPanel = spellPanel;
    }

    private void Click()
    {
        Debug.Log(strSpell + " is clicked");
        spellPanel.AddSpell(new SpellListItem
        {
            caster = user,
            receivers = null,
            strSpell = this.strSpell
        });
    }
}
