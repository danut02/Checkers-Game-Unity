using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PlayFab.ClientModels;
using PlayFab;
using System;
using Random = UnityEngine.Random;

public class InGameUI : MonoBehaviourPunCallbacks
{
    public GameModel gameModelScript;
    //New
    public static InGameUI instance;

    public Emotes emoteScript;
    AdsManager adsManagerScript;

    public TMP_Text localPlayerNameText, opponentPlayerNameText;
    [HideInInspector] public string opponentPlayerName;
    public Sprite[] ranks;
    public Image playerRank, opponentRank;
    public Image gameWinRank, gameLostRank;
    public Image backgroundImage;

    [Header("")]
    int myClientId;
    int myPiecesId;
    int myBoardId;
    int enemyBoardId;
    int enemyPiecesId;
    int myRank;
    public int opRank;

    public GameObject emoteBtn, undoBtn;
    public GameObject enemyBoardSetting;
    public GameObject opponentDisconnectText;
    public GameObject hostUiPanel;
    public GameObject replayMatchPanel;
    public GameObject ingamePanel;

    public SpriteRenderer whiteCheckerSp, blackCheckerSp;
    public SpriteRenderer whiteCheckerHighlitedSp, blackCheckerHighlitedSp;
    public SpriteRenderer boardSp;

    public Sprite[] whiteCheckerPieces, blackCheckerPieces;
    public Sprite[] boards;
    public Sprite[] backgrounds;

    public Toggle enemyBoardToggle;
    public Toggle enemyPiecesToggle;
    public Toggle emoteToggle;
    public Toggle flipViewToggle;


    int noOfPlayersInRoom = 0;
    private bool isGotoMenu;
    private bool isHost = false;

    //New
    public delegate void _UpdateEnemy_Sp(Sprite sp);
    public _UpdateEnemy_Sp _UpdateWhiteEnemyPieces_SP;
    public _UpdateEnemy_Sp _UpdateBlackEnemyPieces_SP;

    public static Action<bool> FlipPiece;
    public static bool IsViewFlipped = false;
    [SerializeField] private Transform cameraParent;

    private string matchType;

    //New
    private void Awake()
    {
        instance = this;
        myRank = (int)PhotonNetwork.LocalPlayer.CustomProperties["Rank"];
        adsManagerScript = AdsManager.instance;
        adsManagerScript.showBannerAd();

        myPiecesId = PlayerPrefs.GetInt("PieceID", 0);
        myBoardId = PlayerPrefs.GetInt("BoardID", 0);
        emoteToggle.isOn = PlayerPrefs.GetInt("ShowEmote", 1) == 1 ? true : false;
        enemyPiecesToggle.isOn = PlayerPrefs.GetInt("ShowEnemyPieces", 1) == 1 ? true : false;
        flipViewToggle.isOn = PlayerPrefs.GetInt("FLIPVIEW", 0) == 1 ? true : false;

    }
    void Start()
    {
        //Update own pieces
        whiteCheckerSp.sprite = whiteCheckerPieces[myPiecesId];
        whiteCheckerHighlitedSp.sprite = whiteCheckerPieces[myPiecesId];
        blackCheckerSp.sprite = blackCheckerPieces[myPiecesId];
        blackCheckerHighlitedSp.sprite = blackCheckerPieces[myPiecesId];


        //update background
        updateBackground();

        noOfPlayersInRoom = PhotonNetwork.CurrentRoom.PlayerCount;
        string gameState = PhotonNetwork.CurrentRoom.CustomProperties["GameState"].ToString();
        matchType = PhotonNetwork.CurrentRoom.CustomProperties["Match"].ToString();
        if (matchType == "H" && PhotonNetwork.IsMasterClient)    //Hosting match owner
        {
            isHost = true;
            //hostUiPanel.SetActive(true);
            localPlayerNameText.text = gameModelScript.whitePhotonPlayer.NickName;
            opponentPlayerNameText.text = gameModelScript.blackPhotonPlayer.NickName;
            opponentPlayerName = opponentPlayerNameText.text;
            myBoardId = int.Parse(gameModelScript.whitePhotonPlayer.CustomProperties["BoardID"].ToString());
            enemyBoardId = int.Parse(gameModelScript.blackPhotonPlayer.CustomProperties["BoardID"].ToString());
            myRank = (int)gameModelScript.whitePhotonPlayer.CustomProperties["Rank"];
            opRank = (int)gameModelScript.blackPhotonPlayer.CustomProperties["Rank"];
            enemyBoardSetting.SetActive(true);
            updateBoard();
            updateRanks();
            return;
        }

        if (gameState == "Online")
        {
            localPlayerNameText.text = PhotonNetwork.LocalPlayer.NickName;
            if (noOfPlayersInRoom == 1)
            {
                opponentPlayerNameText.text = PlayerPrefs.GetString("BotName", "Guest1234");
                opponentPlayerName = opponentPlayerNameText.text;
                enemyBoardId = 0;
                enemyPiecesId = 0;
            }
            else
            {
                enemyPiecesId = (int)gameModelScript.opPhotonPlayer.CustomProperties["PieceID"];
                enemyBoardId = (int)gameModelScript.opPhotonPlayer.CustomProperties["BoardID"];
                opRank = (int)gameModelScript.opPhotonPlayer.CustomProperties["Rank"];
                opponentPlayerNameText.text = gameModelScript.opPhotonPlayer.NickName;
                opponentPlayerName = opponentPlayerNameText.text;
            }
            emoteBtn.SetActive(true);
            enemyBoardSetting.SetActive(true);
            //update ranks
            updateRanks();
        }
        else
        {
            enemyPiecesToggle.isOn = false;
            if (PlayerPrefs.GetInt("RE", 0) == 1)
            {
                //ingamePanel.SetActive(false);
                replayMatchPanel.SetActive(true);
            }
            else
            {
                undoBtn.SetActive(true); // Uncomment this code to show undo button.
                localPlayerNameText.text = "Player 1";
                opponentPlayerNameText.text = "Player 2";
                opponentPlayerName = opponentPlayerNameText.text;
            }
        }

        //Update Board
        updateBoard();
    }

