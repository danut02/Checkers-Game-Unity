using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

[Serializable]
public class puzzleData
{
    public List<Vec2> blackPieces, whitePieces;
    public List<Vec2> promotedPieces;

    public puzzleData()
    {
        blackPieces = new();
        whitePieces = new();
        promotedPieces = new();
    }
}

public class PuzzleManager : MonoBehaviour
{
    private bool isPlacingPiece;
    private bool isPuzzleDetails;
    private string currentSelectedPiece;
    public GameObject whitePiece, blackPiece;
    public GameObject whiteKing, blackKing;
    public GameObject blankPiece;
    public GameObject warningPlanel;
    public GameObject solvePuzzlePanel;
    public GameObject puzzleRulesPanel;
    private GameObject[] placedPieces = new GameObject[64];
    public GameObject[] selectedPieceInd;
    public GameObject[] fieldObjects;
    public GameObject[] puzzlesObjects;

    public TMP_Dropdown piecesD;
    public TMP_Dropdown opponentD;

    private puzzleData _puzzleData = new puzzleData();
    public GameObject availableRoomsItem;

    solvedPuzzles solved_puzzles;


    public Animator[] kingsPieces;

    public RuntimeAnimatorController[] kingAnimators;
    public int kingAnimId;

    public RectTransform availableRoomsItem_Container;
    private List<GameObject> crntPuzzleItems = new List<GameObject>();


    private bool isWarningActive;


    void Start()
    {
        PlayerPrefs.SetInt("INSTANTPUZZLE", 0);
    }

