using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using System.Threading.Tasks;

[System.Serializable]
public class SavedMatch
{
    //public List<RecordMove> Moves { get; set; } = new List<RecordMove>();
    [SerializeField] public string MatchName = "Oponnent Name";
    [SerializeField] public int MyID = 1;
    [SerializeField] public string MatchType = "Casual";
    [SerializeField] public string Result = "Lose";
    [SerializeField] public List<Move> allMoves = new List<Move>();
}

public class UndoSystem : MonoBehaviour, IMoveRecorder
{
    public static UndoSystem Instance;

    SavedMatch SavingMatchData = new SavedMatch();

    [SerializeField] private MatchReplayManager replayManager;

    [SerializeField] private MovesRepresentator moveHighlighter;

    [HideInInspector] public List<RecordMove> PreviousMoves = new List<RecordMove>();
    CheckerData[,] currentBoard;
    GameModel gameModelScript;
    string gameState = string.Empty;
    int lastMovePlayerID;

    private bool IsMatchSaved = false;


    //008
    bool IsReplayMatch;

    private void Awake()
    {
        Instance = this;
    }
    private void OnDestroy()
    {
        Instance = null;
    }

    void Start()
    {
        //Check if replaying match
        if (PlayerPrefs.GetInt("RE", 0) == 1) IsReplayMatch = true;

        gameModelScript = GameModel.GetInstance();
        gameState = gameModelScript.gameState;
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.S)) SaveMatch();
    }
    public bool undoTurn()
    {
        if (PreviousMoves.Count <= 0) return false;
        if (!IsReplayMatch && gameModelScript.isProcessingTurn) return false;

        lastMovePlayerID = PreviousMoves[PreviousMoves.Count - 1].PlayerID;
        currentBoard = gameModelScript.Board;


        if (IsReplayMatch) //If replay match undo only one last move
            UndoMove();
        else
        {
            while (PreviousMoves[PreviousMoves.Count - 1].PlayerID == lastMovePlayerID)
            {
                UndoMove();
                if (PreviousMoves.Count <= 0) break;
            }
        }

        if (gameState == "Offline")
        {
            if (lastMovePlayerID == 1) gameModelScript.myPlayer.isPieceCaptured = false;
            else gameModelScript.opPlayer.GetComponent<Player>().isPieceCaptured = false;
        }
        else
            if (lastMovePlayerID == 1) gameModelScript.myPlayer.isPieceCaptured = false;

        gameModelScript.UpdatePossbileMoves();

        int prevMoveRemaining = PreviousMoves.Count;
        //Highlight previous moves
        if(prevMoveRemaining > 0)
        {
            lastMovePlayerID = PreviousMoves[prevMoveRemaining - 1].PlayerID;
            List<RecordMove> prevRecMoves = new List<RecordMove>();
            while (PreviousMoves[prevMoveRemaining - 1].PlayerID == lastMovePlayerID)
            {
                RecordMove lastMove = PreviousMoves[prevMoveRemaining - 1];
                prevRecMoves.Add(lastMove);
                prevMoveRemaining--;
                if (prevMoveRemaining <= 0) break;
            }
            prevRecMoves.Reverse();
            int index = 0;
            while (index < prevRecMoves.Count)
            {
                RecordMove move = prevRecMoves[index];
                if(index + 1 == prevRecMoves.Count)
                    moveHighlighter.PieceMoved(move.PlayerID, move.From, move.To, true);
                else
                    moveHighlighter.PieceMoved(move.PlayerID, move.From, move.To, false);
                index++;
            }
            gameModelScript.UpdateMoveDisplay(new Move(prevRecMoves[index-1].From, prevRecMoves[index-1].To));
        }
        else
            moveHighlighter.EnqueAllMarks();

        return true;
    }
    //008
    private void UndoMove()
    {
        RecordMove lastMove = PreviousMoves[PreviousMoves.Count - 1];
        CheckerData previousMovedChecker = currentBoard[lastMove.To.x, lastMove.To.y]; //Swaping checker data
        currentBoard[lastMove.From.x, lastMove.From.y] = previousMovedChecker;
        currentBoard[lastMove.To.x, lastMove.To.y] = null;
        gameModelScript.MoveChecker(previousMovedChecker, new Vec2(lastMove.From.x, lastMove.From.y), true);

        if (!lastMove.movedCheckerInfo.IsKing)
        {
            previousMovedChecker.IsKing = false;
            previousMovedChecker.kingSprite.SetActive(false);
        }
        if (lastMove.capturedPieceData != null) respawnCapturedPiece(lastMove.capturedPieceData, lastMove.capturePos);
        gameModelScript.CurrentPlayerID = lastMove.PlayerID;
        PreviousMoves.RemoveAt(PreviousMoves.Count - 1);
    }

    public void OnClickUndo()
    {
        if (gameState == "Offline")
        {
            undoTurn(); //One trun undo
        }
        else    // Undo 2 turns
        {
            undoTurn();
            undoTurn();
        }
    }
    private void respawnCapturedPiece(CheckerInfo capturedData, Vec2 pos)
    {
        if (pos.x == 0 && pos.y == 0) return;
        GameObject spawnChecker;
        if (capturedData.ownerId == 1)
            spawnChecker = Instantiate(gameModelScript.whiteChecker);
        else
            spawnChecker = Instantiate(gameModelScript.blackChecker);

        CheckerData newCheckerData = spawnChecker.GetComponent<CheckerData>();
        gameModelScript.MoveChecker(newCheckerData, pos, true);
        newCheckerData.ownerId = capturedData.ownerId;
        newCheckerData.clientID = capturedData.clientID;
        newCheckerData.IsKing = capturedData.IsKing;
        newCheckerData.updateKingSp();
        gameModelScript.RegiesterCheckerData(newCheckerData, pos);
    }
    public void IRecordMove(CheckerData checker, CheckerData killedChecker, Vec2 from, Vec2 to, Vec2 killPos, int playerID)
    {
        PreviousMoves.Add(new RecordMove(checker, killedChecker, from, to, killPos, playerID));
    }

    //008
    //Converting all saved moves to json
    private async Task GetMovesJSON()
    {
        SavingMatchData.MatchName = InGameUI.instance.opponentPlayerName;
        SavingMatchData.MyID = gameModelScript.myPlayer.myId;
        foreach (RecordMove m in PreviousMoves)
        {
            SavingMatchData.allMoves.Add(new Move(m.From, m.To));
        }
        string jsonMoves = JsonUtility.ToJson(SavingMatchData);
        await replayManager.SaveMatch(jsonMoves);
    }

    public async void SaveMatch(string type, string result)
    {
        if (IsMatchSaved == true) return;
        
        SavingMatchData.MatchType = type == "Online" ? "Rank" : "Casual";
        SavingMatchData.Result = result;

        IsMatchSaved = true;
        //WarningPanel.instance.showWarining("match saved");
        await GetMovesJSON();
    }
}
