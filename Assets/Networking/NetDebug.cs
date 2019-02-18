using UnityEngine;

[System.Flags]
enum DebugMode 
{
    none = 0,
    msg = 1 ,
    warning = 2,
    error = 4,
    full = 255
}

internal class NetDebug
{
    public static DebugMode debugMode = DebugMode.full;

    public static void Log(string msg)
    {
        if((debugMode & DebugMode.msg) == DebugMode.msg)
        {
            Debug.Log(msg);
        }
    }

    public static void LogError(string msg)
    {
        if((debugMode & DebugMode.error) == DebugMode.error)
        {
            Debug.LogError(msg);
        }
    }
}