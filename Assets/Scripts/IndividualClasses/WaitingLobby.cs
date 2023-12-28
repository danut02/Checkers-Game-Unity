using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WaitingLobby : MonoBehaviour
{
    public NetworkManager networkManagerScript;
    public TMP_Text playerName, opponentName;
    public Image playerRank, opponentRank;
    private bool isMatchFound;

    float timer;
    
    private void OnEnable()
    {
        playerName.text = PlayfabManager.instance.displayName;
        opponentName.gameObject.SetActive(false);
        isMatchFound = false;
    }

    private void Update()
    {
        if(!isMatchFound)
        {
            timer -= Time.deltaTime;
            if(timer < 0)
            {
                timer = 0.2f;
                opponentRank.sprite = networkManagerScript.ranks[Random.Range(0, networkManagerScript.ranks.Length)];
            }
        }
    }

    public void matchFound(string opponentName, Sprite rankImage)
    {
        isMatchFound = true;
        this.opponentName.text = opponentName;
        this.opponentName.gameObject.SetActive(true);
        opponentRank.sprite = rankImage;
    }

    public void cancelMatch()
    {
        isMatchFound = false;
        playerName.gameObject.SetActive(false);
        opponentName.gameObject.SetActive(false);
    }    
}
