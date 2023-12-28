using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Photon.Pun;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class MenuManager : MonoBehaviour
{
    private PlayfabManager _playfabManagerScript;
    [SerializeField] private NetworkManager networkManager;

    public static MenuManager instance;
    public TMP_Text nameText;
    public TMP_Text regionText;
    public TMP_Text CoinsText;
    public TMP_Text rank;
    public TMP_Text piecesBtn, boardBtn;
    public TMP_Text pieces_all_btn, pieces_flag_btn;
    public TMP_Text[] customSubPanels_btns;

    public GameObject networkManagerPrefeb;
    public GameObject waitingLobby;
    public GameObject loadingScreen;
    public GameObject matchingScreen;
    public GameObject customizationPanel;
    public GameObject piecesPanel;
    public GameObject pieces_Sub_Panel;
    public GameObject pieces_Flag_Panel;
    public GameObject boardsPanel;
    public GameObject shopPanel;
    public GameObject shopErrorPanel;
    public GameObject shopFlags;
    public GameObject[] customSubPanels;
    public GameObject[] selectedEmotes;
    public GameObject[] replayMatchesBlocks;

    public Transform shop_Flags_P;
    public Transform shop_Pieces_P;

    //008
    [SerializeField] private RectTransform replayMatchesBlocksContent;

    public Sprite useSp, inUseSp;
    public Sprite[] emotes;

    public Image[] boardBtns;
    public Image[] piecesBtns;
    public Image[] damaBtns;
    public Image[] emotesBtn;

    List<int> selectedEmotesIds = new List<int>();

    bool isSound = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        _playfabManagerScript = PlayfabManager.instance;
        Time.timeScale = 1;
        updatePlayerResources();
        AudioManager.instance.initializeAudio();
        StartCoroutine(loadCustomizations());

        CheckAvailableMatchesToWatch();
    }

    private void updatePlayerResources()
    {
        nameText.text = _playfabManagerScript.displayName;
        CoinsText.text = _playfabManagerScript.coins.ToString();
        rank.text = _playfabManagerScript.rankPoints.ToString();
        PhotonNetwork.LocalPlayer.NickName = nameText.text;
        InventoryManager.Instance.updateCoins(_playfabManagerScript.coins);
    }

    private void OnPreviousMatchStatus(ModifyUserVirtualCurrencyResult obj)
    {
        _playfabManagerScript.coins = obj.Balance;
        InventoryManager.Instance.updateCoins(obj.Balance);
        CoinsText.text = _playfabManagerScript.coins.ToString();
        rank.text = _playfabManagerScript.rankPoints.ToString();
        PlayerPrefs.SetInt("IsInMatch", 0);
        loadingScreen.SetActive(false);
    }

    #region ReplayMatches
    private async void CheckAvailableMatchesToWatch()
    {
        int noOfMatches = 0;
        for (int i = 0; i < 20; i++)
        {
            //Debug.Log(Resources.Load("MatchData/" + i.ToString()));

            if (File.Exists(MatchReplayManager.filePath(i)))
            {
                string matchData = await File.ReadAllTextAsync(MatchReplayManager.filePath(i));
                SavedMatch match = JsonUtility.FromJson<SavedMatch>(matchData);

                noOfMatches = i;
                replayMatchesBlocks[i].GetComponent<SavedMatchItem>().ShowMatchItem(match.MatchName, match.MatchType, match.Result);
            }
            else
                break;
        }

        int contentHeight = 115 * noOfMatches + 125;
        replayMatchesBlocksContent.sizeDelta = new Vector2(0, contentHeight);
    }
    #endregion


    #region Customization
    private IEnumerator loadCustomizations()
    {
        yield return new WaitForSeconds(1);
        selectBoard(PlayerPrefs.GetInt("BoardID", 0));
        selectPiece(PlayerPrefs.GetInt("PieceID", 0));
        selectDama(PlayerPrefs.GetInt("KingAnimID", 0));
        loadEmotes();
    }
    public void showCustomizationPanel()
    {
        switchCustomizationPnls(0);
        customizationPanel.SetActive(true);
    }
    public void switchCustomizationPnls(int pnlId)
    {
        for (int i = 0; i < 4; i++)
        {
            customSubPanels[i].SetActive(false);
            customSubPanels_btns[i].color = Color.gray;

            if(i == pnlId)
            {
                customSubPanels[i].SetActive(true);
                customSubPanels_btns[i].color = Color.white;
            }
        }
    }

    public void selectBoard(int boardN)
    {
        int prevBtn = PlayerPrefs.GetInt("BoardID", 0);
        PlayerPrefs.SetInt("BoardID", boardN);

        boardBtns[prevBtn].sprite = useSp;
        boardBtns[prevBtn].SetNativeSize();
        boardBtns[boardN].sprite = inUseSp;
        boardBtns[boardN].SetNativeSize();


        if (!isSound) return;
        AudioManager.instance.click();
    }
    public void selectPiece(int pieceN)
    {
        int prevBtn = PlayerPrefs.GetInt("PieceID", 0);
        PlayerPrefs.SetInt("PieceID", pieceN);

        piecesBtns[prevBtn].sprite = useSp;
        piecesBtns[prevBtn].SetNativeSize();
        piecesBtns[pieceN].sprite = inUseSp;
        piecesBtns[pieceN].SetNativeSize();

        if (!isSound) return;
        AudioManager.instance.click();
    }
    public void select_Sub_Pieces()
    {
        pieces_Sub_Panel.SetActive(true);
        pieces_Flag_Panel.SetActive(false);
        pieces_flag_btn.color = Color.gray;
        pieces_all_btn.color = Color.white;
        shopFlags.transform.SetParent(shop_Pieces_P);
    }
    public void select_Flag_Pieces()
    {
        pieces_Sub_Panel.SetActive(false);
        pieces_Flag_Panel.SetActive(true);
        pieces_flag_btn.color = Color.white;
        pieces_all_btn.color = Color.gray;
        shopFlags.transform.SetParent(shop_Flags_P);
    }
    public void selectDama(int damaN)
    {
        int prevBtn = PlayerPrefs.GetInt("KingAnimID", 0);
        PlayerPrefs.SetInt("KingAnimID", damaN);

        damaBtns[prevBtn].sprite = useSp;
        damaBtns[prevBtn].SetNativeSize();
        damaBtns[damaN].sprite = inUseSp;
        damaBtns[damaN].SetNativeSize();

        if (!isSound) return;
        AudioManager.instance.click();
    }
    public void addEmote(int id)
    {
        if (selectedEmotesIds.Count >= 4) removeEmote(0);

        selectedEmotesIds.Add(id);
        arrangeEmotes();

        emotesBtn[id].sprite = inUseSp;
        emotesBtn[id].SetNativeSize();
        emotesBtn[id].gameObject.GetComponent<Button>().interactable = false;
    }
    public void removeEmote(int id)
    {
        int emIndex = selectedEmotesIds[id];
        selectedEmotesIds.RemoveAt(id);
        arrangeEmotes();
        emotesBtn[emIndex].sprite = useSp;
        emotesBtn[emIndex].SetNativeSize();
        emotesBtn[emIndex].gameObject.GetComponent<Button>().interactable = true;

    }
    void arrangeEmotes()
    {
        for (int i = 0; i < 4; i++)
        {
            selectedEmotes[i].transform.GetChild(0).gameObject.SetActive(false);
            selectedEmotes[i].transform.GetChild(1).gameObject.SetActive(false);
        }
        for (int i = 0; i < selectedEmotesIds.Count; i++)
        {
            selectedEmotes[i].transform.GetChild(0).GetComponent<Image>().sprite = emotes[selectedEmotesIds[i]];
            selectedEmotes[i].transform.GetChild(0).gameObject.SetActive(true);
            selectedEmotes[i].transform.GetChild(1).gameObject.SetActive(true);
        }
    }
    public void saveEmotes()
    {
        PlayerPrefs.SetInt("EmoteL", selectedEmotesIds.Count);
        for (int i = 0; i < selectedEmotesIds.Count; i++)
        {
            PlayerPrefs.SetInt("Emote" + i, selectedEmotesIds[i]);
        }
    }
    private void loadEmotes()
    {
        int l = PlayerPrefs.GetInt("EmoteL", 4);
        for (int i = 0; i < l; i++)
        {
            int id = PlayerPrefs.GetInt("Emote" + i, i);
            addEmote(id);
            //selectedEmotesIds.Add(id);
        }
        arrangeEmotes();
    }

    #endregion

    public static void defaultCustomization()
    {
        PlayerPrefs.SetInt("BoardID", 0);
        PlayerPrefs.SetInt("PieceID", 0);
        PlayerPrefs.SetInt("KingAnimID", 0);
        for (int i = 0; i < 4; i++)
        {
            PlayerPrefs.SetInt("Emote" + i, i);
        }
    }

    #region OnClick
    public void OnClickShop()
    {
        string dName = _playfabManagerScript.displayName;
        if (dName.Length >= 5)
        {
            if (dName.Substring(0, 5) != "Guest")
                shopPanel.SetActive(true);
            else
                shopErrorPanel.SetActive(true);
        }
        else
            shopPanel.SetActive(true);
    }
    public void onClickPlay()
    {
        networkManager.findMatch();
    }
    //008
    public void OnClickWatchReplayMatch(int matchId)
    {
        PlayerPrefs.SetInt("REID", matchId);
        networkManager.PlayReplayMatch();
    }
    #endregion

    public void startGame()
    {
        SceneManager.LoadScene(1);
    }
    public void signout()
    {
        loadingScreen.SetActive(true);
        //SceneManager.LoadScene("Login");
        if (!_playfabManagerScript.UnlinkCustomID())
            loadingScreen.SetActive(false);
    }
    public void quiteGame()
    {
        Application.Quit();
    }
}
