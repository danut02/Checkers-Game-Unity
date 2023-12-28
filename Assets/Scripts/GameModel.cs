using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using System.Threading.Tasks;

public class GameModel : MonoBehaviourPunCallbacks
{
    private static GameModel instance;
    public static GameModel GetInstance()
    {
        if (instance == null)
        {
            instance = GameObject.FindGameObjectWithTag("GameModel").GetComponent<GameModel>();
        }
        return instance;
    }
    static public readonly string CheckerTag = "Checker";
    static public readonly string FieldTag = "Field";
    //------------------- Other Scritps -------------------
    public InGameUI GameUI_Script;
    PlayfabManager playFabScript;

    [HideInInspector] public string gameState;
    [HideInInspector] public string gameMode;
    [HideInInspector] public string matchType;
    [HideInInspector] public bool isRankedGame;
    [HideInInspector] public bool isMatchEnd;
    [HideInInspector] public bool isReplayMatch = false;
    public NetworkManager networkManager;
    public int winner;

    // NEW LINE FOR THE AI
    string aiDifficulty;
    string puzzlePlayer;        //---------FINAL NEW LINES
    string aiPlayerPiece;            //---------FINAL NEW LINES
    string puzzleOP;

    public int CurrentPlayerID;
    public int drawMoves;
    public int myClientID;
    public int opponentGameID;
    public int WhiteClientID;
    public int BlackClientID;
    public int WhitePlayerActorN;
    public int BlackPlayerActorN;
    [HideInInspector] public int whitePlayerPieceId;
    [HideInInspector] public int blackPlayerPieceId;
    public int turnNo;    //New this if for keep tracking of previous done turns 
    
    public Player myPlayer;
    public GameObject opPlayer;

    [HideInInspector] public int BoardSize = 8;
    public int NumCheckersRows = 2;
    public CheckerData[,] board;
    private bool[,] boardPieces;
    public CheckerData[,] copiedBoard;
    public CheckerData[,] Board
    {
        get
        {
            return board;
        }
    }
    //public List<CheckerDataClone[,]> lastBoards = new List<CheckerDataClone[,]>();
    public GameObject whiteHighlightPiece, blackHighlightPiece;
    public List<Move> PossibleMoves;
    private List<GameObject> movePlaces = new List<GameObject>();
    private List<GameObject> CapuredPieces = new List<GameObject>();
    private List<GameObject> capturedPiecesDummies = new List<GameObject>();
    public List<Vec2> capturedPieces = new List<Vec2>();

    //puzzle
    private puzzleData _puzzleData = new puzzleData();
    private int puzzleNo;

    [Header("GameView")]
    public GameObject playerPrefeb;
    public GameObject FieldPrefab;
    public GameObject whiteChecker, blackChecker;
    public GameObject WinPanel, LostPanel, drawPanel, offlinePanel;

    public TMP_Text offlineMatchEndtext;
    public TMP_Text drawMovesText;
    public TMP_Text undoText;
    public TMP_Text lastMoveText;
    public TMP_Text whitePiecesText;
    public TMP_Text blackPiecesText;

    private Dictionary<Vector2, int> movesPositionsIds = new Dictionary<Vector2, int>()
    {
        {new Vector2(0,0), 9 },
        {new Vector2(2,0), 10 },
        {new Vector2(4,0), 11 },
        {new Vector2(6,0), 12 },

        {new Vector2(1,1), 5 },
        {new Vector2(3,1), 6 },
        {new Vector2(5,1), 7 },
        {new Vector2(7,1), 8 },

        {new Vector2(0,2), 4 },
        {new Vector2(2,2), 1 },
        {new Vector2(4,2), 2 },
        {new Vector2(6,2), 3 },

        {new Vector2(1,3), 16 },
        {new Vector2(3,3), 15 },
        {new Vector2(5,3), 14 },
        {new Vector2(7,3), 13 },

        {new Vector2(0,4), 13 },
        {new Vector2(2,4), 14 },
        {new Vector2(4,4), 15 },
        {new Vector2(6,4), 16 },

        {new Vector2(1,5), 3 },
        {new Vector2(3,5), 2 },
        {new Vector2(5,5), 1 },
        {new Vector2(7,5), 4 },

        {new Vector2(0,6), 8 },
        {new Vector2(2,6), 7 },
        {new Vector2(4,6), 6 },
        {new Vector2(6,6), 5 },

        {new Vector2(1,7), 12 },
        {new Vector2(3,7), 11 },
        {new Vector2(5,7), 10 },
        {new Vector2(7,7), 19 },
    };

    public List<Vec2> blackPieces, whitePieces;
    public List<Vec2> promotedPieces;

    private int noOfPlayersInRoom;
    private Photon.Realtime.Player[] allPlayersInRoom;
    public Photon.Realtime.Player whitePhotonPlayer;
    public Photon.Realtime.Player blackPhotonPlayer;
    private Photon.Realtime.Player myPhotonPlayer;
    public Photon.Realtime.Player opPhotonPlayer;

    [HideInInspector] public int botDif;
    float pieceBlinkTime;

    public bool isTesingScenerio;
    [HideInInspector] public bool isProcessingTurn = false;
    bool isGameDraw;

    [Header("Host Match")]
    [HideInInspector] public bool isHost = false;

    //Undo System
    private IMoveRecorder moveRecorder;

    //Move Highlighting on board
    [SerializeField] private MovesRepresentator moveHighlighter;

    public bool isPlayingStarted = false;

