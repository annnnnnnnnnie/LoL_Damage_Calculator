using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class RuneInfo {

    private Dictionary<string, float> attributes;
    private List<string> runeStones;
    private string runePathOne;
    private string runePathTwo;

    public void Initialize(string rP1, string rP2)
    {
        attributes = new Dictionary<string, float>();
        runeStones = new List<string>();
        SetRunePages(rP1, rP2);
    }

    public void SetRunePages(string runePathOne, string runePathTwo)
    {
        this.runePathOne = runePathOne;
        this.runePathTwo = runePathTwo;
        SetBaseRuneStats(runePathOne, runePathTwo);
    }

    private void AddAttribute(string attriName, float value)
    {
        if (attributes.ContainsKey(attriName))
        {
            attributes[attriName] += value;
        }
        else
        {
            attributes[attriName] = value;
        }
    }

    public Dictionary<string, float> GetAttributes()
    {
        return attributes;
    } 

    public void AddRuneStone(string runeStoneName)
    {
        if (runeStoneName!= "" && runeStones.Contains(runeStoneName))
        {
            Debug.LogError("RuneStone already exists");
        }
        else
        {
            runeStones.Add(runeStoneName);
        }
    }

    public List<string> GetRuneStones()
    {
        return runeStones;
    }


    private void SetBaseRuneStats(string runePathOne, string runePathTwo)
    {
        switch (runePathOne)
        {
            case "Precision":
                switch (runePathTwo)
                {
                    case "Precision":
                        Debug.LogError("No such runePage combination");
                        break;
                    case "Domination":
                        AddAttribute("AttackSpeed", 9);
                        AddAttribute("AdaptiveAD", 6);
                        AddAttribute("AdaptiveAP", 10);
                        break;
                    case "Sorcery":
                        AddAttribute("AttackSpeed", 9);
                        AddAttribute("AdaptiveAD", 6);
                        AddAttribute("AdaptiveAP", 10);
                        break;
                    case "Resolve":
                        AddAttribute("AttackSpeed", 9);
                        AddAttribute("ScalingHealthBase", 6);
                        AddAttribute("ScalingHealthIncrement", 9);
                        break;
                    case "Inspiration":
                        AddAttribute("AttackSpeed", 18);
                        break;
                    default:
                        Debug.LogError("Unrecognized RunePage");
                        break;
                }
                break;
            case "Domination":
                switch (runePathTwo)
                {
                    case "Precision":
                        AddAttribute("AttackSpeed", 5.5f);
                        AddAttribute("AdaptiveAD", 8);
                        AddAttribute("AdaptiveAP", 14);
                        break;
                    case "Domination":
                        Debug.LogError("No such runePage combination");
                        break;
                    case "Sorcery":
                        AddAttribute("AdaptiveAD", 12);
                        AddAttribute("AdaptiveAP", 20);
                        break;
                    case "Resolve":
                        AddAttribute("AdaptiveAD", 6);
                        AddAttribute("AdaptiveAP", 10);
                        AddAttribute("ScalingHealthBase", 6);
                        AddAttribute("ScalingHealthIncrement", 9);
                        break;
                    case "Inspiration":
                        AddAttribute("AdaptiveAD", 12);
                        AddAttribute("AdaptiveAP", 20);
                        break;
                    default:
                        Debug.LogError("Unrecognized RunePage");
                        break;
                }
                break;
            case "Sorcery":
                switch (runePathTwo)
                {
                    case "Precision":
                        AddAttribute("AttackSpeed", 5.5f);
                        AddAttribute("AdaptiveAD", 8);
                        AddAttribute("AdaptiveAP", 14);
                        break;
                    case "Domination":
                        AddAttribute("AdaptiveAD", 12);
                        AddAttribute("AdaptiveAP", 20);
                        break;
                    case "Sorcery":
                        Debug.LogError("No such runePage combination");
                        break;
                    case "Resolve":
                        AddAttribute("AdaptiveAD", 6);
                        AddAttribute("AdaptiveAP", 10);
                        AddAttribute("ScalingHealthBase", 6);
                        AddAttribute("ScalingHealthIncrement", 9);
                        break;
                    case "Inspiration":
                        AddAttribute("AdaptiveAD", 12);
                        AddAttribute("AdaptiveAP", 20);
                        break;
                    default:
                        Debug.LogError("Unrecognized RunePage");
                        break;
                }
                break;
            case "Resolve":
                switch (runePathTwo)
                {
                    case "Precision":
                        AddAttribute("AttackSpeed", 9);
                        AddAttribute("ScalingHealthBase", 6);
                        AddAttribute("ScalingHealthIncrement", 9);
                        break;
                    case "Domination":
                        AddAttribute("AdaptiveAD", 5);
                        AddAttribute("AdaptiveAP", 9);
                        AddAttribute("ScalingHealthBase", 6);
                        AddAttribute("ScalingHealthIncrement", 9);
                        break;
                    case "Sorcery":
                        AddAttribute("AdaptiveAD", 6);
                        AddAttribute("AdaptiveAP", 10);
                        AddAttribute("ScalingHealthBase", 6);
                        AddAttribute("ScalingHealthIncrement", 9);
                        break;
                    case "Resolve":
                        Debug.LogError("No such runePage combination");
                        break;
                    case "Inspiration":
                        AddAttribute("ScalingHealthBase", 16);
                        AddAttribute("ScalingHealthIncrement", 14);
                        break;
                    default:
                        Debug.LogError("Unrecognized RunePage");
                        break;
                }
                break;
            case "Inspiration":
                switch (runePathTwo)
                {
                    case "Precision":
                        AddAttribute("AttackSpeed", 20f);
                        break;
                    case "Domination":
                        AddAttribute("AdaptiveAD", 13);
                        AddAttribute("AdaptiveAP", 22);
                        break;
                    case "Sorcery":
                        AddAttribute("AdaptiveAD", 13);
                        AddAttribute("AdaptiveAP", 22);
                        break;
                    case "Resolve":
                        AddAttribute("ScalingHealthBase", 19.5f);
                        AddAttribute("ScalingHealthIncrement", 15.5f);
                        break;
                    case "Inspiration":
                        Debug.LogError("No such runePage combination");
                        break;
                    default:
                        Debug.LogError("Unrecognized RunePage");
                        break;
                }
                break;
            default:
                Debug.LogError("Unrecognized RunePage");
                break;
        }
    }
    public new string ToString()
    {
        StringBuilder str = new StringBuilder();
        str.Append("---RuneInfo---" + System.Environment.NewLine);
        str.Append("Primary path: " + runePathOne + ", Secondary path: " + runePathTwo + System.Environment.NewLine);
        str.Append(">Base attributes: " + System.Environment.NewLine);
        foreach(KeyValuePair<string,float> kvPair in attributes)
        {
            str.Append(kvPair.Key + " = " + kvPair.Value + System.Environment.NewLine);
        }

        str.Append(">runeStones: " + System.Environment.NewLine);
        foreach (string rS in runeStones)
        {
            str.Append(rS + " ");
        }
        str.Append(System.Environment.NewLine + "---End of Rune Info---");
        return str.ToString();
    }

}
