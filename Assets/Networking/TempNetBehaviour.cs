﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TempNetBehaviour : MonoBehaviour, ITempNetObject {

    protected Transform thisTransform;

    protected TempObjID tempId;
    protected BufferedMessage bufMsg;

    void Awake()
    {
        thisTransform = GetComponent<Transform>();
    }

    public Vector3 GetEulerAngles()
    {
        return thisTransform.eulerAngles;
    }

    public Vector3 GetPosition()
    {
        return thisTransform.position;
    }

    public void ObjectRemoved()
    {
        Destroy(gameObject);
    }

    public void ReadInitialBytes(NetReader reader)
    {
        // do nothing
    }

    public void WriteInitialBytes(NetWriter writer)
    {
        // do nothing
    }

    public void SetID(TempObjID id)
    {
        tempId = id;
    }

    public TempObjID GetID()
    {
        return tempId;
    }

    public BufferedMessage GetNetMessage()
    {
        return bufMsg;
    }

    public void SetNetMessage(BufferedMessage msg)
    {
        bufMsg = msg;
    }
}