    private void updateRanks()
    {
        if (noOfPlayersInRoom == 1)
        {
            int dif = GameModel.GetInstance().botDif;
            if (dif == 0)
                opRank = Random.Range(0, 700);
            else if (dif == 1)
                opRank = Random.Range(700, 1400);
            else
                opRank = Random.Range(1400, 1700);
        }

        int rankId = NetworkManager.getRankId(myRank);
        int opRankId = NetworkManager.getRankId(opRank);

        playerRank.sprite = ranks[rankId];
        opponentRank.sprite = ranks[opRankId];
        gameWinRank.sprite = ranks[rankId];
        gameLostRank.sprite = ranks[rankId];
    }
    public void updateBoard()
    {
        if (enemyBoardToggle.isOn)
            boardSp.sprite = boards[enemyBoardId];
        else
            boardSp.sprite = boards[myBoardId];

    }
    private void updateBackground()
    {
        backgroundImage.sprite = backgrounds[Random.Range(0, backgrounds.Length)];
    }

    //New
    public void setupEnemyPieces(int myId)
    {
        myClientId = myId;
        if (myId == 1)
        {
            blackCheckerSp.transform.parent.GetComponent<IEnemyPiece>().IsEnemyPiece();
            blackCheckerHighlitedSp.transform.parent.GetComponent<IEnemyPiece>().IsEnemyPiece();
            if (enemyPiecesToggle.isOn)
            {
                blackCheckerSp.sprite = blackCheckerPieces[enemyPiecesId];
                blackCheckerHighlitedSp.sprite = blackCheckerPieces[enemyPiecesId];
            }
        }
        else
        {
            whiteCheckerSp.transform.parent.GetComponent<IEnemyPiece>().IsEnemyPiece();
            whiteCheckerHighlitedSp.transform.parent.GetComponent<IEnemyPiece>().IsEnemyPiece();
            if (enemyPiecesToggle.isOn)
            {
                whiteCheckerSp.sprite = whiteCheckerPieces[enemyPiecesId];
                whiteCheckerHighlitedSp.sprite = whiteCheckerPieces[enemyPiecesId];
            }

            //Rotate All Pieces According Client ID
            whiteCheckerSp.gameObject.transform.eulerAngles = new Vector3(0, 0, 180);
            whiteCheckerHighlitedSp.gameObject.transform.eulerAngles = new Vector3(0, 0, 180);
            blackCheckerSp.gameObject.transform.eulerAngles = new Vector3(0, 0, 180);
            blackCheckerHighlitedSp.gameObject.transform.eulerAngles = new Vector3(0, 0, 180);
        }
    }
    public void setupAllPieces(int whitePlayerId, int blackPlayerId)
    {
        myClientId = 1;
        myPiecesId = whitePlayerId;
        enemyPiecesId = blackPlayerId;
        blackCheckerSp.transform.parent.GetComponent<IEnemyPiece>().IsEnemyPiece();
        blackCheckerHighlitedSp.transform.parent.GetComponent<IEnemyPiece>().IsEnemyPiece();
        whiteCheckerSp.transform.parent.GetComponent<IEnemyPiece>().IsEnemyPiece();
        whiteCheckerHighlitedSp.transform.parent.GetComponent<IEnemyPiece>().IsEnemyPiece();

        whiteCheckerSp.sprite = whiteCheckerPieces[whitePlayerId];
        whiteCheckerHighlitedSp.sprite = whiteCheckerPieces[whitePlayerId];
        if (enemyPiecesToggle.isOn)
        {
            blackCheckerSp.sprite = blackCheckerPieces[blackPlayerId];
            blackCheckerHighlitedSp.sprite = blackCheckerPieces[blackPlayerId];
        }
        else
        {
            blackCheckerSp.sprite = blackCheckerPieces[whitePlayerId];
            blackCheckerHighlitedSp.sprite = blackCheckerPieces[whitePlayerId];
        }
        
    }

