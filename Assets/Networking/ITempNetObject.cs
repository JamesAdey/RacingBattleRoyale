using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITempNetObject {

    void SetID(TempObjID id);
    TempObjID GetID();
    Vector3 GetPosition();
    Vector3 GetEulerAngles();
    void ObjectRemoved();
    void WriteInitialBytes(NetWriter writer);
    void ReadInitialBytes(NetReader reader);
    BufferedMessage GetNetMessage();
    void SetNetMessage(BufferedMessage msg);
}
