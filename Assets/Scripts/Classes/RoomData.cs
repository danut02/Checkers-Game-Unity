using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RoomData
{
    public static string getRandomRoomId()
    {
        char[] alphabets = new char[]
        {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'};

        string id = string.Empty;
        for (int i = 0; i < 7; i++)
        {
            id += alphabets[UnityEngine.Random.Range(0, alphabets.Length)];
        }
        return id;
    }
    public static RoomOptions CustomRoomOption(bool isVisible, byte playerCount)
    {
        RoomOptions roomOption = new RoomOptions
        {
            CleanupCacheOnLeave = false,
            IsVisible = isVisible,
            PlayerTtl = 20000,
            MaxPlayers = playerCount
        };
        return roomOption;
    }
    public static RoomOptions HostRoomOption()
    {
        RoomOptions roomOption = CustomRoomOption(false, 3);
        return roomOption;
    }
    public static Hashtable RankRoomProperty(string Mode, string matchingRankId)                               //For Random Rooms
    {
        return new Hashtable
        {
            { "GameState", "Online" },
            { "Mode", Mode },
            { "Rank", "Y" },
            { "Match", "R"},
            { "MRank", matchingRankId}
    };
    }
    public static Hashtable CustomRoomProperty(string Mode, string Match, int timer)               //For Custom Rooms
    {
        return new Hashtable
        {
            { "GameState", "Online" },
            { "Mode", Mode },
            { "Rank", "N" },
            { "Match",  Match},
            { "Time",  timer}
        };
    }


    public static Hashtable CustomPlayerProperty(int PlayerId, int BoardId)
    {
        return new Hashtable
        {
            { "PieceID",  PlayerId},
            { "BoardID",  BoardId}
        };
    }
}