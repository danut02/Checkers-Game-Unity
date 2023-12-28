using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Emotes : MonoBehaviourPunCallbacks
{
    public GameModel gameModelScript;
    public AudioManager audioScript;
    [HideInInspector] public int counter;
    public GameObject emotesPanel;
    public GameObject emoteDisplaySelf, emoteDisplayEnemy;
    public Sprite[] emoteSprites;
    public Image[] emotes;
    private int[] chosenEmotesIds = new int[4] { 0, 0, 0, 0 };
    private bool isHost = false;

    private void Start()
    {
        isHost = gameModelScript.isHost;
        int l = PlayerPrefs.GetInt("EmoteL", 4);
        for (int i = 0; i < l; i++)
        {
            int id = PlayerPrefs.GetInt("Emote" + i, i);
            emotes[i].sprite = emoteSprites[id];
            chosenEmotesIds[i] = id;
        }
    }

    public void openEmotes()
    {
        if (PlayerPrefs.GetInt("ShowEmote", 1) == 0) return;
        counter++;
        if (counter % 2 == 1)
            emotesPanel.SetActive(true);
        else
            emotesPanel.SetActive(false);
    }

    [PunRPC]
    public void emote_Rpc(int emoteId, PhotonMessageInfo info)
    {
        if (PlayerPrefs.GetInt("ShowEmote", 1) == 0) return;
        audioScript.emote(emoteId);
        if (isHost)
            hostEmote(emoteId, info);
        else
            RecieveEmote(emoteId, info);
        StartCoroutine(hideEmote());
    }

    private IEnumerator hideEmote()
    {
        yield return new WaitForSeconds(3);

        emoteDisplaySelf.SetActive(false);
        emoteDisplayEnemy.SetActive(false);
    }
    public void sendEmote(int id)
    {
        photonView.RPC("emote_Rpc", RpcTarget.All, chosenEmotesIds[id]);
    }

    public void RecieveEmote(int emoteId, PhotonMessageInfo sender)
    {
        if (sender.Sender == PhotonNetwork.LocalPlayer)
        {
            openEmotes();       //Closing the emotes panel
            emoteDisplaySelf.transform.GetChild(0).GetComponent<Image>().sprite = emoteSprites[emoteId];
            emoteDisplaySelf.SetActive(true);
        }
        else
        {
            emoteDisplayEnemy.transform.GetChild(0).GetComponent<Image>().sprite = emoteSprites[emoteId];
            emoteDisplayEnemy.SetActive(true);
        }
    }

    public void hostEmote(int emoteId, PhotonMessageInfo sender)
    {
        if (sender.Sender == gameModelScript.whitePhotonPlayer)
        {
            emoteDisplaySelf.transform.GetChild(0).GetComponent<Image>().sprite = emoteSprites[emoteId];
            emoteDisplaySelf.SetActive(true);
        }
        else
        {
            emoteDisplayEnemy.transform.GetChild(0).GetComponent<Image>().sprite = emoteSprites[emoteId];
            emoteDisplayEnemy.SetActive(true);
        }
    }

}