    private void Awake()
    {
        instance = this;
        //Getting room info
        gameMode = PhotonNetwork.CurrentRoom.CustomProperties["Mode"].ToString();
        gameState = PhotonNetwork.CurrentRoom.CustomProperties["GameState"].ToString();
        matchType = PhotonNetwork.CurrentRoom.CustomProperties["Match"].ToString();
        isRankedGame = PhotonNetwork.CurrentRoom.CustomProperties["Rank"].ToString() == "Y" ? true : false;
        isReplayMatch = PlayerPrefs.GetInt("RE", 0) == 1 ? true : false;
        noOfPlayersInRoom = PhotonNetwork.CurrentRoom.PlayerCount;
        if (matchType == "H" && PhotonNetwork.IsMasterClient) isHost = true;

        if (gameState == "Online")
        {
            WhiteClientID = PlayerPrefs.GetInt("WhitePlayerID", 0);
            BlackClientID = WhiteClientID == 1 ? 2 : 1;
            myClientID = PlayerPrefs.GetInt("MyClientID", 0);
            opponentGameID = myClientID == 1 ? 2 : 1;

            if (matchType != "H")
            {
                WhiteClientID--;                  //Dicrease white id to 0 - 1
                BlackClientID--;                  //Dicrease white id to 0 - 1
            }

            myPhotonPlayer = PhotonNetwork.LocalPlayer;
            if (noOfPlayersInRoom == 1) return;     //Dont go further while playing with bot

            allPlayersInRoom = PhotonNetwork.PlayerList;
            whitePhotonPlayer = allPlayersInRoom[WhiteClientID];
            blackPhotonPlayer = allPlayersInRoom[BlackClientID];

            WhitePlayerActorN = whitePhotonPlayer.ActorNumber;
            BlackPlayerActorN = blackPhotonPlayer.ActorNumber;

            //Host Only
            whitePlayerPieceId = int.Parse(allPlayersInRoom[WhiteClientID].CustomProperties["PieceID"].ToString());
            blackPlayerPieceId = int.Parse(allPlayersInRoom[BlackClientID].CustomProperties["PieceID"].ToString());

            if (myPhotonPlayer == whitePhotonPlayer) opPhotonPlayer = blackPhotonPlayer;
            else
                opPhotonPlayer = whitePhotonPlayer;
        }
    }
    //ingame id 1 / 2
    public int getOtherClientID(int clientActorN)
    {
        if (clientActorN == WhitePlayerActorN)
            return 1;
        else
            return 2;
    }

