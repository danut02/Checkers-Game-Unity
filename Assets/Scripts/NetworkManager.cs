using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Xml.Linq;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager instance;

    //----------- Other Scripts ------------
    [SerializeField] private FriendsSystem _friendsSystem;
    public TournamentScript tournamentScript;
    public GameModel gameModel;
    [HideInInspector] public gamestate currentGameState;
    [HideInInspector] public matchState startMatch = matchState.None;

    // NEW LINE FOR THE AI
    [HideInInspector] public aiDifficulty aiLevel;
    [HideInInspector] public humanPlayerPuzzle puzzleHumanPlayer;
    [HideInInspector] public humanPlayerAIMatch aiMatchHumanPlayer;

    public const string MAP_PROP_KEY = "map";
    public const string GAME_MODE_PROP_KEY = "gm";
    public const string AI_PROP_KEY = "ai";

    private puzzleOP oPPuzzle
    {
        get
        {
            if (pieceType.value == 0)
                return puzzleOP.AI;
            else
                return puzzleOP.Human;
        }
    }

    public GameObject matchCancelBtn;
    public GameObject loadingPnl;
    public GameObject gameRoomsPnl;
    public GameObject availableRoomsItem;
    public GameObject invalidRoomIdPnl;

    public RandomMatch randomMatch;
    public CustomMatch customMatch;
    public HostMatch hostMatch;

    public IRoomType CurrentRoom;


    private int myId;
    private int rankPoints;

    public Sprite[] ranks;
    public Image rankImage;
    public Image matchingRankImage;

    [Space(5)]
    [Header("Custom Room")]
    public TMP_Dropdown gameMode;
    public TMP_Dropdown roomType;
    public TMP_Dropdown customMatchTime;
    public TMP_Dropdown hostMatchTime;
    public TMP_InputField roomCode;
    public TMP_InputField privateRoomCode;

    [Space(5)]
    [Header("Host Room")]
    public TMP_Dropdown gameMode_H;
    public TMP_InputField roomCode_H;

    [Space(5)]
    [Header("Puzzle Rules")]
    public TMP_Dropdown pieceType;
    public TMP_Dropdown opponentType;

    public RectTransform availableRoomsItem_Container;
    private List<GameObject> crntRoomItems = new List<GameObject>();

    public bool loop;

    private void Awake()
    {
        startMatch = matchState.None;
        if (instance)
        {
            Destroy(gameObject);
        }
        else
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            instance = this;
            PhotonNetwork.AutomaticallySyncScene = true;
        }
    }
    void Start()
    {
        PlayerPrefs.SetInt("RE", 0);
        updateUserRank();
        if (!PhotonNetwork.IsConnected)
        {
            connectOffline();
        }
    }

    private void updateUserRank()
    {
        int rankId = 0;
        rankPoints = PlayfabManager.instance.rankPoints;
        rankId = getRankId(rankPoints);
        rankImage.sprite = ranks[rankId];
        matchingRankImage.sprite = ranks[rankId];
        //Update rank
        if (PhotonNetwork.LocalPlayer.CustomProperties["Rank"] == null)
        {
            var hash = PhotonNetwork.LocalPlayer.CustomProperties;
            hash.Add("Rank", rankPoints);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
        else
        {
            PhotonNetwork.LocalPlayer.CustomProperties["Rank"] = rankPoints;
        }
    }
    public static int getRankId(int rankPoints)
    {
        int id = 0;
        if (rankPoints.ToString().Length < 3) return 0;
        id = int.Parse(rankPoints.ToString().Remove(rankPoints.ToString().Length - 2));
        if (id > 15 && id <= 19) return 15;
        if (id >= 20) return 16;
        return id;
    }
    int getMatchingRank(int rankPoints)
    {
        if (rankPoints < 1500) return 0;
        else if (rankPoints < 2000) return 1;
        else if (rankPoints < 3000) return 2;
        else if (rankPoints < 4000) return 3;
        else if (rankPoints < 5000) return 4;
        else if (rankPoints < 6000) return 5;
        else return 6;
    }

    public void connectOnline(matchState sartingMatchType)
    {
      
       
        currentGameState = gamestate.Online;
        startMatch = sartingMatchType;
        PhotonNetwork.OfflineMode = false;
        MenuManager.instance.loadingScreen.SetActive(true);
        ConnectToServer();
    }
    private void connectOffline()
    {
        currentGameState = gamestate.Offline;
        PhotonNetwork.OfflineMode = true;
    }
    public void ConnectToServer()
    {
        //PhotonNetwork.RejoinRoom();

        if (PhotonNetwork.ReconnectAndRejoin())
        {
            Debug.Log("ReconnectingIsTrue");
        }
        else
        {
            //Debug.Log("ConnectingToMasterServer");
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    public void playOffline()
    {
        startMatch = matchState.Offline;
        currentGameState = gamestate.Offline;

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        else
        {
            PhotonNetwork.OfflineMode = true;
        }
    }
    public void playPuzzle()
    {
        startMatch = matchState.Puzzle;
        currentGameState = gamestate.Puzzle;

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        else
        {
            PhotonNetwork.OfflineMode = true;
        }
    }

    //008
    public void PlayReplayMatch()
    {
        PlayerPrefs.SetInt("RE", 1);
        playOffline();
    }

    // NEW CODE FOR THE AI
    public void playWithAI(string difficulty)
    {
        if (difficulty == "Easy")
        {
            aiLevel = aiDifficulty.Easy;
        }
        else if (difficulty == "Medium")
        {
            aiLevel = aiDifficulty.Medium;
        }
        else
        {
            aiLevel = aiDifficulty.Hard;
        }

        startMatch = matchState.AI;
        currentGameState = gamestate.AI;

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        else
        {
            PhotonNetwork.OfflineMode = true;
        }
    }
    //END NEW CODE
    public void SetPuzzlePiece(string piece)
    {          //FINAL NEW FUNCTION
        if (piece == "White")
        {
            puzzleHumanPlayer = humanPlayerPuzzle.White;
        }
        else
        {
            puzzleHumanPlayer = humanPlayerPuzzle.Black;
        }
    }
    public void SetAIPiece(string piece)
    {              //FINAL NEW FUNCTION
        if (piece == "White")
        {
            aiMatchHumanPlayer = humanPlayerAIMatch.White;
        }
        else
        {
            aiMatchHumanPlayer = humanPlayerAIMatch.Black;
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (currentGameState != gamestate.Online) PhotonNetwork.OfflineMode = true;
    }

    public void findMatch()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable) return;
        if (PhotonNetwork.OfflineMode)
        {
            connectOnline(matchState.RandomMatch);
        }
        else
        {
            MenuManager.instance.waitingLobby.SetActive(true);
            Hashtable customProperties = new Hashtable {{ "Match", "R" }, {"MRank", getMatchingRank(rankPoints).ToString() } };
            
            PhotonNetwork.JoinRandomRoom(customProperties, 2);
        }
    }
    public void cancelMatch()
    {
        PhotonNetwork.LeaveRoom();
    }
    public void showRooms()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable) return;
        if (PhotonNetwork.OfflineMode)
        {
            connectOnline(matchState.GetRooms);
        }
        else
        {
            gameRoomsPnl.SetActive(true);
        }
    }

    public void joinCustomRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
        loadingPnl.SetActive(true);
    }
    public override void OnConnectedToMaster()
    {
        //Call When Connect to master server
        if (currentGameState == gamestate.Online)
        {
            PhotonNetwork.JoinLobby();

            //Calls if you want to start something instant (Set before connecting online)
            if (startMatch != matchState.RandomMatch && startMatch != matchState.GetRooms)
            {
                MenuManager.instance.loadingScreen.SetActive(false);
            }
            PhotonNetwork.LocalPlayer.NickName = PlayfabManager.instance.displayName;
            MenuManager.instance.regionText.text = PhotonNetwork.CloudRegion;
        }

        //Calls if you want to start something instant (Set before connecting Offline)
        if (startMatch == matchState.Offline || startMatch == matchState.Puzzle || startMatch == matchState.AI)
        {
            string GameState = currentGameState.ToString();
            Hashtable customProperties = new Hashtable
            {
                { "GameState", GameState },
                { "Mode", "D" },
                { "aiLevel", aiLevel },
                { "aiPlayer", aiMatchHumanPlayer },     
                { "puzzlePlayer", puzzleHumanPlayer },  
                { "puzzleOp", oPPuzzle },
                { "Rank", "N" },
                { "Match",  "O"}
            };

            RoomOptions roomOpt = new RoomOptions { CleanupCacheOnLeave = false, PlayerTtl = 20000, MaxPlayers = 2 };
            roomOpt.CustomRoomProperties = customProperties;
            PhotonNetwork.CreateRoom(RoomData.getRandomRoomId(), roomOpt);
        }
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (returnCode == 32758) invalidRoomIdPnl.SetActive(true);
        loadingPnl.SetActive(false);
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        RoomOptions roomOption = RoomData.CustomRoomOption(true, 2);
        Hashtable playerProperties = RoomData.CustomPlayerProperty(PlayerPrefs.GetInt("PieceID", 0), PlayerPrefs.GetInt("BoardID", 0));
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        Hashtable customRoomProperties = RoomData.RankRoomProperty("D", getMatchingRank(rankPoints).ToString());
        roomOption.CustomRoomPropertiesForLobby = new string[2] { "Match", "MRank" };
        roomOption.CustomRoomProperties = customRoomProperties;
        PhotonNetwork.CreateRoom(RoomData.getRandomRoomId(), roomOption);
    }

    public override void OnJoinedLobby()
    {
        //008
        //Remove or comment below two lines
        //ClearRooms();
        //avilabelRooms.Clear();


        if (startMatch == matchState.RandomMatch)
        {
            //Start random match
            startMatch = matchState.None;
            MenuManager.instance.loadingScreen.SetActive(false);
            findMatch();
        }
    }
    private List<RoomInfo> avilabelRooms = new List<RoomInfo>();
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
                avilabelRooms.Remove(room);
            else
                avilabelRooms.Add(room);
        }

        ClearRooms();

        float offset = -30;
        float rectHeight = 30;
        foreach (RoomInfo room in avilabelRooms)
        {
            if (room.PlayerCount == 0) continue;

            string mode = room.CustomProperties["Mode"].ToString() == "P" ? "PILDI GANA" : "DAMA";
            int timeId = int.Parse(room.CustomProperties["Time"].ToString());
            if (mode == null) continue; //If no mode value. It means this is rank game and we dont need to display in room list
            Vector3 itemPos = availableRoomsItem.transform.localPosition;
            itemPos.y = offset;
            GameObject roomItem = Instantiate(availableRoomsItem, availableRoomsItem.transform.parent);
            roomItem.transform.localPosition = itemPos;
            roomItem.gameObject.SetActive(true);
            CreatedRoomItem roomItemScript = roomItem.GetComponent<CreatedRoomItem>();
            roomItemScript.updateRoomInfo(room.Name, mode, timeId);
            crntRoomItems.Add(roomItem);
            offset -= 170;
            rectHeight += 170;
        }
        availableRoomsItem_Container.sizeDelta = new Vector2(0, rectHeight);

        if (startMatch == matchState.GetRooms)
        {
            //Get all available rooms
            startMatch = matchState.None;
            MenuManager.instance.loadingScreen.SetActive(false);
            showRooms();
        }
    }

    private void ClearRooms()
    {
        for (int i = 0; i < crntRoomItems.Count; i++)
        {
            Destroy(crntRoomItems[crntRoomItems.Count -1]);
            crntRoomItems.RemoveAt(crntRoomItems.Count -1);
        }
    }

    public override void OnCreatedRoom()
    {
        loadingPnl.SetActive(false);
    }
    public override void OnJoinedRoom()
    {
        Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        Hashtable playerProperties = RoomData.CustomPlayerProperty(PlayerPrefs.GetInt("PieceID", 0), PlayerPrefs.GetInt("BoardID", 0));
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        loadingPnl.SetActive(false);
        if (currentGameState != gamestate.Online)
        {
            StartCoroutine(initiateOfflineGame());
            return;
        }

        if (roomProperties["Match"].ToString() == "C")
            CurrentRoom = customMatch;
        else if (roomProperties["Match"].ToString() == "R")
            CurrentRoom = randomMatch;
        else if (roomProperties["Match"].ToString() == "H")
            CurrentRoom = hostMatch;

        CurrentRoom.OnJoined_Room();
    }


    public override void OnLeftRoom()
    {
        CurrentRoom.OnLeft_Room();
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        CurrentRoom.OnPlayerEntered_Room(newPlayer);
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        CurrentRoom.OnPlayerLeft_Room(otherPlayer);
    }

    public void joinPrivateRoom()
    {
        if (privateRoomCode.text.Length == 0) return;
        joinCustomRoom(privateRoomCode.text);
    }

    private IEnumerator initiateOfflineGame()
    {
        AudioManager.instance.transitionIn();
        MenuManager.instance.matchingScreen.SetActive(true);

        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Game");
    }


    public void OnClickCreateGame()
    {
        string roomName;
        bool roomVisibily;
        if (roomType.value == 0)
        {
            roomVisibily = true;
            roomName = PlayfabManager.instance.displayName + "'s Room";
        }
        else
        {
            roomVisibily = false;
            roomName = roomCode.text;
            if (roomCode.text.Length == 0) return;
        }

        createRoom(roomVisibily, _RoomMode(gameMode), "C", roomName, 2, customMatchTime.value);
    }
    public void CreatePrivateRoom(string roomId)
    {
        createRoom(false, "D", "C", roomId, 2, customMatchTime.value);
    }
    public void OnClickCreateHostRoom()
    {
        string roomName = roomCode_H.text;
        if (roomName.Length == 0) return;
        createRoom(false, _RoomMode(gameMode_H), "H", roomName, 3, hostMatchTime.value);
    }

    private void createRoom (bool visibility, string mode, string matchType, string roomName, byte players, int time)
    {
        RoomOptions roomOption = RoomData.CustomRoomOption(visibility, players);
        PhotonNetwork.LocalPlayer.SetCustomProperties(RoomData.CustomPlayerProperty(PlayerPrefs.GetInt("PieceID", 0), PlayerPrefs.GetInt("BoardID", 0)));
        roomOption.CustomRoomPropertiesForLobby = new string[2] { "Mode", "Time" };
        roomOption.CustomRoomProperties = RoomData.CustomRoomProperty(mode, matchType, time);
        for(int i=0;i<tournamentScript.players.Count;i++)
        PhotonNetwork.CreateRoom(roomName, roomOption);
        loadingPnl.SetActive(true);
    }

    private string _RoomMode(TMP_Dropdown modeDropDown)
    {
        if (gameMode.value == 0)
            return "D";
        else if (gameMode.value == 1)
            return "P";
        else if (gameMode.value == 2)
            return "K1";
        else if (gameMode.value == 3)
            return "K2";
        else
            return "K3";
    }

}