    void Update()
    {
        //Get out if not own
        if (Input.GetMouseButtonDown(0))
        {
            if (isWarningActive) return;
            else if (isPuzzleDetails) return;
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 50.0f))
            {
                if (hit.transform.CompareTag("PuzzlePiece"))
                {
                    isPlacingPiece = true;
                    currentSelectedPiece = hit.transform.name;

                    foreach (var item in selectedPieceInd)
                    {
                        item.SetActive(false);
                    }
                    switch (currentSelectedPiece)
                    {
                        case "WhitePiece":
                            selectedPieceInd[0].SetActive(true);
                            break;
                        case "BlackPiece":
                            selectedPieceInd[1].SetActive(true);
                            break;
                        case "WhiteKing":
                            selectedPieceInd[2].SetActive(true);
                            break;
                        case "BlackKing":
                            selectedPieceInd[3].SetActive(true);
                            break;
                        case "BlankPiece":
                            selectedPieceInd[4].SetActive(true);
                            break;
                    }

                }
                else if (hit.transform.CompareTag("PuzzleField"))
                {
                    if (isPlacingPiece == false) return;
                    string n = hit.transform.name;
                    int x = Convert.ToInt32(n.Substring(1, 1));
                    int y = Convert.ToInt32(n.Substring(4, 1));
                    placePiece(currentSelectedPiece, new Vector2(x, y));
                }
            }
            else
            {
                //isPlacingPiece = false;
            }

        }
    }

    public void createPuzzle()
    {
        foreach (var item in selectedPieceInd)
        {
            item.SetActive(false);
        }
        currentSelectedPiece = "";
        _puzzleData = new();
        foreach (var obj in puzzlesObjects)
        {
            obj.SetActive(true);
        }
        kingAnimId = PlayerPrefs.GetInt("KingAnimID", 0);
        for (int i = 0; i < 4; i++)
        {
            kingsPieces[i].runtimeAnimatorController = kingAnimators[kingAnimId];
        }
    }

    public void closePuzzle()
    {
        for (int i = 0; i < placedPieces.Length; i++)
        {
            if (placedPieces[i] != null)
                Destroy(placedPieces[i]);
        }
        foreach (var obj in puzzlesObjects)
        {
            obj.SetActive(false);
        }
    }

    public void closeWarningPanel()
    {
        warningPlanel.SetActive(false);
        isWarningActive = false;
    }

    public void savePuzzle()
    {
        //Debug.Log(Application.persistentDataPath);
        if (_puzzleData.blackPieces.Count == 0 || _puzzleData.whitePieces.Count == 0)
        {
            isWarningActive = true;
            warningPlanel.GetComponent<WarningPanel>().showWarining("Place atleast One piece of each sides.");
            return;
        }
        //Save puzzle in json
        string puzzleJson = JsonUtility.ToJson(_puzzleData);

        int puzzleN = 0;
        while (Resources.Load("Puzzles/" + puzzleN.ToString()) as TextAsset != null)
        {
            puzzleN++;
        }
        string path = Application.dataPath + "/Resources/Puzzles/" + puzzleN + ".txt";
        try
        {
            File.WriteAllText(path, puzzleJson);
            for (int i = 0; i < placedPieces.Length; i++)
            {
                if (placedPieces[i] != null)
                    Destroy(placedPieces[i]);
            }
            //AssetDatabase.Refresh();
            closePuzzle();
        }
        catch
        {
            Debug.Log("Cannot Save Puzzle");
        }
    }

    public void playInstantPuzzle()
    {
        //Debug.Log(Application.persistentDataPath);
        if (_puzzleData.blackPieces.Count == 0 || _puzzleData.whitePieces.Count == 0)
        {
            isWarningActive = true;
            warningPlanel.GetComponent<WarningPanel>().showWarining("Place atleast One piece of each sides.");
            return;
        }
        //Save puzzle in json
        string puzzleJson = JsonUtility.ToJson(_puzzleData);
        string path = Application.persistentDataPath + "/-1.txt";
        try
        {
            File.WriteAllText(path, puzzleJson);
            for (int i = 0; i < placedPieces.Length; i++)
            {
                if (placedPieces[i] != null)
                    Destroy(placedPieces[i]);
            }
            closePuzzle();
            puzzleRulesPanel.SetActive(false);
            PlayerPrefs.SetInt("INSTANTPUZZLE", 1);
            startPuzzle(piecesD.value, -1);
        }
        catch
        {
            Debug.Log("Cannot Save Puzzle");
        }
    }

    public void getSavedPuzzles()
    {
        //Debug.Log(Application.persistentDataPath);
        int length = crntPuzzleItems.Count;
        for (int i = 0; i < length; i++)
        {
            Destroy(crntPuzzleItems[0]);
            crntPuzzleItems.RemoveAt(0);
        }
        solved_puzzles = PlayfabManager.instance.solved_puzzles;
        solvePuzzlePanel.SetActive(true);
        int puzzleN = 0;
        float offset = -30;
        float rectHeight = 30;
        while (Resources.Load("Puzzles/" + puzzleN.ToString()) as TextAsset != null)
        {
            Vector3 puzzleItemPos = availableRoomsItem.transform.localPosition;
            puzzleItemPos.y = offset;
            GameObject puzzleItem = Instantiate(availableRoomsItem, availableRoomsItem.transform.parent);
            puzzleItem.transform.localPosition = puzzleItemPos;
            puzzleItem.gameObject.SetActive(true);
            CreatedPuzzleItem puzzleItemScript = puzzleItem.GetComponent<CreatedPuzzleItem>();
            if(isSolvedPuzzle(puzzleN))
                puzzleItemScript.updatePuzzleInfo(puzzleN, true);
            else
                puzzleItemScript.updatePuzzleInfo(puzzleN, false);
            crntPuzzleItems.Add(puzzleItem);
            offset -= 170;
            rectHeight += 170;
            puzzleN++;
        }
    }
    bool isSolvedPuzzle(int puzzleN)
    {
        foreach (int id in solved_puzzles.puzzleIds)
        {
            if (id == puzzleN) return true;
        }
        return false;
    }

    public void startPuzzle(int pieces, int puzzleNo)
    {
        PlayerPrefs.SetInt("PPIECES", pieces);
        PlayerPrefs.SetInt("PUZZLENO", puzzleNo);

        NetworkManager.instance.playPuzzle();
    }

    private void placePiece(string piece, Vector2 position)
    {
        GameObject slctdField = _getFieldObject(position);

        Vec2 _pos = new Vec2((int)position.x, (int)position.y);
        foreach (var piecePos in _puzzleData.blackPieces)
        {
            if (piecePos.Equals(_pos))
            {
                _puzzleData.blackPieces.Remove(_pos);
                Destroy(placedPieces[_getFieldtIndex(position)]);
                break;
            }
        }
        foreach (var piecePos in _puzzleData.whitePieces)
        {
            if (piecePos.Equals(_pos))
            {
                _puzzleData.whitePieces.Remove(_pos);
                Destroy(placedPieces[_getFieldtIndex(position)]);
                break;
            }
        }
        foreach (var piecePos in _puzzleData.promotedPieces)
        {
            if (piecePos.Equals(_pos))
            {
                _puzzleData.promotedPieces.Remove(_pos);
                Destroy(placedPieces[_getFieldtIndex(position)]);
                break;
            }
        }

        switch (piece)
        {
            case "WhitePiece":
                if (isMaxPieceReached("WhitePiece")) return;

                _puzzleData.whitePieces.Add(_pos);
                GameObject w = Instantiate(whitePiece, slctdField.transform.position, Quaternion.identity);
                Destroy(w.GetComponent<BoxCollider>());
                placedPieces[_getFieldtIndex(position)] = w;
                break;

            case "BlackPiece":
                if (isMaxPieceReached("BlackPiece")) return;

                _puzzleData.blackPieces.Add(_pos);
                GameObject b = Instantiate(blackPiece, slctdField.transform.position, Quaternion.identity);
                Destroy(b.GetComponent<BoxCollider>());
                placedPieces[_getFieldtIndex(position)] = b;
                break;

            case "WhiteKing":
                if (isMaxPieceReached("WhitePiece")) return;

                _puzzleData.whitePieces.Add(_pos);
                _puzzleData.promotedPieces.Add(_pos);
                GameObject wk = Instantiate(whiteKing, slctdField.transform.position, Quaternion.identity);
                Destroy(wk.GetComponent<BoxCollider>());
                placedPieces[_getFieldtIndex(position)] = wk;
                break;

            case "BlackKing":
                if (isMaxPieceReached("BlackPiece")) return;

                _puzzleData.blackPieces.Add(_pos);
                _puzzleData.promotedPieces.Add(_pos);
                GameObject bk = Instantiate(blackKing, slctdField.transform.position, Quaternion.identity);
                Destroy(bk.GetComponent<BoxCollider>());
                placedPieces[_getFieldtIndex(position)] = bk;
                break;
        }
    }

    public void addPuzzleDetails()
    {
        if (_puzzleData.blackPieces.Count == 0 || _puzzleData.whitePieces.Count == 0)
        {
            isWarningActive = true;
            warningPlanel.GetComponent<WarningPanel>().showWarining("Place atleast One piece of each sides.");
            return;
        }
        foreach (var item in selectedPieceInd)
        {
            item.SetActive(false);
        }
        puzzleRulesPanel.SetActive(true);
        isPuzzleDetails = true;
    }
    public void closePuzzleDetails()
    {
        isPuzzleDetails = false;
    }

    private GameObject _getFieldObject(Vector2 fieldPos)
    {
        int objectNumber = _getFieldtIndex(fieldPos);
        return fieldObjects[objectNumber];
    }

    private bool isMaxPieceReached(string piece)
    {
        if(piece == "BlackPiece")
        {
            if (_puzzleData.blackPieces.Count == 12)
            {
                isWarningActive = true;
                warningPlanel.GetComponent<WarningPanel>().showWarining("12 PIECE MAXIMUM.");
                return true;
            }
        }
        else if(piece == "WhitePiece")
        {
            if (_puzzleData.whitePieces.Count == 12)
            {
                isWarningActive = true;
                warningPlanel.GetComponent<WarningPanel>().showWarining("12 PIECE MAXIMUM.");
                return true;
            }
        }

        return false;
    }
    private int _getFieldtIndex(Vector2 position)
    {
        return (int)(position.y * 8 + position.x);
    }
}
