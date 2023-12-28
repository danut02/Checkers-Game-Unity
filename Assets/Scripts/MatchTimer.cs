using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public enum clock { whiteTurn, blackTurn}

public class MatchTimer : MonoBehaviourPun
{
    public static MatchTimer instance;
    public GameModel gameModelScript;

    public TMP_Text blackTimerTxt, whiteTimerTxt;
    public double blackTimer, whiteTimer;
    public double blackTimerDisplay, whiteTimerDisplay;
    double crntTurnDuration;


    clock CurrentTurn = clock.whiteTurn;
    public double lastTurntime;

    bool isMasterClient = true;
    [HideInInspector] public bool isTimerRunning = false;

    //Temp
    public TMP_Text turnTimeTxt, timerTxt;

    public string gameMode = string.Empty;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        //lastTurntime = DateTime.Now;
        lastTurntime = PhotonNetwork.Time;

        string matchType = gameModelScript.matchType;
        if (matchType == "H")
        {
            string timeCode = PhotonNetwork.CurrentRoom.CustomProperties["Time"].ToString();
            whiteTimer = _getMatchDuration(timeCode);
            blackTimer = _getMatchDuration(timeCode);
        }
        whiteTimerDisplay = whiteTimer;
        blackTimerDisplay = blackTimer;

        updatTimerText();

        if (matchType == "H" && PhotonNetwork.IsMasterClient)
        {
            isMasterClient = true;
            return;
        }

        gameMode = PhotonNetwork.CurrentRoom.CustomProperties["GameState"].ToString();
        if (gameMode == "Online")
        {
            isMasterClient = gameModelScript.myClientID == 1 ? true : false;
        }
        else if (gameMode == "Puzzle")
        {
            blackTimerTxt.gameObject.SetActive(false);
            whiteTimerTxt.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!isTimerRunning) return;
        if (CurrentTurn == clock.whiteTurn) //White player in turn
        {
            crntTurnDuration = PhotonNetwork.Time - lastTurntime;
            whiteTimerDisplay = whiteTimer - crntTurnDuration;
            if (whiteTimerDisplay < 0)
            {
                isTimerRunning = false;
                whiteTimerDisplay = 0;
                StartCoroutine(gameModelScript.matchEnd(2));
            }
        }
        else
        {
            crntTurnDuration = PhotonNetwork.Time - lastTurntime;
            blackTimerDisplay = blackTimer - crntTurnDuration;
            if (blackTimerDisplay < 0)
            {
                isTimerRunning = false;
                blackTimerDisplay = 0;
                StartCoroutine(gameModelScript.matchEnd(1));
            }
        }
        updatTimerText();
    }
    public void startTimer()
    {
        if (gameMode != "Puzzle")
        {
            isTimerRunning = true;
            lastTurntime = PhotonNetwork.Time;
            if (gameModelScript.myPlayer != null) gameModelScript.myPlayer.isLocked = false;
        }
    }
    public string _crntPlayerTimer()
    {
        if (CurrentTurn == clock.whiteTurn)
            return (whiteTimer - crntTurnDuration).ToString();
        else
            return (blackTimer - crntTurnDuration).ToString();
    }
    public void tapClock(string lastPlayerTime, string turnTime)
    {
        double previousTurnTimer = Convert.ToDouble(lastPlayerTime);
        if (CurrentTurn == clock.whiteTurn)
        {
            whiteTimerDisplay = whiteTimer = previousTurnTimer;
            CurrentTurn = clock.blackTurn;
        }
        else
        {
            blackTimerDisplay = blackTimer = previousTurnTimer;
            CurrentTurn = clock.whiteTurn;
        }
        updatTimerText();
        lastTurntime = Convert.ToDouble(turnTime);
    }
    void updatTimerText()
    {
        if(isMasterClient)
        {
            float minutes = Mathf.FloorToInt((float)(blackTimerDisplay / 60));
            float seconds = Mathf.FloorToInt((float)(blackTimerDisplay % 60));
            blackTimerTxt.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            minutes = Mathf.FloorToInt((float)(whiteTimerDisplay / 60));
            seconds = Mathf.FloorToInt((float)(whiteTimerDisplay % 60));
            whiteTimerTxt.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        else
        {
            float minutes = Mathf.FloorToInt((float)(blackTimerDisplay / 60));
            float seconds = Mathf.FloorToInt((float)(blackTimerDisplay % 60));
            whiteTimerTxt.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            minutes = Mathf.FloorToInt((float)(whiteTimerDisplay / 60));
            seconds = Mathf.FloorToInt((float)(whiteTimerDisplay % 60));
            blackTimerTxt.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

    }


    public static float _getMatchDuration(string timerCode)
    {
        Debug.Log(timerCode);
        switch(timerCode)
        {
            case "0":
                return  60;
            case "1":
                return  180;
            case "2":
                return  300;
            case "3":
                return  420;
            case "4":
                return  600;
        }
        return 0;
    }

    /// <summary>
    /// Returning Match Time in minutes by time id from custom room property
    /// </summary>
    /// <returns></returns>
    public static float _GetMatchTime(string timeId)
    {
        switch (timeId)
        {
            case "0":
                return 1;
            case "1":
                return 3;
            case "2":
                return 5;
            case "3":
                return 7;
            case "4":
                return 10;
        }
        return 0;
    }
}
