using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IndividualRuneChoice : MonoBehaviour, IRecycle {

    private string strName;
    private string pathName;
    private GameObject parent;
    private IndividualRuneChoice individualRuneChoice;
    private Image img;

    public void Initialize(string name, GameObject parent, string pathName)
    {
        strName = name;
        this.pathName = pathName;
        GetComponentInChildren<Text>().text = "";
        this.parent = parent;
        img = GetComponent<Image>();
        img.sprite = Resources.Load<Sprite>("RuneIcons/" + pathName+"/" + strName);
        individualRuneChoice = GetComponent<IndividualRuneChoice>();
        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(()=> { HandleClick(); });
    }

    public void SelectThisRune()
    {
        SelectRune();
    }

    private void SelectRune()
    {
        img.color = Color.red;
    }

    public void DeselectThisRune()
    {
        DeselectRune();
    }

    private void DeselectRune()
    {
        img.color = Color.white;
    }
    public void HandleClick()
    {
        parent.GetComponent<IndividualRune>().MakeSelection_Primary(strName, individualRuneChoice);
    }

    public void Restart()
    {
       
    }

    public void Shutdown()
    {
        strName = "";
        parent = null;
        individualRuneChoice = null;
        img.color = Color.white;
    }

}
