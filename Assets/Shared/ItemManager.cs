using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TempObjID
{
    public int IDNum { get; private set; }
    public int PrefabID { get; private set; }

    public TempObjID(int idNum, int prefabId)
    {
        this.IDNum = idNum;
        this.PrefabID = prefabId;
    }

    internal static TempObjID GetDataFromBytes(NetReader reader)
    {
        int id = reader.ReadInt();
        int pref = reader.ReadInt();
        return new TempObjID(id, pref);
    }

    public void WriteBytes(NetWriter writer)
    {
        writer.WriteInt(IDNum);
        writer.WriteInt(PrefabID);
    }
}

[System.Serializable]
public struct TempObjPrefab
{
    public GameObject serverPrefab;
    public GameObject clientPrefab;
}

public class ItemManager : NetBehaviour
{
    [SerializeField]
    private NetEditorPrefab[] prefabs;

    private Dictionary<TempObjID, ITempNetObject> mapping = new Dictionary<TempObjID, ITempNetObject>();

    private int nextNum = 0;

    public static ItemManager singleton;

    public override void OnNewNetworkSceneLoaded()
    {
        NetworkManager.ClientConnectEvent += SV_OnClientConnected;
        NetworkManager.AddNetFunctionListener("create_temp_obj", thisNetworkID, CL_CreateNetObject);
        NetworkManager.AddNetFunctionListener("destroy_temp_obj", thisNetworkID, CL_RemoveNetObject);
    }

    public void Awake()
    {
        singleton = this;
    }

    public void OnDestroy()
    {
        NetworkManager.ClientConnectEvent -= SV_OnClientConnected;
        NetworkManager.RemoveNetFunctionListener("create_temp_obj", thisNetworkID);
        NetworkManager.RemoveNetFunctionListener("destroy_temp_obj", thisNetworkID);
        singleton = null;
    }

    public int GetPrefabIDForName(string name)
    {
        for(int i = 0; i < prefabs.Length; i++)
        {
            if(prefabs[i].name == name)
            {
                return i;
            }
        }
        return -1;
    }

    #region SERVER
    public override void ServerStart()
    {
        // do nothing
        
    }

    void SV_OnClientConnected(int num)
    {
        
    }

    public ITempNetObject SpawnNewTempNetObject(int prefabID, Vector3 pos, Quaternion rot)
    {
        TempObjID newID = new TempObjID(nextNum,prefabID);
        nextNum++;
        ITempNetObject comp = CreateLocalNetObject(prefabID, newID, pos, rot);

        // send the creation message
        NetWriter writer = NetworkManager.StartNetworkMessage("create_temp_obj", thisNetworkID);
        newID.WriteBytes(writer);
        writer.WriteVector3(comp.GetPosition());
        writer.WriteVector3(comp.GetEulerAngles());
        comp.WriteInitialBytes(writer);
        BufferedMessage msg = NetworkManager.SendMessageToOtherClients(writer,NetworkCore.ReliableSequencedMsg,true);
        comp.SetNetMessage(msg);
        mapping.Add(newID, comp);
        return comp;
    }

    public void RemoveTempNetObject(ITempNetObject obj)
    {
        TempObjID tempId = obj.GetID();
        RemoveLocalNetObject(tempId);
        // remove from queue
        NetworkManager.RemoveBufferedMessage(obj.GetNetMessage());
        NetWriter writer = NetworkManager.StartNetworkMessage("destroy_temp_obj", thisNetworkID);
        tempId.WriteBytes(writer);
        NetworkManager.SendMessageToOtherClients(writer, NetworkCore.ReliableSequencedMsg, false);
    }

    #endregion

    #region CLIENT
    public override void ClientStart()
    {
        // do nothing
    }

    void CL_CreateNetObject(NetReader reader)
    {
        TempObjID tempObjId = TempObjID.GetDataFromBytes(reader);

        Vector3 pos = reader.ReadVector3();
        Quaternion rot = Quaternion.Euler(reader.ReadVector3());

        Debug.Log("Creating object of type " + tempObjId.PrefabID);
        ITempNetObject obj = CreateLocalNetObject(tempObjId.PrefabID, tempObjId, pos, rot);
        
        if (obj != null)
        {
            mapping.Add(tempObjId, obj);
            obj.ReadInitialBytes(reader);
        }
    }

    void CL_RemoveNetObject(NetReader reader)
    {
        TempObjID objID = TempObjID.GetDataFromBytes(reader);
        Debug.Log("removing object with id " + objID.IDNum);
        RemoveLocalNetObject(objID);
    }

    #endregion

    private ITempNetObject CreateLocalNetObject(int prefabID, TempObjID tempObjID, Vector3 pos, Quaternion rot)
    {
        // verify prefab id
        if (prefabID < 0 || prefabID >= prefabs.Length)
        {
            GameLog.Err("invalid prefab id for temp object");
            // log an error
            return null;
        }
        GameObject obj = null;
        if (NetworkCore.isServer)
        {
            obj = Instantiate(prefabs[prefabID].serverPrefab, pos, rot);
        }
        else if (NetworkCore.isClient)
        {
            obj = Instantiate(prefabs[prefabID].clientPrefab, pos, rot);
        }

        if (obj == null)
        {
            GameLog.Err("instantiated temp object is null!");
            return null;
        }

        ITempNetObject tempObj = obj.GetComponent<ITempNetObject>();
        tempObj.SetID(tempObjID);
        return tempObj;
    }

    private void RemoveLocalNetObject(TempObjID objID)
    {
        // lookup the object
        if (mapping.ContainsKey(objID))
        {
            ITempNetObject obj = mapping[objID];
            obj.ObjectRemoved();
            mapping.Remove(objID);
        }
        else
        {
            GameLog.Err("Can't remove temp object.");
        }
    }
}
