using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Items {

    private static Dictionary<byte, Powerup> idToPowers = new Dictionary<byte, Powerup>();

    private static byte nextID = 0;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p"></param>
    /// <returns>the ID allocated to the powerup</returns>
    public static byte RegisterPowerup(Powerup p)
    {
        nextID++;
        idToPowers.Add(nextID, p);
        return nextID;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns>The powerup for the given ID if one exists, null otherwise.</returns>
    public static Powerup GetPowerupForID(byte id)
    {
        if (idToPowers.ContainsKey(id))
        {
            return idToPowers[id];
        }

        return null;
    }

}
