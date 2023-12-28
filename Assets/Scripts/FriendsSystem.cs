using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using PlayfabFriendInfo = PlayFab.ClientModels.FriendInfo;

enum FriendIdType { PlayFabId, Username, Email, DisplayName };


public class FriendsSystem : MonoBehaviourPunCallbacks, IChatClientListener
{
    [SerializeField] private NetworkManager NetworkManager;

    private string nickName = PlayfabManager.instance.displayName;
    private ChatClient chatClient;

    List<PlayfabFriendInfo> _friends = null;

    private string _recept = string.Empty;
    private bool isCreatingRoom = false;
    private bool isJoiningRoom = false;

    [SerializeField] private GameObject friendItem;
    [SerializeField] private RectTransform friendListContent;

    [SerializeField] private ChallengePopupItem challengePopup;
    [SerializeField] private GameObject challangeResponsePopup;
    [SerializeField] private TMP_Text challengeResponseText;

    [SerializeField] private TMP_InputField FriendSearchInput;
    [SerializeField] private GameObject FriendSearchItem;

    private WarningPanel _warning;

    async void Start()
    {
        _warning = WarningPanel.instance;
        isJoiningRoom = false;

        await GetFriends();
        await ConnectToPhotonChat();

        challengePopup.OnAccept = AcceptChallenge;
        challengePopup.OnDecline = DeclineChallenge;
    }
    void Update()
    {
        chatClient.Service();
    }
    private void OnDestroy()
    {
        challengePopup.OnAccept -= AcceptChallenge;
        challengePopup.OnDecline -= DeclineChallenge;
    }
    //Connect To Chat Server
    public async Task ConnectToPhotonChat()
    {
        await Task.Delay(0);
        //Debug.Log("Connecting Chat");
        chatClient = new ChatClient(this);
        chatClient.AuthValues = new Photon.Chat.AuthenticationValues(nickName);
        chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion, new Photon.Chat.AuthenticationValues(nickName));
    }


    public void OnConnected()
    {
        //Debug.Log("OnConnected To Chat");
        //SendDirectMessage("Ali", "Lets Play Atif");
        //GetFriends();
    }

    #region Add Friends

    //Get All Friends
    private async Task GetFriends()
    {
        Debug.Log("Getting Friends");
        await Task.Delay(0);
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {
            IncludeFacebookFriends = true,
            XboxToken = null
        }, result => {
            _friends = result.Friends;
            DisplayFriends(_friends); // triggers your UI
        }, DisplayPlayFabError); ;
    }

    int childIndex = 1;
    int childCount = 0;
    void DisplayFriends(List<PlayfabFriendInfo> friendsCache)
    {
        List<PlayfabFriendInfo> sortedFriends = friendsCache.OrderBy(friend => friend.TitleDisplayName).ToList();

        childIndex = 1;
        childCount = friendListContent.childCount;

        sortedFriends.ForEach(f => SpawnFriendItem(f));

        if(childCount > sortedFriends.Count)
        {
            int friendsToRemove = childCount - sortedFriends.Count;
            RemoveFriend(friendsToRemove);
        }
    }

    private void SpawnFriendItem(PlayfabFriendInfo info)
    {
        if(childIndex < childCount)
        {
            Transform tr = friendListContent.GetChild(childIndex);
            tr.GetComponent<FriendUI>().UpdateInfo(info);
            childIndex++;
        }
        else
        {
            GameObject go = Instantiate(friendItem, friendListContent);
            go.SetActive(true);
            go.GetComponent<FriendUI>().UpdateInfo(info);
            go.GetComponent<FriendUI>().OnSendMessage = ChellangeFriend;
            go.GetComponent<FriendUI>().OnUnfriend = RemoveFriend;
        }
    }
    private void RemoveFriend(int friendsToRemove)
    {
        for (int i = 1; i < friendsToRemove; i++)
        {
            Destroy(friendListContent.GetChild(friendListContent.childCount - 1).gameObject);
        }
    }

    public async void OnClickSearchFriend()
    {
        await SearchFriend();
    }
    private async Task SearchFriend()
    {
        await Task.Delay(0);
        string friendId = FriendSearchInput.text;

        if (friendId == "") return;

        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest
        {
            TitleDisplayName = friendId
        },OnFriendGot =>
        {
            if (PhotonNetwork.NickName.ToLower() != OnFriendGot.AccountInfo.TitleInfo.DisplayName.ToLower())
            {
                FriendSearchItem.SetActive(true);
                FriendSearchItem.GetComponent<FriendUI>().UpdateName(friendId);
            }
            else
                FriendSearchItem.SetActive(false);

        }, Error =>
        {
            _warning.showWarining(Error.ErrorMessage);
        });
    }
    public async void OnAddFriend()
    {
        string friendId = FriendSearchInput.text;
        await AddFriend(FriendIdType.DisplayName, friendId);
    }
    private async Task AddFriend(FriendIdType idType, string friendId)
    {
        await Task.Delay(0);
        var request = new AddFriendRequest();
        switch (idType)
        {
            case FriendIdType.PlayFabId:
                request.FriendPlayFabId = friendId;
                break;
            case FriendIdType.Username:
                request.FriendUsername = friendId;
                break;
            case FriendIdType.Email:
                request.FriendEmail = friendId;
                break;
            case FriendIdType.DisplayName:
                request.FriendTitleDisplayName = friendId;
                break;
        }
        // Execute request and update friends when we are done
        PlayFabClientAPI.AddFriend(request, async result =>
        {
            _warning.showWarining("Friend Added");
            FriendSearchItem.SetActive(false);
            await GetFriends();
        }, Error =>
        {
            _warning.showWarining(Error.ErrorMessage.ToString());
        });
    }
    void RemoveFriend(PlayfabFriendInfo friendInfo)
    {
        PlayFabClientAPI.RemoveFriend(new RemoveFriendRequest
        {
            FriendPlayFabId = friendInfo.FriendPlayFabId
        }, result => {
            _friends.Remove(friendInfo);
            DisplayFriends(_friends);
        }, DisplayPlayFabError);
    }
    #endregion

    #region Invite Friends
    private void ChellangeFriend(string friendId)
    {
        SendDirectMessage(friendId, "C");
    }
    public void SendDirectMessage(string recipient, string message)
    {
        chatClient.SendPrivateMessage(recipient, message);
    }
    //On Reieveed Message
    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        if (!string.IsNullOrEmpty(message.ToString()))
        {
            string[] splitNames = channelName.Split(new char[] { ':' });
            string senderName = splitNames[0];
            if (!sender.Equals(senderName, StringComparison.OrdinalIgnoreCase))
            {
                //Debug.Log($"{sender}:{message}");

                if (message.ToString() == "A") OnChallandedAccepted(sender.ToString());
                else if (message.ToString() == "D") OnChallengeDeclined(sender.ToString());
                else if (message.ToString() == "C") OnChallenged(sender.ToString());
            }

        }
    }
    //Send Accept message
    public void AcceptChallenge(string recept)
    {
        _recept = recept;
        if (PhotonNetwork.OfflineMode)
        {
            NetworkManager.connectOnline(matchState.CustomRoom);
            return;
        }

        NetworkManager.CreatePrivateRoom(PhotonNetwork.NickName + "'sRoom");
        isCreatingRoom = true;
    }
    //Send Decline Message
    public void DeclineChallenge(string recept)
    {
        SendDirectMessage(recept, "D");
    }
    //Challenge Recieved
    private void OnChallenged(string challenger)
    {
        challengePopup.ShowChallangePopup(challenger);
    }
    //He accepted challenge
    private void OnChallandedAccepted(string roomId)
    {
        _recept = roomId;

        if (PhotonNetwork.OfflineMode)
        {
            isJoiningRoom = true;
            NetworkManager.connectOnline(matchState.CustomRoom);
            return;
        }
        PhotonNetwork.JoinRoom(roomId + "'sRoom");
    }
    //He Declined challenge
    private void OnChallengeDeclined(string sender)
    {
        challengeResponseText.text = sender + ": Can't play now!";
        challangeResponsePopup.SetActive(true);
    }
    #endregion

    #region PhotonFriendList
    //Not using currently

    public bool FindFriends(string[] friendsUserIds)
    {
        return PhotonNetwork.FindFriends(friendsUserIds);
    }

    public override void OnFriendListUpdate(List<Photon.Realtime.FriendInfo> friendsInfo)
    {
        Debug.Log("Finding Friends");
        for (int i = 0; i < friendsInfo.Count; i++)
        {
            Photon.Realtime.FriendInfo friend = friendsInfo[i];
            Debug.LogFormat("{0}", friend);
        }
    }

    #endregion
    #region PhotonServerCallbacks
    public override void OnConnectedToMaster()
    {
        isCreatingRoom = false;

    }
    public override void OnJoinedLobby()
    {
        if (NetworkManager.startMatch == matchState.CustomRoom)
        {
            if(isJoiningRoom)
            {
                isJoiningRoom = false;
                PhotonNetwork.JoinRoom(_recept + "'sRoom");
                _recept = string.Empty;
            }
            else
            {
                NetworkManager.startMatch = matchState.None;
                isCreatingRoom = true;
                NetworkManager.CreatePrivateRoom(PhotonNetwork.NickName + "'sRoom");
                MenuManager.instance.loadingScreen.SetActive(false);
            }
        }
    }

    public override void OnJoinedRoom()
    {
        if (isCreatingRoom == true) SendDirectMessage(_recept, "A");
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
    }

    #endregion

    #region PhotonChatCallbacks
    void DisplayPlayFabError(PlayFabError error) { Debug.Log(error.GenerateErrorReport()); }

    public void OnChatStateChange(ChatState state)
    {
    }

    public void DebugReturn(DebugLevel level, string message)
    {
    }

    public void OnDisconnected()
    {
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
    }

    public void OnUnsubscribed(string[] channels)
    {
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
    }

    public void OnUserSubscribed(string channel, string user)
    {
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
    }
    #endregion

   








}
