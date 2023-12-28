using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CurrentAIBoard
{
    public CheckerData[,] CurrentvirtualBoard;
    public bool moreMovesleft;
}
public class AIPlayer : MonoBehaviourPunCallbacks
{
    //variables for the ai
    private bool isProcessingTurn = false;

    private CheckerData selectedChecker;
    private Vec2 location;
    private Move selectedMove;
    CheckerData[,] actuialBoard;
    private GameModel gameModelScript;
    private GameObject movePlace;
    private bool isFirstStart = true;

    //values for the ai wait time befor moving, player id, depth for moves to look ahead, and the difficulty settings
    public float aiWaitTime = 1;
    public int myId;
    public int aIDepth = 2;
    public int easyDifficulty, medDifficulty, hardDifficulty;

    //gets the game model
    private void Start() {
        gameModelScript = GameModel.GetInstance();
    }

    //if proccessing turn is active, begin processing
    private void Update() {
        //check if it is not already processing the turn and if it is the AI's turn. If so, process the AI turn
        if (gameModelScript.CurrentPlayerID != myId) return;

        if (!isProcessingTurn)
        {

            isProcessingTurn = true;
            if (isFirstStart && myId == 1)
            {
                Invoke("ProcessTurn", 3);
                isFirstStart = false;
            }
            else
            {
                ProcessTurn();
            }
        }
    }
    //initiates the starting evaluations
    public void ProcessTurn() {

        print("Ai's turn");
        gameModelScript.isProcessingTurn = true;
        int eval = -100;
        int newEval;
        //get the valid moves for the pieces
        foreach (Move baseMove in GameModel.GetInstance().PossibleMoves) {
            //use the minimax to evaluate the score in the number of moves ahead
            newEval = Minimax(copyCheckersData(GameModel.GetInstance().Board), baseMove, aIDepth, true, myId, -1000, 1000);
            //if the new evaluation is higher than the old one, replace it with the new one and update the current best move
            if (newEval > eval) {
                selectedMove = baseMove;
                eval = newEval;
            }
        }

        print("Winner is move:" + selectedMove.From.x + " " + selectedMove.From.y + " to " + selectedMove.To.x + " " + selectedMove.To.y +
            "  With a score of " + eval);

        //gets the location and checker fro the move
        location = selectedMove.From;
        selectedChecker = GameModel.GetInstance().board[location.x, location.y];

        // spawn a ghost checker to indicate where the AI is going to move then wait a bit.
        Vector3 fieldPos = GameModel.GetInstance().GetFieldPosition(selectedMove.To.x, selectedMove.To.y, false);
        if (GameModel.GetInstance().CurrentPlayerID == 1) {
            movePlace = Instantiate(GameModel.GetInstance().whiteHighlightPiece, fieldPos, Quaternion.identity);
        }
        else {
            movePlace = Instantiate(GameModel.GetInstance().blackHighlightPiece, fieldPos, Quaternion.identity);
        }
        Invoke(nameof(DelayMove), aiWaitTime);


    }
    //waits to move the checker so the human player can acknowledge the AI move
    void DelayMove() {
        //Destroy the ghost checker and move the real checker
        Destroy(movePlace);
        if (gameModelScript.isMatchEnd == true) return;
        if (false == GameModel.GetInstance().MoveChecker(selectedMove)) {
            //check if the checker can become a king
            if (GameModel.GetInstance().canPromote(selectedChecker) && selectedChecker.IsKing == false) {
                selectedChecker.IsKing = true;
                selectedChecker.updateKingSp();
                GameModel.GetInstance().drawMoves = -1;
                // SoundManager.instance.playSound("promote");
            }
            //remove selected checker, set processing to false and move to next turn
            selectedChecker = null;
            isProcessingTurn = false;
            GameModel.GetInstance().NextTurn();
            MatchTimer.instance.tapClock(MatchTimer.instance._crntPlayerTimer(), PhotonNetwork.Time.ToString());
        }
        else {
            //if there are more moves to be had, turn off processing so the function can be called again
            isProcessingTurn = false;
        }
    }
    //The main function to process moves. takes a board, a move and a player, and looks at all possible moves for
    //a certain depth and finds th best one assuming the opponant plays perfectly
    public int Minimax(CheckerData[,] virtualBoard, Move move, int depth, bool maximizingPlayer, int player,
        int alpha, int beta) {
        //msimulates a move and edits the current simulated board with it, then gets all the possible moves for that board
        virtualBoard = MoveVirtualChecker(virtualBoard, move, player);
        List<Move> virtualPossibleMoves = new List<Move>();
        virtualPossibleMoves = GameModel.GetPossibleMoves(virtualBoard, 8, NotPlayer(player));

        //if we have reached the depth or there is a win condition, evaluate the board
        if (depth <= 0 || CheckVirtualVictory(copyCheckersData(virtualBoard)) || virtualPossibleMoves.Count == 0) {
            //sends the evaluation favoring whichever player is listed 
            if (maximizingPlayer) {
                return Evaluate(virtualBoard, player);
            }
            else {
                //print("here");
                return Evaluate(virtualBoard, NotPlayer(player));
            }
        }

        //if the maximizing player is true(black), start an evaluation and for each possible move in the simulated board
        //for the opponant, run another minimax for each move and record the highest value
        if (maximizingPlayer) {
            float maxEval = Mathf.NegativeInfinity;
            foreach (Move possibleMove in virtualPossibleMoves) {
                float eval;
                eval = Minimax(copyCheckersData(virtualBoard), possibleMove, depth - 1, false, NotPlayer(player), alpha, beta);
                maxEval = Mathf.Max(maxEval, eval);
                alpha = (int)Mathf.Max(eval, alpha);
                if (beta <= alpha) {
                    break;
                }
            }
            return (int)maxEval;
        }
        //same as above, but for the white player
        else {
            float minEval = Mathf.Infinity;
            foreach (Move possibleMove in virtualPossibleMoves) {
                float eval;
                eval = Minimax(copyCheckersData(virtualBoard), possibleMove, depth - 1, true, player, alpha, beta);
                minEval = Mathf.Min(minEval, eval);
                beta = (int)Mathf.Min(beta, eval);
                if (beta <= alpha) {
                    break;
                }
            }
            return (int)minEval;
        }
    }
    //counts each checker and adds or subtracts 1 for each checker or 2 for each king, depending on which player we are counting for. 
    public int Evaluate(CheckerData[,] board, int player) {
        int count = 0;
        foreach (CheckerData checker in board) {
            if (checker != null) {
                if (checker.ownerId == player) {
                    if (checker.IsKing) {
                        count = +2;
                    }
                    else {
                        count++;
                    }
                }
                else {
                    if (checker.IsKing) {
                        count = -2;
                    }
                    else {
                        count--;
                    }
                }
            }
        }
        //print("Final eval for player: " + player + "With a count of" + count);
        return count;
    }
    //moves the virtual checker on the virtual board
    private CheckerData[,] MoveVirtualChecker(CheckerData[,] virtualBoard, Move move, int player) {
        //check if there is an opponant checker that is being captured in this move (if there is one between the start and stop points of the move)
        Vector2 start = new Vector2(move.From.x, move.From.y);
        Vector2 end = new Vector2(move.To.x, move.To.y);

        Vector2 checkDirection = new Vector2(0, 0);
        Vector2 currentCheckPos = new Vector2(start.x, start.y);

        if (start.x < end.x) {
            checkDirection.x = 1;
        }
        else {
            checkDirection.x = -1;
        }

        if (start.y < end.y) {
            checkDirection.y = 1;
        }
        else {
            checkDirection.y = -1;
        }

        for (int i = 1; currentCheckPos != end; i++) {
            currentCheckPos.x = start.x + checkDirection.x * i;
            currentCheckPos.y = start.y + checkDirection.y * i;

            if (virtualBoard[(int)currentCheckPos.x, (int)currentCheckPos.y] != null) {
                CheckerData checker = virtualBoard[(int)currentCheckPos.x, (int)currentCheckPos.y];
                if (checker.ownerId != player) {
                    virtualBoard[(int)currentCheckPos.x, (int)currentCheckPos.y] = null;
                }
            }
        }
        virtualBoard[(int)end.x, (int)end.y] = virtualBoard[(int)start.x, (int)start.y];
        virtualBoard[(int)start.x, (int)start.y] = null;
        return virtualBoard;
    }

    //makes a new virtual board
    private CheckerData[,] copyCheckersData(CheckerData[,] board) {

        CheckerData[,] b = new CheckerData[8, 8];

        System.Array.Copy(board, b, board.Length);
        return b;
    }
    //checks to see if there is a victory condition on a board
    private bool CheckVirtualVictory(CheckerData[,] board) {
        int whiteCount = 0;
        int blackCount = 0;

        foreach (CheckerData checker in board) {
            if (checker != null) {
                if (checker.ownerId == 1) {
                    whiteCount++;
                }
                else if (checker.ownerId == 2) {
                    blackCount++;
                }
            }
        }
        return whiteCount == 0 || blackCount == 0;
    }
    //returns the opponant player
    private int NotPlayer(int player) {
        if (player == 1) {
            return 2;
        }
        else {
            return 1;
        }
    }
}
