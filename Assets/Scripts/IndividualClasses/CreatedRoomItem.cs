using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CreatedRoomItem : MonoBehaviour
{
    private string roomName = string.Empty;
    public TMP_Text roomNameText;
    public TMP_Text gameModeText;
    public TMP_Text matchTimerText;

    public void updateRoomInfo(string name, string mode, int matchDuration)
    {
        roomName = name;
        roomNameText.text = roomName;
        gameModeText.text = mode;
        matchTimerText.text = MatchTimer._GetMatchTime(matchDuration.ToString()) + " Minutes";
    }

    public void joinRoom()
    {
        NetworkManager.instance.joinCustomRoom(roomName);
    }
}
