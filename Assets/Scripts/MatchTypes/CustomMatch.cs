using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class CustomMatch : MatchBase, IRoomType
{
    public void OnJoined_Room()
    {
        matchFoundDetails.SetActive(true);
        MenuManager.instance.waitingLobby.SetActive(true);
    }

    public void OnLeft_Room()
    {
        matchFoundDetails.SetActive(false);
        MenuManager.instance.waitingLobby.SetActive(false);
        MenuManager.instance.loadingScreen.SetActive(true);
    }

    public void OnPlayerEntered_Room(Photon.Realtime.Player NewPlayer)
    {
        Debug.Log("Starting Game");
        if (PhotonNetwork.PlayerList.Length == 2)
        {
            int hostPlayerId = Random.Range(1, 3);  //New
            photonView.RPC("RPC_StartMatch", RpcTarget.All, hostPlayerId);
        }
    }

    public void OnPlayerLeft_Room(Photon.Realtime.Player OtherPlayer)
    {
        waitingLobbyScript.cancelMatch();
    }


    [PunRPC]
    private IEnumerator RPC_StartMatch(int whitePlayer)
    {
        PlayerPrefs.SetInt("WhitePlayerID", whitePlayer);
        AudioManager.instance.matchFound();
        waitingLobbyScript.matchFound(PhotonNetwork.PlayerListOthers[0].NickName, ranks[int.Parse(PhotonNetwork.PlayerListOthers[0].CustomProperties["Rank"].ToString())]);

        yield return new WaitForSeconds(3);
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
