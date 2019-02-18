using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GamePlayer
{
    public int playerNum = -1;
    public PlayerType playerType = PlayerType.BOT;
    public int clientConnection = -1;
    internal BaseCar car = null;
    internal bool isGhost = false;
    private Queue<PlayerInput> inputs = new Queue<PlayerInput>();
    private PlayerInput lastKnownInput = PlayerInput.None;

    public GamePlayer(PlayerType playerType)
    {
        this.playerType = playerType;
    }

    public void AddInput(PlayerInput inp)
    {
        lastKnownInput = inp;
        return;
        inputs.Enqueue(inp);
    }

    public PlayerInput ConsumeNextInput()
    {
        return lastKnownInput;

        if(inputs == null)
        {
            //Debug.LogWarning("THIS SHOULDN'T HAPPEN EVER!!!!");
            return PlayerInput.None;
        }
        if(inputs.Count == 0)
        {
            return lastKnownInput;
        }
        PlayerInput data = inputs.Dequeue();
        lastKnownInput = data;

        return data;
    }
}