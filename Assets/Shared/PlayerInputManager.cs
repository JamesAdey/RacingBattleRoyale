using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{

    private static List<GamePlayer> players = new List<GamePlayer>();


    private void FixedUpdate()
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].playerType == PlayerType.BOT)
            {
                PlayerInput input = NetBot.ThinkForCar(players[i].car);
                players[i].AddInput(input);
            }
        }
    }

    internal static GamePlayer FindNewPlayer()
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].playerType == PlayerType.BOT)
            {
                return players[i];
            }
        }
        return null;
    }

    internal static GamePlayer MakePlayerForCar(BaseCar car)
    {
        Debug.Log("Making player for vehicle " + car.entry.vehicleNumber);
        GamePlayer player = new GamePlayer(PlayerType.BOT);
        player.car = car;
        players.Add(player);
        return player;
    }
}
