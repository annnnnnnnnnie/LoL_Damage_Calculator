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
    public static string ShowAllDebugMsg()
    {
        StringBuilder str = new StringBuilder();
        foreach (string msg in DebugMsgs)
        {
            str.Append("\n ");
            str.Append(msg);
            str.Append(" (Time: ");
            str.Append(Time.realtimeSinceStartup.ToString());
            str.Append("s)");
        }
        Debug.Log(str.ToString());
        return str.ToString();
    }

    public static void AddDebugMsg(string message)
    {
        DebugMsgs.Add(message);
    }
    public static void AddDebugMsg(string message, int time)
    {
        DebugMsgs.Add(message + "(at " + time + "0ms)");
    }
}

