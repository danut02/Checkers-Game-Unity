using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using System;

public class CountDown : MonoBehaviourPunCallbacks
{
    public GameModel gameModelScript;
    public TMP_Text countDowntext;
    public double delayTime;

    double matchStartTime = 0;
    double timePassed = 0;
    double remainingTime = 0;

    private void Start()
    {
        if (gameModelScript.gameState != "Online")
        {
            if (PlayerPrefs.GetInt("RE", 0) != 1) GetComponent<MatchTimer>().startTimer(); //008 Start Timer if not a Replay match
            Destroy(this);
        }
        else
        {
            remainingTime = delayTime;
            string startingTime = PhotonNetwork.CurrentRoom.CustomProperties["StartingTime"].ToString();
            matchStartTime = Convert.ToDouble(startingTime);
            countDowntext.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (remainingTime >= 0)
        {
            timePassed = PhotonNetwork.Time - matchStartTime;
            remainingTime = delayTime - timePassed;
            if(remainingTime < 1)countDowntext.text = "Go";
            else countDowntext.text = remainingTime.ToString("0");
        }
        else
        {
            GetComponent<MatchTimer>().startTimer();
            countDowntext.gameObject.SetActive(false);
            Destroy(this);
        }
    }
}
