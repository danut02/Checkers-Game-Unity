using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;

public class RandomMatch : MatchBase, IRoomType
{
    bool isTimerRunning = false;
    private float matchTime = 0; //Set Random
    float timer = 0;

    private void Update()
    {
        if (isTimerRunning)
        {
            timer += Time.deltaTime;
            if (timer > matchTime)
            {
                isTimerRunning = false;
                int hostPlayerId = Random.Range(1, 3);  //New
                photonView.RPC("RPC_StartMatch", RpcTarget.All, true, hostPlayerId);
            }
        }
    }

    public void OnJoined_Room()
    {
        isStartMatch = true;
        matchFoundDetails.SetActive(true);
        MenuManager.instance.waitingLobby.SetActive(true);

        matchTime = Random.Range(12, 15);
        //matchTime = 2;
        timer = 0;
        isTimerRunning = true;
    }
    public void OnLeft_Room()
    {
        isTimerRunning = false;
        isStartMatch = false;

        matchFoundDetails.SetActive(false);
        MenuManager.instance.waitingLobby.SetActive(false);
        MenuManager.instance.loadingScreen.SetActive(true);
    }
    public void OnPlayerEntered_Room(Photon.Realtime.Player NewPlayer)
    {
        isStartMatch = true;

        Debug.Log("Starting Game");
        if (PhotonNetwork.PlayerList.Length == 2)
        {
            isTimerRunning = false;
            int hostPlayerId = Random.Range(1, 3);  //New
            photonView.RPC("RPC_StartMatch", RpcTarget.All, false, hostPlayerId);
        }
    }
    public void OnPlayerLeft_Room(Photon.Realtime.Player OtherPlayer)
    {
        isStartMatch = false;

        waitingLobbyScript.cancelMatch();
    }

    [PunRPC]
    private IEnumerator RPC_StartMatch(bool isBot, int whitePlayer)
    {
        PlayerPrefs.SetInt("WhitePlayerID", whitePlayer);
        AudioManager.instance.matchFound();
        //PhotonNetwork.CurrentRoom.IsOpen = false;
        if (isBot)
        {
            string guestName = new GuestNameGenerator().GuestName;
            PlayerPrefs.SetString("BotName", guestName);
            waitingLobbyScript.matchFound(guestName, ranks[0]);
        }
        else
        {
            int opponentRank = int.Parse(PhotonNetwork.PlayerListOthers[0].CustomProperties["Rank"].ToString());
            opponentRank = NetworkManager.getRankId(opponentRank);
            waitingLobbyScript.matchFound(PhotonNetwork.PlayerListOthers[0].NickName, ranks[opponentRank]);
        }

        yield return new WaitForSeconds(3);
        if (!isStartMatch) yield break;
        MenuManager.instance.matchingScreen.SetActive(true);
        AudioManager.instance.transitionIn();
        if (PhotonNetwork.IsMasterClient)
        {
            //Updateing room starting time in room
            Hashtable customRoomPropertyies = PhotonNetwork.CurrentRoom.CustomProperties;
            customRoomPropertyies.Add("StartingTime", PhotonNetwork.Time.ToString());
            PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomPropertyies);
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PlayerPrefs.SetInt("MyClientID", whitePlayer); //New
        }
        else
        {
            PlayerPrefs.SetInt("MyClientID", whitePlayer == 1 ? 2 : 1); //New
        }
        yield return new WaitForSeconds(1f);
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("Game");
    }
}
