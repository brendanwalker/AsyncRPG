#define DEBUG_LEVEL_LOG

using UnityEngine;
using System.Collections;

public class Assert
{
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEBUG_LEVEL_LOG")]
    public static void Test(bool comparison, string message)
    {
        if (!comparison)
        {
            Debug.LogWarning("assert failed! "+message);
            Debug.Break();
        }
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEBUG_LEVEL_LOG")]
    public static void Throw(string message)
    {
        Debug.LogWarning(message);
    }
}