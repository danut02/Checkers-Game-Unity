using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class HostMatch : MonoBehaviourPunCallbacks, IRoomType
{
    public GameObject hostMatchLobby;
    public GameObject whitePlayerOutline;
    public Button player1Btn;
    public Button player2Btn;
    public Button startGameBtn;
    public TMP_Text[] joinedPlayerNames;
    public TMP_Text cancelButtonText;
    public TMP_Text roomIdText;

    private int whitePlayer = 1;

    public void OnJoined_Room()
    {
        hostMatchLobby.SetActive(true);
        if (PhotonNetwork.IsMasterClient)
        {
            player1Btn.interactable = true;
            player2Btn.interactable = true;
            cancelButtonText.text = "Cancel Match";
            roomIdText.text = "id: " + PhotonNetwork.CurrentRoom.Name;
            startGameBtn.gameObject.SetActive(true);
            whitePlayerOutline.transform.position = player1Btn.transform.position;
        }
        else
            cancelButtonText.text = "Leave Match";
        updatePlayers();
    }

    public void OnLeft_Room()
    {
        hostMatchLobby.SetActive(false);
        MenuManager.instance.loadingScreen.SetActive(true);
    }

    public void OnPlayerEntered_Room(Photon.Realtime.Player NewPlayer)
    {
        updatePlayers();
        if (PhotonNetwork.CurrentRoom.PlayerCount == 3)
            startGameBtn.interactable = true;
        else
            startGameBtn.interactable = false;
    }

    public void OnPlayerLeft_Room(Photon.Realtime.Player OtherPlayer)
    {
        updatePlayers();
        startGameBtn.interactable = false;
    }

    private void updatePlayers()
    {
        joinedPlayerNames[0].text = "Empty";
        joinedPlayerNames[1].text = "Empty";
        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;

        for (int i = 1; i < players.Length; i++)
        {
            if (players[i].IsInactive) continue;
            joinedPlayerNames[i - 1].text = players[i].NickName;
        }
    }

    public void OnClickSelectWhitePlayer(int whitePlayer)
    {
        this.whitePlayer = whitePlayer;
        if (whitePlayer == 1)
            whitePlayerOutline.transform.position = player1Btn.transform.position;
        else
            whitePlayerOutline.transform.position = player2Btn.transform.position;
    }
    public void OnClickStartaMatch()
    {
        photonView.RPC("RPC_StartMatch", RpcTarget.All, whitePlayer);
    }
    public void OnClickCancelMatch()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            photonView.RPC("kickPlayer_Rpc", RpcTarget.AllBuffered);
        }
        else
            PhotonNetwork.LeaveRoom();
    }

    [PunRPC]
    private IEnumerator RPC_StartMatch(int whitePlayer)
    {
        AudioManager.instance.matchFound();
        startGameBtn.interactable = false;
        MenuManager.instance.matchingScreen.SetActive(true);
        AudioManager.instance.transitionIn();

        PlayerPrefs.SetInt("WhitePlayerID", whitePlayer);
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PlayerPrefs.SetInt("MyClientID", 0); //New

            yield return new WaitForSeconds(1f);

            //Updateing room starting time in room
            Hashtable customRoomPropertyies = PhotonNetwork.CurrentRoom.CustomProperties;
            customRoomPropertyies.Add("StartingTime", PhotonNetwork.Time.ToString());
            PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomPropertyies);
            PhotonNetwork.LoadLevel("Game");
        }
        else
        {
            Debug.Log(whitePlayer);
            Debug.Log(PhotonNetwork.LocalPlayer.ActorNumber);
            int whitePlayerActorn = PhotonNetwork.PlayerList[whitePlayer].ActorNumber;
            if (PhotonNetwork.LocalPlayer.ActorNumber == whitePlayerActorn)
                PlayerPrefs.SetInt("MyClientID", 1);
            else
                PlayerPrefs.SetInt("MyClientID", 2);
        }
    }

    [PunRPC]
    private void kickPlayer_Rpc()
    {
        PhotonNetwork.LeaveRoom();
    }
}
