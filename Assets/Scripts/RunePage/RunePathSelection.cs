using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RunePathSelection : MonoBehaviour, IRecycle {

    public string strName { get; set; }
    public RunePathSelector rpathSelector { get; set; }

    private RunePage runePage;
    private bool isPrimary;

    public void Restart()
    {
        Initialize("", null, null, false);
    }
    public void Shutdown()
    {
        Initialize("", null, null, false);
    }
    public void Initialize(string strName, RunePathSelector runePathSelector, RunePage runePage, bool isPrimary)
    {
        this.strName = strName;
        this.runePage = runePage;
        this.isPrimary = isPrimary;
        rpathSelector = runePathSelector;

        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => { HandleOnClick(); });
        btn.GetComponentInChildren<Text>().text = strName;
    }

    private void HandleOnClick()
    {
        SelectThis();
    }

    private void SelectThis()
    {
        Debug.Log(strName +" is selected");
        runePage.ChangePath(strName, isPrimary);
    }

    public void DeselectThis()
    {
        Debug.Log(strName + " is deselected");

    }
}
