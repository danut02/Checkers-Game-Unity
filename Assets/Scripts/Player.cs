using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;


public class RpcTurn
{
    public int moveIndex;
    public string lastPlayerTurn;
    public string turnTime;
    public int nextTurn;
}

public class Player : MonoBehaviourPunCallbacks
{
    enum PlayerState
    {
        SelectingChecker,
        SelectingField
    }

    public GameModel gameModelScript;

    private PlayerState currentSate = PlayerState.SelectingChecker;
    public int myId;
    public bool inTurn;
    private bool isMine;
    public int points;
    [HideInInspector]public bool isLocked = false;
    string lastTurnTimer;

    private bool isGameEnd;
    public bool isPlaying = false;
    [HideInInspector] public bool isPieceCaptured;
    private CheckerData selectedChecker;
    public CheckerData SelectedChecker
    {
        get
        {
            return selectedChecker;
        }
        set
        {
            gameModelScript.HighlightChecker(value, selectedChecker);
            selectedChecker = value;
        }
    }

    private RpcTurn firstTurn = null;

    private void Start()
    {
        gameModelScript = GameModel.GetInstance();
        isMine = photonView.IsMine;
        string gameState = PhotonNetwork.CurrentRoom.CustomProperties["GameState"].ToString();
        if (gameState == "Online")
        {
            if (isMine)
            {
                gameModelScript.myPlayer = this;
                myId = gameModelScript.myClientID;
            }
            else
                myId = gameModelScript.getOtherClientID(photonView.OwnerActorNr);
        }

        //008
        if (PlayerPrefs.GetInt("RE", 0) == 1)
        {
            isLocked = true;
        }

        isPlaying = gameModelScript.isPlayingStarted;
    }

    private void Update()
    {
        if (!isMine || gameModelScript.CurrentPlayerID != myId || isLocked) return;

        //Get out if not own
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 50.0f))
            {
                if (currentSate == PlayerState.SelectingChecker)
                {
                    StateSelectingChecker(hit);
                }
                else if (currentSate == PlayerState.SelectingField)
                {
                    StateSelectingFiled(hit);
                }
            }
            else
            {
                if (isPieceCaptured) return;
                SelectedChecker = null;
                currentSate = PlayerState.SelectingChecker;
                gameModelScript.clearHighlightPlaces(); //remove highlighted places
            }
        }
    }

    private void StateSelectingChecker(RaycastHit hit)
    {
        if (hit.collider.tag == GameModel.CheckerTag)
        {
            if (hit.collider.GetComponent<CheckerData>().ownerId != myId) return;

            CheckerData checker = hit.collider.GetComponent<CheckerData>();

            if (gameModelScript.highlightCheckerMoves(checker) == true)
            {
                SelectedChecker = checker;
                currentSate = PlayerState.SelectingField;
            }
            else
                checker.shakeChecker();
        }
    }
    private void StateSelectingFiled(RaycastHit hit)
    {
        if (hit.collider.tag == GameModel.FieldTag)
        {
            processTurn(hit.transform.gameObject);
        }
    }
    public void isMyTurn()
    {
        isPieceCaptured = false;
    }
    //Actions Callbacks
    private void onGameEnd()
    {
        isGameEnd = true;
    }
    public void StartPlaying()
    {
        Debug.Log("Playing started");
        isPlaying = true;

        if(firstTurn != null)
        {
            RPC_UpdateOtherClientTurn(firstTurn.moveIndex, firstTurn.lastPlayerTurn, firstTurn.turnTime, firstTurn.nextTurn);
        }

    }
    //Callbacks Multiplayer
    public void processTurn(GameObject field = null, Move Move = null)
    {
        //008
        //Edited
        Move move;

        if(field == null && Move != null)
            move = Move;
        else
        {
            FieldData fieldData = field.GetComponent<FieldData>();
            move = new Move(gameModelScript.GetCheckerFiled(SelectedChecker), new Vec2(fieldData.X, fieldData.Y));
        }
        //End

        int moveIndex = gameModelScript.moveID(move);

        if ((move = gameModelScript.IsMoveValid(move)) != null)
        {
            gameModelScript.isProcessingTurn = true;
            if (false == gameModelScript.MoveChecker(move))
            {
                if (gameModelScript.canPromote(SelectedChecker) && selectedChecker.IsKing == false)
                {
                    //checkerData.updateKingSp();
                    SelectedChecker.IsKing = true;
                    SelectedChecker.updateKingSp();
                    gameModelScript.drawMoves = -1;
                    AudioManager.instance.promotePiece();
                }

                SelectedChecker = null;
                currentSate = PlayerState.SelectingChecker;

                gameModelScript.NextTurn();
                lastTurnTimer = MatchTimer.instance._crntPlayerTimer();
                MatchTimer.instance.tapClock(lastTurnTimer, PhotonNetwork.Time.ToString());
            }
            else
            {
                isPieceCaptured = true;
                highlightPieces();
            }
            gameModelScript.turnNo++;
            photonView.RPC("RPC_UpdateOtherClientTurn", RpcTarget.OthersBuffered, moveIndex, lastTurnTimer, PhotonNetwork.Time.ToString(), gameModelScript.turnNo);
        }
        else
        {
            //when click on a wronge plac
            //Response back to player
            invalidMove();
        }
    }
    //New
    public void leaveMatch()
    {
        photonView.RPC("leaveMatch_Rpc", RpcTarget.All);
    }
    //New
    [PunRPC]
    public void leaveMatch_Rpc()
    {
        if (photonView.IsMine)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
            gameModelScript.gameWin(myId);
    }
    [PunRPC]
    public void RPC_UpdateOtherClientTurn(int moveIndex, string lastPlayerTurn, string turnTime, int nextTurn)
    {
        if(isPlaying == false)
        {
            firstTurn = new RpcTurn();

            firstTurn.moveIndex = moveIndex;
            firstTurn.lastPlayerTurn = lastPlayerTurn;
            firstTurn.turnTime = turnTime;
            firstTurn.nextTurn = nextTurn;

            return;
        }

        if (gameModelScript == null)
        {
            gameModelScript = GameModel.GetInstance();
            myId = gameModelScript.getOtherClientID(photonView.OwnerActorNr);
        }

        if (nextTurn != gameModelScript.turnNo + 1) return;
        gameModelScript.turnNo++;

        if (gameModelScript.CurrentPlayerID != myId) return;

        Move move = gameModelScript.PossibleMoves[moveIndex];
        CheckerData checker = gameModelScript.board[move.From.x, move.From.y];

        if (false == gameModelScript.MoveChecker(move))
        {
            if (gameModelScript.canPromote(checker) && checker.IsKing == false)
            {
                //checkerData.updateKingSp();
                checker.IsKing = true;
                checker.updateKingSp();
                gameModelScript.drawMoves = -1;
                AudioManager.instance.promotePiece();
            }

            gameModelScript.NextTurn();
            MatchTimer.instance.tapClock(lastPlayerTurn, turnTime);
        }
    }
    public void highlightPieces()
    {
        gameModelScript.highlightCheckerMoves(SelectedChecker);
    }
    public void invalidMove()
    {
        //Debug.Log("Invalid Move");
        if (isPieceCaptured) return;
        SelectedChecker = null;
        currentSate = PlayerState.SelectingChecker;
        gameModelScript.clearHighlightPlaces();
    }
}