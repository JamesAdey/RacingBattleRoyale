using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExcludeConnectionFilter : NetworkFilter
{
    private int connectionID = -1;
    public ExcludeConnectionFilter(int connectionID)
    {
        this.connectionID = connectionID;
    }
    public override bool CheckConnection(int connectionNum)
    {
        return connectionNum != connectionID;
    }
}
