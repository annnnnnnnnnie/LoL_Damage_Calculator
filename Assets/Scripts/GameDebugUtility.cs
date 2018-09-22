using System.Collections.Generic;
using UnityEngine;
using System.Text;

public static class GameDebugUtility
{
    private static List<string> DebugMsgs;
    public static void Debug_ShowDictionary(string msg, Dictionary<string, float> dic)
    {
        StringBuilder debugMsg = new StringBuilder();
        foreach (KeyValuePair<string, float> kvPair in dic)
        {
            debugMsg.Append(kvPair.Key + ": " + kvPair.Value + "\n");
        }
        Debug.Log("Showing Dictionary: " + msg + "\n"+ debugMsg.ToString());
    }

    public static void Initialize()
    {
        DebugMsgs = new List<string>();
    }
    public static void ShowAllDebugMsg()
    {
        StringBuilder str = new StringBuilder();
        foreach (string msg in DebugMsgs)
        {
            str.Append("\n");
            str.Append(Time.realtimeSinceStartup.ToString());
            str.Append(" ");
            str.Append(msg);
        }
        Debug.Log(str.ToString());
    }

    public static void AddDebugMsg(string message)
    {
        DebugMsgs.Add(message);
    }

}

