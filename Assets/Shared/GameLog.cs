using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct LogEntry
{
    public string text;
    public LogType type;
}

public class GameLog : MonoBehaviour
{

    [SerializeField]
    private Rect rect = new Rect(10, 10, 150, 400);

    [SerializeField]
    private Rect bottomLeft = new Rect(10, 390, 150, 25);

    [SerializeField]
    private static Queue<LogEntry> logContents = new Queue<LogEntry>();

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnGUI()
    {
        Rect r = bottomLeft;
        foreach (var item in logContents)
        {
            if (r.height < rect.y)
            {
                return;
            }
            switch (item.type)
            {
                case LogType.Error:
                    GUI.contentColor = Color.red;
                    break;
                default:
                    GUI.contentColor = Color.white;
                    break;
            }
            GUI.Label(r, item.text);
            r.height -= bottomLeft.height;
        }

    }

    public static void Msg(string msg)
    {
        LogEntry entry = new LogEntry();
        entry.text = msg;
        entry.type = LogType.Log;
        logContents.Enqueue(entry);
    }

    public static void Err(string msg)
    {
        LogEntry entry = new LogEntry();
        entry.text = msg;
        entry.type = LogType.Error;
        logContents.Enqueue(entry);
    }
}
