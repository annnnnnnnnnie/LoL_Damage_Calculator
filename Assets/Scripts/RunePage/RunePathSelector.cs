using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RunePathSelector : MonoBehaviour
{
    /// <summary>
    /// Calls the ChangePath function on runePage, both primaryPath and secondaryPath
    /// Controlled by the runePage, its RunePathSelections get destroyed and regenerated when path is changed
    /// </summary>
    public GameObject pathSelectionButton;
    public string PathSelected { get; private set; }
    public bool isPrimary { get; set; }
    public RunePage runePage { get; set; }
    private readonly List<string> strPathNames = new List<string>() { "Precision", "Domination", "Sorcery", "Resolve", "Inspiration" };
    private List<GameObject> secPathSelections = new List<GameObject>();


    public void Initialize(bool isPrimary, RunePage runePage, string priPathSelected = "")
    {
        this.isPrimary = isPrimary;
        this.runePage = runePage;
        if (isPrimary)
        {
            GeneratePrimaryPathSelections();
        }
        else
        {
            GenerateSecondaryPathSelections(priPathSelected);
        }
    }

    private void GeneratePrimaryPathSelections()//Primary Path Selectors are not destroyed 
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject gO = GameObjectUtility.CustomInstantiate(pathSelectionButton, transform);
            RunePathSelection rpS = gO.GetComponent<RunePathSelection>();
            rpS.Initialize(strPathNames[i], this, runePage, isPrimary);
        }
    }

    private void GenerateSecondaryPathSelections(string primaryPathSelected)
    {   //Secondary Path Selectors are destroyed and regenerated when a new primary path is selected
        secPathSelections = new List<GameObject>();
        if (primaryPathSelected == "")
        {
            Debug.LogError("Error when Generating Secondary path: No primary path selected");
            throw new System.Exception("Error when Generating Secondary path: No primary path selected");
        }
        for (int i = 0; i < 5; i++)
        {
            if (primaryPathSelected.Equals(strPathNames[i])) continue;
            GameObject gO = GameObjectUtility.CustomInstantiate(pathSelectionButton, transform);
            secPathSelections.Add(gO);
            RunePathSelection rpS = gO.GetComponent<RunePathSelection>();
            rpS.Initialize(strPathNames[i], this, runePage, isPrimary);
        }
    }

    public void RegenearteSecondaryPathSelections(string primaryPathSelected)
    {
        Debug.Log("Regenerating secondary path selections");
        DestroySecondaryPathSelections();
        GenerateSecondaryPathSelections(primaryPathSelected);

    }
    private void DestroySecondaryPathSelections()
    {
        if (secPathSelections == null) return;
        foreach (GameObject gO in secPathSelections)
        {
            if (gO == null) Debug.Log("null gameObject");
            GameObjectUtility.CustomDestroy(gO);
        }
    }

}