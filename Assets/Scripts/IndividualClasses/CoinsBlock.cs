using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinsBlock : MonoBehaviour
{
    public TMPro.TMP_Text coinsTxt;

    public void updateCoins(int newCoins)
    {
        coinsTxt.text = newCoins.ToString();
    }


}
