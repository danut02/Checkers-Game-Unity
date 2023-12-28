using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CreatedPuzzleItem : MonoBehaviour
{
    private int puzzleNo = 0;
    public TMP_Text puzzleNameTxt;
    public PuzzleManager puzzleManagerScript;
    public GameObject coinsIcon;

    public void updatePuzzleInfo(int number, bool isSolved)
    {
        if(isSolved)
            coinsIcon.SetActive(false);
        puzzleNameTxt.text = "PUZZLE " + number;
        puzzleNo = number;
    }

    public void playPuzzle()
    {
        puzzleManagerScript.startPuzzle(0, puzzleNo);
    }
}
