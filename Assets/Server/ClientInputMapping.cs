using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientInputMapping
{
    public List<ClientPlayerPair> pairs = new List<ClientPlayerPair>();
}

public struct ClientPlayerPair
{
    public int localPlayerNum;
    public GamePlayer pPlayer;

    public ClientPlayerPair(int localPlayerNum, GamePlayer gamePlayer)
    {
        this.localPlayerNum = localPlayerNum;
        this.pPlayer = gamePlayer;
    }
}
