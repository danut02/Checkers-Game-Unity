using System;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;


//Whole New Script controls the disconnection during game
public class InGameConnection : MonoBehaviourPunCallbacks
{
    public GameModel gameModelScript;
    public MatchTimer matchTimer_Script;

    public GameObject reconnectionPanel;
    public GameObject matchFinishedPanel;
    public GameObject reconnectingPanel;

    private bool inRoom;
    private bool rejoinCalled;
    private bool reconnectCalled;
    private float reconnectionTimer;

    private DisconnectCause previousDisconnectCause;

    private bool isHostMatch = false;

    private void Start()
    {
        if (gameModelScript.matchType == "H") isHostMatch = true;
        string gameState = gameModelScript.gameState;
        if (gameState == "Online")
        {
            this.inRoom = true;
        }
    }
    

    private void Update()
    {
        if (reconnectionTimer > 0)
            reconnectionTimer -= Time.deltaTime;

        if (reconnectionTimer < 0)
        {
            reconnectionTimer = 0;
            GameModel.GetInstance().gameWin();
        }

    }
    public void TryReconnect()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable) return;

        reconnectionPanel.SetActive(false);
        reconnectingPanel.SetActive(true);

        if (this.inRoom)
        {
            this.rejoinCalled = PhotonNetwork.ReconnectAndRejoin();
            if (!this.rejoinCalled)
            {
                this.reconnectCalled = PhotonNetwork.Reconnect();
            }
        }
        else
        {
            this.reconnectCalled = PhotonNetwork.Reconnect();
        }
        if (!this.rejoinCalled && !this.reconnectCalled)
        {
        }
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        matchTimer_Script.isTimerRunning = false;
        //Connection Lost
        if (CanReconnect(cause))
        {
            reconnectingPanel.SetActive(false);
            reconnectionPanel.SetActive(true);
        }
        else Debug.Log("Cannot Reconnect from this Cause:" + cause);
    }
    private bool CanReconnect(DisconnectCause cause)
    {
        switch (cause)
        {
            // cases that we can recover from
            case DisconnectCause.ServerTimeout:
            case DisconnectCause.Exception:
            case DisconnectCause.ClientTimeout:
            case DisconnectCause.DisconnectByServerLogic:
            case DisconnectCause.AuthenticationTicketExpired:
            case DisconnectCause.DisconnectByServerReasonUnknown:
                return true;
        }
        return false;
    }

    //Photon Callbacks
    public override void OnLeftRoom()
    {
        //this.inRoom = false;
        InGameUI.instance.ILeftRoom();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (isHostMatch)
        {
            if (otherPlayer.ActorNumber == 1) return;
        }
        //New
        InGameUI.instance.opponentDisconnectText.SetActive(true);
        reconnectionTimer = 10;
    }

    public override void OnJoinedRoom()
    {
        if (PlayerPrefs.GetInt("RE", 0) == 1) matchTimer_Script.isTimerRunning = false;

        this.inRoom = true;
        if (this.rejoinCalled)
        {
            this.rejoinCalled = false;
            reconnectingPanel.SetActive(false);
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (this.rejoinCalled)
        {
            if (returnCode.ToString() == "32758")
            {
                reconnectingPanel.SetActive(false);
                matchFinishedPanel.SetActive(true);
            }
            this.rejoinCalled = false;
        }
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        reconnectionTimer = 0;
        InGameUI.instance.opponentDisconnectText.SetActive(false);
    }
    public override void OnConnectedToMaster()
    {
        if (PlayerPrefs.GetInt("RE", 0) != 1) matchTimer_Script.isTimerRunning = true;

        reconnectingPanel.SetActive(false);
        if (this.reconnectCalled)
        {
            this.reconnectCalled = false;
        }
    }
    public void OnClickMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}