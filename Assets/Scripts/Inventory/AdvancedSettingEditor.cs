using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdvancedSettingEditor : MonoBehaviour {

    public Button upBtn;
    public Button downBtn;
    public Button resetBtn;

    public Text descriptionText;
    public Text valueText;
    private Item item;
    private string attributeToBeEdited;
    private int originalValue;

    public void Initialize(string attributeToBeEdited, Item item)
    {
        this.item = item;
        this.attributeToBeEdited = attributeToBeEdited;
        originalValue = item.Extras[attributeToBeEdited];

        descriptionText.text = attributeToBeEdited;
        valueText.text = item.Extras[attributeToBeEdited].ToString();

        upBtn.onClick.RemoveAllListeners();
        upBtn.onClick.AddListener(() => { UP(); });

        downBtn.onClick.RemoveAllListeners();
        downBtn.onClick.AddListener(() => { DOWN(); });

        resetBtn.onClick.RemoveAllListeners();
        resetBtn.onClick.AddListener(() => { RESET(); });
    }

    private void UP()
    {
        if (attributeToBeEdited.Equals("Unique_Passive_ManaCharge"))
        {
            item.Extras[attributeToBeEdited] += 50;
        }else if (attributeToBeEdited.Equals("Unique_Active_Spellbinder"))
        {
            item.Extras[attributeToBeEdited] += 10;
        }
        else
        {
            item.Extras[attributeToBeEdited] += 1;
        }
        Debug.Log("Upping " + attributeToBeEdited);
        UpdateValueDisplayed();
    }
    private void DOWN()
    {
        if (attributeToBeEdited.Equals("Unique_Passive_ManaCharge"))
        {


            item.Extras[attributeToBeEdited] -= 50;
        }
        else
        {
            item.Extras[attributeToBeEdited] -= 1;
        }
        Debug.Log("Downing " + attributeToBeEdited);
        UpdateValueDisplayed();
    }
    public void RESET()
    {
        item.Extras[attributeToBeEdited] = originalValue;
        Debug.Log("Reseting " + attributeToBeEdited);
        UpdateValueDisplayed();
    }

    private void UpdateValueDisplayed()
    {
        valueText.text = item.Extras[attributeToBeEdited].ToString();
    }
}
