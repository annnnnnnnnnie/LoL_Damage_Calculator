using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellPanel : MonoBehaviour { //Owned by everyhero

    public Hero user;
    public Button spellButton;
    public Button BackSpaceBtn;
    public Button ClearAllBtn;
    public GameObject displayPanelGameObject;
    private DisplayPanel displayPanel;
    private Dictionary<string, GameObject> newSpellBtns;

	public void Initialize(List<string> spells, Hero user)
    {
        this.user = user;
        newSpellBtns = new Dictionary<string, GameObject>();
        foreach (string spell in spells)
        {
            GameObject splBtnGo = GameObjectUtility.CustomInstantiate(spellButton.gameObject, transform);
            SpellButton splBtn = splBtnGo.GetComponent<SpellButton>();
            splBtn.Initialize(spell, this);
        }

        displayPanelGameObject = GameObject.Find("DisplayPanel");
        displayPanel = displayPanelGameObject.GetComponent<DisplayPanel>();

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
        displayPanel.BackSpace();
    }
    private void BackSpace(SpellListItem spell)
    {
        Debug.Log("BackSpace: " + spell.strSpell);
        displayPanel.BackSpace(spell);
    }

    private void ClearAll()
    {
        Debug.Log("ClearAll");
        displayPanel.ClearAll();
    }

    public void AddSpell(SpellListItem spell)
    {
        Debug.Log("Adding Spell: " + user.heroName + " :" +  spell.strSpell);
        displayPanel.AddSpell(spell);
    }

    public void NewSpell(string spell)
    {
        GameObject spellToBeAdded;
        if (newSpellBtns.TryGetValue(spell, out spellToBeAdded))
        {
            Debug.Log("Spell already exists");
        }
        else
        {
            GameObject splBtnGo = GameObjectUtility.CustomInstantiate(spellButton.gameObject, transform);
            SpellButton splBtn = splBtnGo.GetComponent<SpellButton>();
            splBtn.Initialize(spell, this);
            newSpellBtns.Add(spell, splBtnGo);
        }
    }

    public void RemoveSpell(SpellListItem spell)
    {
        GameObject spellToBeRemoved;
        if (newSpellBtns.TryGetValue(spell.strSpell, out spellToBeRemoved))
        {
            newSpellBtns.Remove(spell.strSpell);
            GameObjectUtility.CustomDestroy(spellToBeRemoved);
            BackSpace(spell);
        }
        else
        {
            Debug.Log("No spell removed");
        }
    }
}

public class SpellListItem
{
    public Hero caster;
    public List<Hero> receivers;
    public string strSpell;

    public override bool Equals(object obj)
    {
        var item = obj as SpellListItem;
        return item != null &&
               EqualityComparer<Hero>.Default.Equals(caster, item.caster) &&
               EqualityComparer<List<Hero>>.Default.Equals(receivers, item.receivers) &&
               strSpell == item.strSpell;
    }

    public override int GetHashCode()
    {
        var hashCode = -1253606716;
        hashCode = hashCode * -1521134295 + EqualityComparer<Hero>.Default.GetHashCode(caster);
        hashCode = hashCode * -1521134295 + EqualityComparer<List<Hero>>.Default.GetHashCode(receivers);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(strSpell);
        return hashCode;
    }
}
