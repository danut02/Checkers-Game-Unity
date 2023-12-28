using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

//008

public class MatchReplayManager : MonoBehaviour
{
    [SerializeField] private GameModel gameModel;
    [SerializeField] private UndoSystem undoManager;
    public Player player;

    public SavedMatch MatchMoves = new SavedMatch();
    [HideInInspector]public int currentMoveIndex = 0;
    int numberOfMatches = 0;

    string path = string.Empty; 

    public string movesData;
        
    async void Awake()
    {
        gameModel = GameModel.GetInstance();
        //Saving match id to start this id match in game.
        //TextAsset levelFile = Resources.Load("MatchData/" + PlayerPrefs.GetInt("REID", 0).ToString()) as TextAsset;
        movesData = await File.ReadAllTextAsync(filePath(PlayerPrefs.GetInt("REID", 0)));
        MatchMoves = JsonUtility.FromJson<SavedMatch>(movesData);

        InGameUI.instance.opponentPlayerNameText.text = MatchMoves.MatchName;

        gameModel.myClientID = MatchMoves.MyID;
        gameModel.SetupPlayreIDs();
        //gameModel.opponentGameID = MatchMoves.MyID == 1 ? 2 : 1;
    }

    public async Task SaveMatch(string movesJson)
    {
        //Get Current Files Name
        for (int i = 0; i < 20; i++)
        {
            if (!File.Exists(filePath(i)))
            {
                path = filePath(i);
                break;
            }
            else if (i == 19) // All Match saved
            {
                path = filePath(i);
                RemoveVeryLastMatch();
                break;
            }
        }
        await File.WriteAllTextAsync(path, movesJson); //Saving Match on device
        //Debug.Log("Match Saved");
    }

    private void RemoveVeryLastMatch()
    {
        for (int i = 0; i < 20; i++)
        {
            path = filePath(i);
            if (i == 0)
                File.Delete(path);
            else
                File.Move(path, filePath(i - 1));
        }
    }
    public static string filePath(int matchId)
    {
        string fileName = string.Format("{0}{1}.{2}", "M", matchId, "txt");
        //Debug.Log(Application.persistentDataPath + fileName);
        return Path.Combine(Application.persistentDataPath, fileName);
        //return Path.Combine(Application.dataPath + "/Resources/MatchData", fileName);
    }

    public void NextMove()
    {
        if (player == null) player = gameModel.myPlayer; //Get player if player is null

        if (currentMoveIndex >= MatchMoves.allMoves.Count) return;
        if (currentMoveIndex < 0) currentMoveIndex = 0;

        Move move = MatchMoves.allMoves[currentMoveIndex];

        //Select Checker to move
        player.SelectedChecker = gameModel.GetCheckerData(move.From);

        player.processTurn(null, move);
        currentMoveIndex++;
    }
    public void PreviousMove()
    {
        if (currentMoveIndex < 0) return;
        if (undoManager.undoTurn()) currentMoveIndex--;
    }
}
