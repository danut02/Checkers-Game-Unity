using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameEndView : MonoBehaviour
{
    public TMP_Text opponentNameTxt, rankPoints;
    public void updateOpponentName(string winText)
    {
        opponentNameTxt.text = winText;
        rankPoints.text = PlayfabManager.instance.rankPoints.ToString();
    }
}
