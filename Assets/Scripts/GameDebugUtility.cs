using System.Collections.Generic;
using UnityEngine;
using System.Text;

public static class GameDebugUtility
{
    public static void Debug_ShowDictionary(string msg, Dictionary<string, float> dic)
    {
        StringBuilder debugMsg = new StringBuilder();
        foreach (KeyValuePair<string, float> kvPair in dic)
        {
            debugMsg.Append(kvPair.Key + ": " + kvPair.Value + "\n");
        }
        Debug.Log("Showing Dictionary: " + msg + "\n"+ debugMsg.ToString());
    }
}

