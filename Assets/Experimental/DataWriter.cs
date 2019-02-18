using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class DataWriter
{
#if UNITY_EDITOR
    [MenuItem("Network/GenerateDataTables")]
    public static void DoWrite()
    {
        string filePath = Application.dataPath + "/DataTables.cs";

        using (StreamWriter file = new StreamWriter(filePath,false))
        {
            // write header
            WriteHeader(file);

            file.WriteLine("}");
            file.WriteLine("");
            // go over all the scripts
            var scripts = MonoImporter.GetAllRuntimeMonoScripts();
            foreach (var script in scripts)
            {
                // go over all the members
                var cl = script.GetClass();
                if(cl == null)
                {
                    continue;
                }
                var members = cl.GetMembers();

                Dictionary<string, string> dataMembers = new Dictionary<string, string>();

                foreach (var member in members)
                {
                    // find the fields
                    if (member.MemberType != System.Reflection.MemberTypes.Field)
                    {
                        continue;
                    }
                    if (!member.IsDefined(typeof(AutoDataTable), true))
                    {
                        continue;
                    }

                    string dataMethodName = GetDataTypeMethodName(member.ReflectedType.DeclaringType);
                    if (dataMethodName == null)
                    {
                        Debug.LogErrorFormat("Invalid type {0} given for AutoDataTable", member.ReflectedType.DeclaringType.Name);
                    }
                    else
                    {
                        dataMembers.Add(dataMethodName, member.Name);
                    }
                }
                if (dataMembers.Count > 0)
                {
                    MakeDataTable(file, cl.Name, dataMembers);
                }
            }
            
        }

    }
#endif

    static string GetDataTypeMethodName(Type t)
    {
        if(t == typeof(bool))
        {
            return "WriteBool";
        }
        if (t == typeof(byte))
        {
            return "WriteByte";
        }
        if (t == typeof(short))
        {
            return "WriteShort";
        }
        if (t == typeof(int))
        {
            return "WriteInt";
        }
        if (t == typeof(float))
        {
            return "WriteFloat";
        }
        if (t == typeof(Vector3))
        {
            return "WriteVector3";
        }
        return null;
    }

    static void MakeDataTable(StreamWriter stream, string className, Dictionary<string,string> dataMembers)
    {
        stream.WriteLine();
        stream.WriteLine("public void Encode({0} script, NetWriter writer)", className);
        stream.WriteLine("{");
        foreach(var pair in dataMembers)
        {
            stream.WriteLine("    writer.{0}({1});",pair.Key,pair.Value);
        }
        stream.WriteLine("}");
        stream.WriteLine();
    }

    static void WriteHeader(StreamWriter writer)
    {
        writer.WriteLine("using UnityEngine;");
        writer.WriteLine();
        writer.WriteLine("public static class DataTables");
        writer.WriteLine("{");
    }
}