    private void Start()
    {
        isMatchEnd = false;
        playFabScript = PlayfabManager.instance;
        isGameDraw = false;


        if (myClientID == 2) Camera.main.GetComponent<Transform>().localEulerAngles = new Vector3(0, 0, 180);
        if (isHost)    //Hosting match owner
        {
            moveRecorder = GetComponent<IMoveRecorder>();
            GameUI_Script.setupAllPieces(whitePlayerPieceId, blackPlayerPieceId);
            StartCoroutine(RPC_startGame());
            return;
        }

        if (isRankedGame) {
            PlayerPrefs.SetInt("IsInMatch", 1);
            Debug.Log(InGameUI.instance.opRank);
            PlayerPrefs.SetInt("PrevOpRank", InGameUI.instance.opRank);

        }
        if (gameState == "Online")
        {
            myPlayer = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity).GetComponent<Player>();
            myPlayer.isLocked = true;
            if (noOfPlayersInRoom == 1)
            {
                opPlayer = PhotonNetwork.Instantiate("AIPlayer", Vector3.zero, Quaternion.identity);
                opPlayer.GetComponent<AIPlayer>().myId = opponentGameID;
                AIPlayer ai = opPlayer.GetComponent<AIPlayer>();
                botDif = UnityEngine.Random.Range(0, 3);
                if (botDif == 0)
                    ai.aIDepth = ai.easyDifficulty;
                else if (botDif == 1)
                    ai.aIDepth = ai.medDifficulty;
                else
                    ai.aIDepth = ai.hardDifficulty;
            }
            GameUI_Script.setupEnemyPieces(myClientID);
        }
        else if (gameState == "Offline")
        {
            myPlayer = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity).GetComponent<Player>();
            opPlayer = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);

            //009
            if (!isReplayMatch)
            {
                myClientID = UnityEngine.Random.Range(1, 3); //Asign Randome black or white
                SetupPlayreIDs();
            }
        }
        else if(gameState == "Puzzle")
        {
            aiDifficulty = PhotonNetwork.CurrentRoom.CustomProperties["aiLevel"].ToString();  //---------FINAL NEW LINES
            aiPlayerPiece = PhotonNetwork.CurrentRoom.CustomProperties["aiPlayer"].ToString();             //---------FINAL NEW LINES
            puzzlePlayer = PhotonNetwork.CurrentRoom.CustomProperties["puzzlePlayer"].ToString();     //---------FINAL NEW LINES
            puzzleOP = PhotonNetwork.CurrentRoom.CustomProperties["puzzleOp"].ToString();  //---------FINAL NEW LINES

            puzzleNo = PlayerPrefs.GetInt("PUZZLENO", 0);
            myClientID = PlayerPrefs.GetInt("PPIECES", 0) == 0 ? 1 : 2;
            int opId = PlayerPrefs.GetInt("PPIECES", 0) == 0 ? 2 : 1;

            if(puzzleNo == -1)
            {
                _puzzleData = JsonUtility.FromJson<puzzleData>(File.ReadAllText(Application.persistentDataPath + "/-1.txt"));
            }
            else
            {
                TextAsset levelFile = Resources.Load("Puzzles/" + puzzleNo.ToString()) as TextAsset;
                _puzzleData = JsonUtility.FromJson<puzzleData>(levelFile.text);
            }

            if (puzzleOP == "AI")
            {
                if (puzzlePlayer == "White")
                { //FINAL NEW IF STATEMENT
                    myPlayer = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity).GetComponent<Player>();
                    myPlayer.myId = myClientID;
                    opPlayer = PhotonNetwork.Instantiate("AIPlayer", Vector3.zero, Quaternion.identity);
                    opPlayer.GetComponent<AIPlayer>().myId = opId;
                    AIPlayer ai = opPlayer.GetComponent<AIPlayer>();
                    ai.aIDepth = ai.medDifficulty;
                }
                else
                {
                    myPlayer = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity).GetComponent<Player>();
                    myPlayer.myId = opId;
                    opPlayer = PhotonNetwork.Instantiate("AIPlayer", Vector3.zero, Quaternion.identity);
                    opPlayer.GetComponent<AIPlayer>().myId = myClientID;
                    AIPlayer ai = opPlayer.GetComponent<AIPlayer>();
                    ai.aIDepth = ai.medDifficulty;
                }
            }
            else
            {
                if (puzzlePlayer == "White")
                { //FINAL NEW IF STATEMENT
                    myPlayer = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity).GetComponent<Player>();
                    myPlayer.myId = myClientID;
                    opPlayer = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
                    opPlayer.GetComponent<Player>().myId = opId;
                }
                else
                {
                    myPlayer = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity).GetComponent<Player>();
                    myPlayer.myId = opId;
                    opPlayer = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
                    opPlayer.GetComponent<Player>().myId = myClientID;
                }
            }
            isTesingScenerio = true;
        }
        else
        {
            //
            // NEW CODE FOR THE Ai
           
            AIPlayer ai;                        //---------FINAL NEW LINES
            if (aiPlayerPiece == "White")        //FINAL NEW IF STATEMENT
            {
                myPlayer = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity).GetComponent<Player>();
                myPlayer.myId = 1;

                opPlayer = PhotonNetwork.Instantiate("AIPlayer", Vector3.zero, Quaternion.identity);
                opPlayer.GetComponent<AIPlayer>().myId = 2;
            }
            else
            {
                myPlayer = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity).GetComponent<Player>();
                myPlayer.myId = 2;

                opPlayer = PhotonNetwork.Instantiate("AIPlayer", Vector3.zero, Quaternion.identity);
                opPlayer.GetComponent<AIPlayer>().myId = 1;
            }
            ai = opPlayer.GetComponent<AIPlayer>();

            if (aiDifficulty == "Easy")
            {
                ai.aIDepth = ai.easyDifficulty;
            }
            else if (aiDifficulty == "Medium")
            {
                ai.aIDepth = ai.medDifficulty;
            }
            else
            {
                ai.aIDepth = ai.hardDifficulty;
            }
        }

        moveRecorder = GetComponent<IMoveRecorder>();
        StartCoroutine(RPC_startGame());
    }
    public void SetupPlayreIDs()
    {
        if (gameState == "Offline")
        {
            myPlayer.myId = myClientID;
            opPlayer.GetComponent<Player>().myId = myClientID == 1 ? 2 : 1;

            GameUI_Script.setupEnemyPieces(myClientID);

            if (myClientID == 2) Camera.main.GetComponent<Transform>().localEulerAngles = new Vector3(0, 0, 180);
        }
    }
    private void Update()
    {
        if (pieceBlinkTime > 0)
        {
            pieceBlinkTime -= Time.deltaTime;
            if (pieceBlinkTime < 0)
            {
                pieceBlinkTime = 0;
                //Blink Pieces here.
                foreach (var item in CapuredPieces)
                {
                    item.GetComponent<Animator>().SetBool("IsBlinking", true);
                }
            }
        }
    }

    public IEnumerator RPC_startGame()
    {
        yield return new WaitForSeconds(.1f);

        board = new CheckerData[BoardSize, BoardSize];

        StartCoroutine(spawnPieces());


        //yield return new WaitForSeconds(.18f);


    }

    IEnumerator spawnPieces()
    {
        if (isTesingScenerio == true)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                for (int x = 0; x < BoardSize; x++)
                {
                    GameObject newField = Instantiate(FieldPrefab, GetFieldPosition(x, y, true), Quaternion.identity);
                    newField.GetComponent<FieldData>().X = x;
                    newField.GetComponent<FieldData>().Y = y;
                }
            }

            foreach (Vec2 piecePos in _puzzleData.whitePieces) // Spawning White Pieces
            {
                GameObject checker = Instantiate(whiteChecker, GetFieldPosition(piecePos.x, piecePos.y, true), Quaternion.identity);
                CheckerData checkerData = checker.GetComponent<CheckerData>();
                RegiesterCheckerData(checkerData, new Vec2(piecePos.x, piecePos.y));
                checkerData.updatePosition(new Vec2(piecePos.x, piecePos.y), false);
                checkerData.clientID = myClientID;
                Board[piecePos.x, piecePos.y].ownerId = 1;

                if (canPromote(checkerData) && checkerData.IsKing == false)
                {
                    PromotePiece(checkerData);
                }
            }
            foreach (Vec2 piecePos in _puzzleData.blackPieces) // Spawning Black Pieces
            {
                GameObject checker = Instantiate(blackChecker, GetFieldPosition(piecePos.x, piecePos.y, true), Quaternion.identity);
                CheckerData checkerData = checker.GetComponent<CheckerData>();
                RegiesterCheckerData(checkerData, new Vec2(piecePos.x, piecePos.y));
                checkerData.updatePosition(new Vec2(piecePos.x, piecePos.y), false);
                checkerData.clientID = myClientID;
                Board[piecePos.x, piecePos.y].ownerId = 2;

                if (canPromote(checkerData) && checkerData.IsKing == false)
                {
                    PromotePiece(checkerData);
                }
            }
            foreach (Vec2 promPiece in _puzzleData.promotedPieces)
            {
                CheckerData promotedPiece = board[promPiece.x, promPiece.y];
                promotedPiece.IsKing = true;
                promotedPiece.updateKingSp();
            }
            UpdatePossbileMoves();
        }
        else
        {
            for (int y = 0; y < BoardSize; y++)
            {
                for (int x = 0; x < BoardSize; x++)
                {
                    GameObject newField = Instantiate(FieldPrefab, GetFieldPosition(x, y, true), Quaternion.identity);
                    newField.GetComponent<FieldData>().X = x;
                    newField.GetComponent<FieldData>().Y = y;

                    if ((x % 2 == 0 && y % 2 == 0) || (x % 2 == 1 && y % 2 == 1))
                    {
                        yield return new WaitForSeconds(0.01f);

                        if (y < NumCheckersRows)
                        {
                            GameObject checker = Instantiate(whiteChecker, GetFieldPosition(x, y, true), Quaternion.identity);
                            CheckerData checkerData = checker.GetComponent<CheckerData>();
                            checkerData.ownerId = 1;
                            checkerData.clientID = myClientID;
                            checkerData.updatePosition(new Vec2(x, y), false);
                            RegiesterCheckerData(checker.GetComponent<CheckerData>(), new Vec2(x, y));
                        }
                        else if (y >= BoardSize - NumCheckersRows)
                        {
                            GameObject checker = Instantiate(blackChecker, GetFieldPosition(x, y, true), Quaternion.identity);
                            CheckerData checkerData = checker.GetComponent<CheckerData>();
                            checkerData.ownerId = 2;
                            checkerData.clientID = myClientID;
                            checkerData.updatePosition(new Vec2(x, y), false);
                            RegiesterCheckerData(checker.GetComponent<CheckerData>(), new Vec2(x, y));
                        }
                    }
                }
            }

            UpdatePossbileMoves();

            //lastBoards.Add(copyCheckersData(board));
        }

        if (gameMode == "K1")
        {
            PromotePiece(board[2, 0]);
            PromotePiece(board[5, 7]);
        }
        else if (gameMode == "K2")
        {
            PromotePiece(board[4, 0]);
            PromotePiece(board[3, 7]);
        }
        else if (gameMode == "K3")
        {
            PromotePiece(board[2, 0]);
            PromotePiece(board[5, 7]);
            PromotePiece(board[4, 0]);
            PromotePiece(board[3, 7]);
        }

        isPlayingStarted = true;

        //Tell players to start playing
        Player[] players = FindObjectsOfType<Player>();
        foreach (Player player in players)
        {
            
            player.StartPlaying();
            if (winner!=-1)
                isReplayMatch = true;
        }
    }

    public void NextTurn()
    {
        isProcessingTurn = false;
        drawMoves++;
        drawMovesText.text = drawMoves.ToString();
        if (drawMoves >= 20)
        {
            StartCoroutine(matchEnd(0));
            return;
        }

        CurrentPlayerID = CurrentPlayerID == 1 ? 2 : 1;
        if (matchType != "H")
        {
            if (myPlayer.myId == CurrentPlayerID) myPlayer.isMyTurn();
            else
                if (puzzleOP == "Human" || gameState == "Offline") opPlayer.GetComponent<Player>().isMyTurn();
        }
        UpdatePossbileMoves();

       winner = CheckVictory();
       if (winner != -1)
       {
           StartCoroutine(matchEnd(winner));
           networkManager.PlayReplayMatch();
       }
    }
    public void UpdatePossbileMoves()
    {
        stopBlinkingPieces();
        boardPieces = copyBoard(board);
        PossibleMoves = GetPossibleMoves(board, BoardSize, CurrentPlayerID);

        clearCapturedDummies();
        clearHighlightPlaces();

        bool isCaptureAvailable = false;
        CapuredPieces.Clear();
        foreach (Move m in PossibleMoves)
        {
            List<Vec2> captured = GetCaptureList(m.From, board, BoardSize);
            if (captured.Count > 0)
            {
                CapuredPieces.Add(board[m.From.x, m.From.y].gameObject);
                isCaptureAvailable = true;
            }
        }
        if(isCaptureAvailable)
            pieceBlinkTime = 7;

        CountPieces();
    }
    private void stopBlinkingPieces()
    {
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                CheckerData chkr = board[x, y];
                if (chkr != null)
                    chkr.GetComponent<Animator>().SetBool("IsBlinking", false);
            }
        }
    }

    bool cleanMoves(Move move)
    {
        Move m = move;
        if (move.Children.Count > 0)
        {
            foreach (Move child in move.Children)
            {
                if(cleanMoves(child) == false)
                {
                    move.Children.Remove(child);
                }
            }
        }
        else
            return true;
        return true;
    }
    private void clearCapturedDummies()
    {
        foreach (GameObject place in capturedPiecesDummies)
        {
            Destroy(place);
        }
        capturedPiecesDummies.Clear();
    }
    public void clearHighlightPlaces()
    {
        foreach (GameObject place in movePlaces)
        {
            Destroy(place);
        }
        movePlaces.Clear();
    }
    public bool highlightCheckerMoves(CheckerData selectedChecker)
    {
        clearHighlightPlaces();

        Vec2 checkerPos = new Vec2((int)selectedChecker.position.x, (int)selectedChecker.position.y);
        for (int i = 0; i < PossibleMoves.Count; i++)
        {
            Move move = PossibleMoves[i];
            if (checkerPos.x == move.From.x && checkerPos.y == move.From.y)
            {
                Vector3 fieldPos = GetFieldPosition(move.To.x, move.To.y, false);
                GameObject movePlace;
                if (CurrentPlayerID == 1)
                {
                    movePlace = Instantiate(whiteHighlightPiece, fieldPos, Quaternion.identity);
                }
                else
                {
                    movePlace = Instantiate(blackHighlightPiece, fieldPos, Quaternion.identity);
                }
                movePlaces.Add(movePlace);
            }
        }

        if (movePlaces.Count == 0)
            return false;
        else
            return true;
    }

    //Compare request move with the valid moves in the list
    public Move IsMoveValid(Move selectedMove)
    {
        foreach(Move move in PossibleMoves)
        {
            if(selectedMove.Equals(move))
            {
                return move;
            }
        }
        return null;
    }
    public int moveID(Move selectedMove)
    {
        int id = 0;
        foreach (Move move in PossibleMoves)
        {
            if (selectedMove.Equals(move))
            {
                return id;
            }
            id++;
        }
        return 0;
    }
    public Vec2 GetCheckerFiled(CheckerData cheker)
    {
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                if (cheker == board[x, y])
                {
                    return new Vec2(x, y);
                }
            }
        }
        throw new Exception("Pionek poza stolem");
    }
    public CheckerData GetCheckerData(Vec2 position)
    {
        return board[position.x, position.y];
    }

    public void RegiesterCheckerData(CheckerData data, Vec2 position)
    {
        board[position.x, position.y] = data;
    }
    public static bool[,] CopyIsKingProperty(CheckerData[,] board)
    {
        Vec2 size = new Vec2(board.GetLength(0), board.GetLength(1));
        bool[,] copy = new bool[size.x, size.y];
        for(int y=0; y<size.y;y++)
        {
            for(int x=0; x<size.x;x++)
            {
                if(board[x,y])
                {
                    copy[x, y] = board[x, y].IsKing;
                }
            }
        }
        return copy;
    }
    public static void RestoreOrginalData(CheckerData[,] board, bool[,] copy)
    {
        Vec2 size = new Vec2(board.GetLength(0), board.GetLength(1));
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                if (board[x, y])
                {
                    board[x, y].IsKing = copy[x, y];
                }
            }
        }
    }
    public static List<Move> GetPossibleMoves(CheckerData[,] board, int size, int currentPlayer)
    {
        List<Move> ret = new List<Move>();
        
        bool[,] copy = CopyIsKingProperty(board);
        bool foundCaptureMove = false;
        int maxCombo = 0;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                CheckerData curr;
                int pathPoint = -1;
                if ((curr = board[x, y]) != null && curr.ownerId == currentPlayer)  
                {
                    GetInstance().boardPieces[x, y] = false;

                    //Save current checker data to check move target not placing on this checker place
                    GetInstance().capturedPieces.Clear();
                    Vec2 currPos = new Vec2(x, y);
                    List<Vec2> captured = GetCaptureList(currPos, board, size);
                    //Debug.Log(captured.Count);
                    if (captured.Count == 0 && foundCaptureMove == false) 
                    {
                        List<Vec2> nonKillingMoves = GetNonKillingMovesForField(currPos, board, size);
                        foreach (Vec2 target in nonKillingMoves)
                        {
                            ret.Add(new Move(currPos, target));
                        }
                    }
                    else
                    {
                        foundCaptureMove = true;
                        foreach (Vec2 target in captured)
                        {
                            GetInstance().capturedPieces.Clear();
                            Move move = new Move(currPos, target);

                            CheckerData[] lineCopy = CopyLine(board, currPos, target);
                            bool[,] kingsData = CopyIsKingProperty(board);

                            MoveSimulation(currPos, target, board);
                            int currCombo = SimulateCombo(target, board, size, 1, move);
                            RestoreLine(board, lineCopy, currPos, target);
                            RestoreOrginalData(board, kingsData);
                            if (currCombo > maxCombo)
                            {
                                ret.Clear();
                                maxCombo = currCombo;
                                ret.Add(move);
                                pathPoint++;
                            }
                            else if (currCombo == maxCombo)
                            {
                                ret.Add(move);
                                pathPoint++;
                            }
                        }
                    }

                    GetInstance().boardPieces[x, y] = true;
                }
            }
        }

        //po symulacji ruchu przywracamy oryginalne dane pionkom
        RestoreOrginalData(board, copy);
        return ret; 
    }
    private bool[,] copyBoard(CheckerData[,] board)
    {
        bool[,] b = new bool[BoardSize, BoardSize];
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                if (board[x, y] != null) b[x, y] = true;
            }
        }
        return b;
    }
    private bool isPieceOnTarget(Vec2 from, Vec2 to)
    {
        //Multiple opponent's pieces in way
        Vec2 direction = new Vec2((to.x - from.x) / Mathf.Abs(to.x - from.x), (to.y - from.y) / Mathf.Abs(to.y - from.y));
        int checkersAmount = 0;
        for (int i = 1; i < Mathf.Abs(to.y - from.y); i++)
        {
            if (boardPieces[from.x + i * direction.x, from.y + i * direction.y] != false)
            {
                checkersAmount++;
                if (checkersAmount > 1) return true;
            }
        }

        //check if captured, other piece on target
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                CheckerData curr;
                if ((curr = board[x, y]) != null && curr.ownerId != CurrentPlayerID)
                {
                    if (to.x == x && to.y == y)
                    {
                        Debug.Log("Piece on target" + x + y);
                        return true;
                    }
                }
            }
        }

        //check if move target is on a capture piece place.
        foreach (Vec2 capturedPiece in capturedPieces)
        {
            if (capturedPiece.x == to.x && capturedPiece.y == to.y)
            {
                Debug.Log("Cap piece on target" + to.x + to.y);
                return true;
            }
        }
        return false;
    }
    public static CheckerData[] CopyLine(CheckerData[,] board, Vec2 from, Vec2 to)
    {
        CheckerData[] data = new CheckerData[Mathf.Abs(from.y - to.y) + 1];
        Vec2 dir = new Vec2((to.x - from.x) / Mathf.Abs(to.x - from.x),
            (to.y - from.y) / Mathf.Abs(to.y - from.y));
        for (int mul = 0; mul < data.GetLength(0); mul++)
        {
            data[mul] = board[from.x + dir.x * mul, from.y + dir.y * mul];
        }
        return data;
    }
    public static void RestoreLine(CheckerData[,] board, CheckerData[] copy, Vec2 from, Vec2 to)
    {
        Vec2 dir = new Vec2((to.x - from.x) / Mathf.Abs(to.x - from.x),
            (to.y - from.y) / Mathf.Abs(to.y - from.y));
        for (int mul = 0; mul < copy.GetLength(0); mul++)
        {
            board[from.x + dir.x * mul, from.y + dir.y * mul] = copy[mul];
        }
    }
    static int SimulateCombo(Vec2 position,CheckerData[,] board, int size, int counter, Move move)
    {
        int insideCounter = 1;
        List<Vec2> captured = GetCaptureList(position, board, size);
        if (captured.Count == 0)
        {
            move.Children = null;
            return counter;
        }
        else
        {
            move.Children = new List<Move>();
            foreach(Vec2 target in captured)
            {
                Move child = new Move(position, target);
                CheckerData[] lineCopy = CopyLine(board, position, target);
                bool[,] kingsData = CopyIsKingProperty(board);
                MoveSimulation(position, target, board);
                int newComboCounter = SimulateCombo(target, board, size, counter + 1, child);
                if (newComboCounter > insideCounter)
                {
                    move.Children.Clear();
                    move.Children.Add(child);
                    insideCounter = newComboCounter;
                }
                else if(newComboCounter==insideCounter)
                {
                    move.Children.Add(child);
                }
                RestoreLine(board, lineCopy, position, target);
                RestoreOrginalData(board, kingsData);
            }
        }



        return insideCounter;
    }
    static List<Vec2> GetNonKillingMovesForField(Vec2 field, CheckerData[,] board, int size)
    {
        List<Vec2> ret = new List<Vec2>();

        CheckerData checker = board[field.x, field.y];
        
        if(checker.IsKing == false)
        {
            Vec2 target = new Vec2(field.x + 1, field.y + checker.direction);
            if (target.x >= 0 && target.x < size && target.y >= 0 && target.y < size && board[target.x, target.y] == null)
            {
                ret.Add(target);
            }
            target = new Vec2(field.x - 1, field.y + checker.direction);
            if (target.x >= 0 && target.x < size && target.y >= 0 && target.y < size && board[target.x, target.y] == null)
            {
                ret.Add(target);
            }
        }
        else
        {
            bool leftFinished = false, rightFinished = false;   
                                                                
            for (int y = field.y + 1; y < size; y++)
            {
                int distance = Mathf.Abs(field.y - y);

                if (!leftFinished && field.x + distance < size && board[field.x + distance, y] == null)
                {
                    ret.Add(new Vec2(field.x + distance, y));
                }
                else
                {
                    leftFinished = true;
                }

                if (!rightFinished && field.x - distance >= 0 && board[field.x - distance, y] == null)
                {
                    ret.Add(new Vec2(field.x - distance, y));
                }
                else
                {
                    rightFinished = true;
                }
            }

            leftFinished = false;
            rightFinished = false;
            for (int y = field.y - 1; y >= 0; y--)
            {
                int distance = Mathf.Abs(field.y - y);

                if (!leftFinished && field.x + distance < size && board[field.x + distance, y] == null)
                {
                    ret.Add(new Vec2(field.x + distance, y));
                }
                else
                {
                    leftFinished = true;
                }

                if (!rightFinished && field.x - distance >= 0 && board[field.x - distance, y] == null)
                {
                    ret.Add(new Vec2(field.x - distance, y));
                }
                else
                {
                    rightFinished = true;
                }
            }
        }

        
        return ret;
    }
    static List<Vec2> GetCaptureList(Vec2 field, CheckerData[,] board, int size)
    {
        List<Vec2> ret = new List<Vec2>();

        CheckerData checker = board[field.x, field.y];
        if (checker.IsKing == false)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    Vec2 target = new Vec2(field.x + x, field.y + y);
                    Vec2 behind = new Vec2(field.x + 2 * x, field.y + 2 * y);
                    if (behind.x >= 0 && behind.x < size && behind.y >= 0 && behind.y < size)
                    {
                        CheckerData targetChecker = board[target.x, target.y];

                    }
                }
            }
        }
        else
        {

            System.Action<CheckerData[,], Vec2, Vec2> directionIterator =
                (data, startPoint, direction) =>
                {

                    for (int y = startPoint.y + direction.y; y < size - 1 && y > 0; y += direction.y)
                    {
                        int targetX = startPoint.x + direction.x * Mathf.Abs(startPoint.y - y);



                    }
                };
        }

        return ret;
    }
    public static void MoveSimulation(Vec2 from, Vec2 to, CheckerData[,] board, Action<CheckerData, Vec2, bool> moveAction = null, 
        Action<CheckerData> destroyAction = null, Action<CheckerData, CheckerData, Vec2, Vec2, Vec2, int> recordMove = null)
    {
        CheckerData selected = board[from.x, from.y];
        Vec2 direction = new Vec2((to.x - from.x) / Mathf.Abs(to.x - from.x), (to.y - from.y) / Mathf.Abs(to.y - from.y));
        CheckerData killed = null;
        Vec2 killPos = null;

        Debug.Assert(selected != null);
        if (selected.IsKing == false)
        {
            if (Mathf.Abs(to.y - from.y) > 1)
            {
                killPos =  new Vec2(from.x + direction.x, from.y + direction.y);
                killed = board[killPos.x, killPos.y];
                GetInstance().capturedPieces.Add(GetInstance().GetCheckerFiled(killed));
                GetInstance().drawMoves = -1;
                board[from.x + direction.x, from.y + direction.y] = null;
                if (destroyAction != null)
                {
                    destroyAction(killed);
                    Vector3 fieldPos = GetInstance().GetFieldPosition(from.x + direction.x, from.y + direction.y, false);
                    GameObject movePlace;   //Captured Piece Dummy
                    if (GetInstance().CurrentPlayerID == 1)
                        movePlace = Instantiate(GetInstance().blackHighlightPiece, fieldPos, Quaternion.identity);
                    else
                        movePlace = Instantiate(GetInstance().whiteHighlightPiece, fieldPos, Quaternion.identity);

                    GetInstance().capturedPiecesDummies.Add(movePlace);
                    AudioManager.instance.capturePiece();
                }
            }
        }
        else
        {
            for (int i = 1; i < Mathf.Abs(to.y - from.y); i++)
            {
                killPos = new Vec2(from.x + i * direction.x, from.y + i * direction.y);
                if ((killed = board[killPos.x, killPos.y]) != null && killed.ownerId != selected.ownerId)
                {
                    GetInstance().capturedPieces.Add(GetInstance().GetCheckerFiled(killed));
                    GetInstance().drawMoves = -1;
                    board[from.x + i * direction.x, from.y + i * direction.y] = null;
                    if (destroyAction != null)
                    {
                        destroyAction(killed);
                        Vector3 fieldPos = GetInstance().GetFieldPosition(from.x + i * direction.x, from.y + i * direction.y, false);
                        GameObject movePlace;   //Captured Piece Dummy
                        if (GetInstance().CurrentPlayerID == 1)
                            movePlace = Instantiate(GetInstance().blackHighlightPiece, fieldPos, Quaternion.identity);
                        else
                            movePlace = Instantiate(GetInstance().whiteHighlightPiece, fieldPos, Quaternion.identity);

                        GetInstance().capturedPiecesDummies.Add(movePlace);
                        AudioManager.instance.capturePiece();
                    }
                    break;
                }
            }
        }

        if (recordMove != null)
            recordMove(selected, killed, from, to, killPos, GetInstance().CurrentPlayerID);

        board[to.x, to.y] = selected;
        board[from.x, from.y] = null;
        if (moveAction != null)
        {
            moveAction(board[to.x, to.y], to, false);
        }
    }

    public bool canPromote(CheckerData selected)
    {
        Vec2 checkerField = GetCheckerFiled(selected);
        if ((checkerField.y == 0 && selected.direction == -1) || (checkerField.y == board.GetUpperBound(0) && selected.direction == 1))
        {
            return true;
        }
        else
            return false;
    }
    private void PromotePiece(CheckerData piece)
    {
        piece.IsKing = true;
        piece.updateKingSp();
    }
    public bool MoveChecker(Move move)
    {
        MoveSimulation(move.From, move.To, board, MoveChecker, DestroyChecker, moveRecorder.IRecordMove);
        UpdateMoveDisplay(move); //Show move text 

        AudioManager.instance.movePiece();
        if (move.Children == null || move.Children.Count == 0)
        {
            capturedPieces.Clear();
            moveHighlighter.PieceMoved(CurrentPlayerID, move.From, move.To, true);
            return false;
        }
        else
        {
            PossibleMoves = move.Children;
            moveHighlighter.PieceMoved(CurrentPlayerID, move.From, move.To, false);
            return true;
        }
    }
    public void UpdateMoveDisplay(Move move)
    {
        //Display move position 
        int moveStartId = 0;
        int moveLandId = 0;
        Vector2 moveFrom = new Vector2(move.From.x, move.From.y);
        Vector2 moveTo = new Vector2(move.To.x, move.To.y);
        if (movesPositionsIds.ContainsKey(moveFrom))
            moveStartId = movesPositionsIds[moveFrom];
        if (movesPositionsIds.ContainsKey(moveTo))
            moveLandId = movesPositionsIds[moveTo];
        lastMoveText.text = moveStartId + "-" + moveLandId;
    }
    private void CountPieces()
    {
        int whitePieces = 0;
        int blackPieces = 0;
        bool[,] b = new bool[BoardSize, BoardSize];
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                if (board[x, y] != null)
                {
                    if (board[x, y].ownerId == 1)
                        whitePieces++;
                    else
                        blackPieces++;
                }
            }
        }
        whitePiecesText.text = whitePieces.ToString();
        blackPiecesText.text = blackPieces.ToString();
    }
    private void DestroyChecker(CheckerData checker)
    {
        Destroy(checker.gameObject);
    }
    public void MoveChecker(CheckerData checker, Vec2 target, bool instantMove)
    {
        checker.targetPosition = GetFieldPosition(target.x, target.y, true);
        checker.updatePosition(new Vec2(target.x, target.y), instantMove);
        checker.isMoving = true;
    }
    public Vector3 GetFieldPosition(int x, int y, bool isChecker)
    {
        SpriteRenderer renderer = FieldPrefab.GetComponent<SpriteRenderer>();
        float fieldSize = renderer.sprite.bounds.size.x * FieldPrefab.transform.localScale.x;
        float startFieldPosition = -(fieldSize * BoardSize / 2);
        return new Vector3(startFieldPosition + x * fieldSize, startFieldPosition + y * fieldSize, (isChecker ? -1.0f : 0.0f));
    }
    public int CheckVictory()
    {
        int winner;
        if (gameMode == "D")
        {
            int[] numPCheckers = { 0, 0 };
            foreach (CheckerData checker in Board)
            {
                if (checker)
                {
                    numPCheckers[checker.ownerId == 1 ? 0 : 1]++;
                }
            }
            winner = numPCheckers[0] == 0 ? 2 : (numPCheckers[1] == 0 ? 1 : -1);
        }
        else
        {

            int[] numPCheckers = { 0, 0 };
            foreach (CheckerData checker in Board)
            {
                if (checker)
                {
                    numPCheckers[checker.ownerId == 1 ? 0 : 1]++;
                }
            }
            winner =  numPCheckers[0] == 0 ? 1 : (numPCheckers[1] == 0 ? 2 : -1);
            Debug.Log(winner);
        }

        if (winner == -1)
        {
            if (PossibleMoves.Count == 0) return CurrentPlayerID == 1 ? 2 : 1;
            else return winner;
        }
        else return winner;
    }
    public void HighlightChecker(CheckerData toHighlight, CheckerData previouslyHighlited = null)
    {
        if (previouslyHighlited)
        {
            previouslyHighlited.transform.GetChild(0).gameObject.SetActive(false);
        }
        if (toHighlight)
        {
            toHighlight.transform.GetChild(0).gameObject.SetActive(true);
        }
    }
    public void gameWin(int leavingPlayerID = 0)
    {
        if (isGameDraw) return;
        if (isMatchEnd) return;
        PlayerPrefs.SetInt("IsInMatch", 0);
        isMatchEnd = true;
        int rkPoints = 0;
        if (isRankedGame)
        {
            //Calculate Win rank points
            //rkPoints = PlayfabManager.instance.rankPoints += 10;

            rkPoints += CalculateRankPoints(PlayfabManager.instance.rankPoints, InGameUI.instance.opRank, "Win");
            PlayfabManager.instance.rankPoints = rkPoints;

            PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    {
                        "RankPoints", rkPoints.ToString()
                    }
                }
            },
            respone =>
            {
                PlayfabCustomRequests.AddVirtualCurrency(CoinsReward.match_Win_coins, OnCoinsAdded, OnError);
            },
            error =>
            {

            });
        }
        else
        {
            if(gameState == "Puzzle")
            {
                solvedPuzzles dl = playFabScript.solved_puzzles;
                bool isNewLevel = true;
                foreach (var id in dl.puzzleIds)
                {
                    if (id == puzzleNo) isNewLevel = false;
                }
                if (PlayerPrefs.GetInt("INSTANTPUZZLE", 0) == 1) isNewLevel = false;
                if (isNewLevel)
                {
                    dl.puzzleIds.Add(puzzleNo);
                    string puzzleData = JsonUtility.ToJson(dl);
                    PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
                    {
                        Data = new Dictionary<string, string>()
                        {
                            {
                                "DonePuzzles", puzzleData
                            }
                        }

                    }, response =>
                    {
                        PlayfabCustomRequests.AddVirtualCurrency(CoinsReward.puzzle_Solve_coins, OnCoinsAdded, OnError);
                    }, error =>
                    {
                    }); ;
                }
                else
                {
                    AudioManager.instance.victory();
                    PhotonNetwork.LeaveRoom();
                    WinPanel.SetActive(true);
                    WinPanel.GetComponent<GameEndView>().updateOpponentName(GameUI_Script.opponentPlayerName);
                }
            }
            else
            {
                AudioManager.instance.victory();
                PhotonNetwork.LeaveRoom();
                offlinePanel.SetActive(true);
                if(leavingPlayerID != 0)
                {
                    int winnter = leavingPlayerID == 1 ? 2 : 1;
                    offlineMatchEndtext.text = winnter == 1 ? "LIGHT PLAYER WINS!" : "DARK PLAYER WINS!";
                }
                else
                    offlineMatchEndtext.text = myClientID == 1 ? "LIGHT PLAYER WINS!" : "DARK PLAYER WINS!";
            }

        }

    }
    public async void gameLost()
    {
        if (isGameDraw) return;
        if (isMatchEnd) return;
        PlayerPrefs.SetInt("IsInMatch", 0);
        isMatchEnd = true;
        Debug.Log("Lost");
        int rkPoints = 0;
        if (isRankedGame)
        {
            if(PlayfabManager.instance.rankPoints > 200)
            {
                //rkPoints = PlayfabManager.instance.rankPoints -= 10;

                rkPoints = CalculateRankPoints(PlayfabManager.instance.rankPoints, InGameUI.instance.opRank, "Lost");
                Debug.Log(rkPoints);
                PlayfabManager.instance.rankPoints = rkPoints;

                PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
                {
                    Data = new Dictionary<string, string>
                    {
                        {
                            "RankPoints", rkPoints.ToString()
                        }
                    }
                },
                async respone =>
                {
                    await PlayfabCustomRequests.AddVirtualCurrency(CoinsReward.match_Lost_coins, OnCoinsAdded, OnError);
                },
                error =>
                {

                });
            }
            else
            {
                await PlayfabCustomRequests.AddVirtualCurrency(CoinsReward.match_Lost_coins, OnCoinsAdded, OnError);
            }
        }
        else
        {
            AudioManager.instance.defeat();
            PhotonNetwork.LeaveRoom();
            offlinePanel.SetActive(true);
            offlineMatchEndtext.text = (myPlayer.myId == 1) ? "DARK PLAYER WINS!" : "LIGHT PLAYER WINS!";
        }
    }
    public async void drawGame()
    {
        if (isMatchEnd) return;
        PlayerPrefs.SetInt("IsInMatch", 0);
        isMatchEnd = true;
        int rkPoints = 0;

        if (isRankedGame)
        {
            //await Task.Delay(0);

            //PlayFabClientAPI.AddUserVirtualCurrency(new AddUserVirtualCurrencyRequest
            //{
            //    VirtualCurrency = "CN",
            //    Amount = CoinsReward.match_Lost_coins
            //},
            //succes =>
            //{

            //},
            //error =>
            //{

            //});


            if (PlayfabManager.instance.rankPoints > 200)
            {
                //rkPoints = PlayfabManager.instance.rankPoints -= 10;

                rkPoints = CalculateRankPoints(PlayfabManager.instance.rankPoints, InGameUI.instance.opRank, "Draw");

                PlayfabManager.instance.rankPoints = rkPoints;

                PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
                {
                    Data = new Dictionary<string, string>
                    {
                        {
                            "RankPoints", rkPoints.ToString()
                        }
                    }
                },
                async respone =>
                {
                    await PlayfabCustomRequests.AddVirtualCurrency(CoinsReward.match_Lost_coins, succes =>
                    {
                        PlayfabManager.instance.coins = succes.Balance;
                        AudioManager.instance.victory();
                        PhotonNetwork.LeaveRoom();
                        drawPanel.GetComponent<GameEndView>().updateOpponentName(GameUI_Script.opponentPlayerName);
                        drawPanel.SetActive(true);
                    }, OnError);
                },
                error =>
                {

                });
            }
            else
            {
                await PlayfabCustomRequests.AddVirtualCurrency(CoinsReward.match_Lost_coins, succes =>
                {
                    PlayfabManager.instance.coins = succes.Balance;
                    AudioManager.instance.victory();
                    PhotonNetwork.LeaveRoom();
                    drawPanel.GetComponent<GameEndView>().updateOpponentName(GameUI_Script.opponentPlayerName);
                    drawPanel.SetActive(true);
                }, OnError);
            }
        }
        else
        {
            AudioManager.instance.victory();
            PhotonNetwork.LeaveRoom();
            offlinePanel.SetActive(true);
            offlineMatchEndtext.text = "DRAW";
        }

    }
    public static int CalculateRankPoints(int R1, int R2, string GameResult)
    {
        float newRating = 0;
        float E = 0;
        float S = 0;

        if (GameResult == "Win")
            S = 1;
        else if (GameResult == "Draw")
            S = 0.5f;

        E = 1 / (1 + Mathf.Pow(10, (R2 - R1) / 400));

        newRating = R1 + (32 * (S - E));

        Debug.Log(newRating);

        return Mathf.RoundToInt(newRating);
    }
    public IEnumerator matchEnd(int winnerID)
    {
        //yield break;
        if (isMatchEnd) yield break;

        AudioManager.instance.matchEnd();

        //008
        //Saving Match if not a rematch

        //GameEnd?.Invoke();
        if (winnerID == 0)
        {
            //Debug.Log("DrawGame");
            if (!isReplayMatch) UndoSystem.Instance.SaveMatch(gameState, "Draw");

            isGameDraw = true;
            yield return new WaitForSeconds(3);
            drawGame();
        }
        else if (winnerID == myPlayer.myId)
        {
            if (!isReplayMatch) UndoSystem.Instance.SaveMatch(gameState, "Win");

            yield return new WaitForSeconds(3);
            gameWin();
        }
        else
        {
            if (!isReplayMatch) UndoSystem.Instance.SaveMatch(gameState, "Lose");

            yield return new WaitForSeconds(3);
            gameLost();
        }
    }
    public void leaveMatch()
    {
        //008
        //Save Match Before Leaving
        if (!isReplayMatch) UndoSystem.Instance.SaveMatch(gameState, "Lose");

        if (matchType == "H")
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == 1) { PhotonNetwork.LeaveRoom(); return; }
        }
        myPlayer.leaveMatch();
    }
    private void OnCoinsAdded(ModifyUserVirtualCurrencyResult obj)
    {
        PlayfabManager.instance.coins = obj.Balance;

        if (obj.BalanceChange == 5)
        {
            AudioManager.instance.defeat();
            LostPanel.SetActive(true);
            LostPanel.GetComponent<GameEndView>().updateOpponentName(GameUI_Script.opponentPlayerName);
        }
        else
        {
            AudioManager.instance.victory();
            WinPanel.SetActive(true);
            WinPanel.GetComponent<GameEndView>().updateOpponentName(GameUI_Script.opponentPlayerName);
        }
        PhotonNetwork.LeaveRoom();
    }
    private void OnError(PlayFabError obj)
    {
        Debug.Log(obj.ErrorMessage);
    }
}