    //New
    public void updatePieces()
    {
        PlayerPrefs.SetInt("ShowEnemyPieces", (enemyPiecesToggle.isOn) ? 1 : 0);
        if (enemyPiecesToggle.isOn)
        {
            if (myClientId == 1)
            {
                _UpdateBlackEnemyPieces_SP?.Invoke(blackCheckerPieces[enemyPiecesId]);
            }
            else
            {
                _UpdateWhiteEnemyPieces_SP?.Invoke(whiteCheckerPieces[enemyPiecesId]);
            }
        }
        else
        {
            if (myClientId == 1)
            {
                _UpdateBlackEnemyPieces_SP?.Invoke(blackCheckerPieces[myPiecesId]);
            }
            else
            {
                _UpdateWhiteEnemyPieces_SP?.Invoke(whiteCheckerPieces[myPiecesId]);
            }
        }
    }
    public void updateEmotes()
    {
        if (emoteToggle.isOn)
        {
            PlayerPrefs.SetInt("ShowEmote", 1);
        }
        else
        {
            emoteScript.emotesPanel.SetActive(false);
            if (emoteScript.counter % 2 == 1) emoteScript.counter++;
            PlayerPrefs.SetInt("ShowEmote", 0);
        }
    }

    //Flip View
    public void FlipView()
    {
        IsViewFlipped = flipViewToggle.isOn;  //Save flip state for new pieces

        boardSp.flipX = IsViewFlipped;  //Flip Board

        Vector3 rot = transform.eulerAngles;
        rot.y = IsViewFlipped ? 180 : 0;
        cameraParent.eulerAngles = rot; //Flip Camera

        FlipPiece?.Invoke(IsViewFlipped); //Flip Pieces

        PlayerPrefs.SetInt("FLIPVIEW", flipViewToggle.isOn ? 1 : 0);
    }


    public void gotoMenu()
    {
        //temp
        SceneManager.LoadScene("Menu");

        if (!adsManagerScript.showInterstitialAd())
        {
            SceneManager.LoadScene("Menu");
        }
    }
    public void leaveRoom()
    {
        isGotoMenu = true;
        //New Code
        GameModel.GetInstance().leaveMatch();
        
    }
    public void ILeftRoom()
    {
        if (!isGotoMenu) return;
        int rkPoints;
        if (GameModel.GetInstance().isRankedGame)
        {
            if (PlayfabManager.instance.rankPoints > 200)
            {
                rkPoints = GameModel.CalculateRankPoints(myRank, opRank, "Lost");
                PlayfabManager.instance.rankPoints = rkPoints;
                PlayfabCustomRequests.UpdateUserData("RankPoints", rkPoints.ToString(), OnRankUpdated, OnError);
            }
        }
        PlayerPrefs.SetInt("IsInMatch", 0);
        SceneManager.LoadScene("Menu");
    }

    //Dont Remove
    private void OnRankUpdated(UpdateUserDataResult obj)
    {
    }
    private void OnError(PlayFabError obj)
    {
        Debug.Log(obj.Error);
    }   
}
