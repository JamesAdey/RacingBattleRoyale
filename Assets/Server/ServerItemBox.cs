using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerItemBox : MonoBehaviour, ITempNetObject
{

    private Transform thisTransform;
    private TempObjID objId;
    private BufferedMessage bufMsg;

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

    void OnTriggerEnter(Collider other)
    {
        BaseCar car = other.transform.root.GetComponent<BaseCar>();
        if (car != null)
        {
            car.RandomItem();
        }
    }

    public void SetID(TempObjID id)
    {
        objId = id;
    }

    public TempObjID GetID()
    {
        return objId;
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

