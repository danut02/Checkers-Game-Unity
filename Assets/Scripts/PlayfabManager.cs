using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using TMPro;
using UnityEngine.SceneManagement;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

using Photon.Pun;
using System.Threading;
using System.Threading.Tasks;

[Serializable]
public class solvedPuzzles
{
    public List<int> puzzleIds = new List<int>();
}
public class GuestNameGenerator
{
    public string GuestName;
    public GuestNameGenerator()
    {
        GuestName = "Guest ";
        for (int i = 0; i < 4; i++)
        {
            GuestName += UnityEngine.Random.Range(0, 10);
        }
    }
}

public class PlayfabManager : MonoBehaviourPunCallbacks
{
    public static PlayfabManager instance;

    [HideInInspector] public GameObject signInPanel;
    [HideInInspector] public GameObject playfabConnectingScreen;
    [HideInInspector] public GameObject displayNamePanel;
    [HideInInspector] public GameObject loadingScreen;
    [HideInInspector] public GameObject loginError;
    [HideInInspector] public TMP_InputField displayInputField;

    private bool NewlyCreatedAccount = false;
    [HideInInspector] public string displayName = string.Empty;
    [HideInInspector] public int coins;
    [HideInInspector] public int rankPoints = 0;
    [HideInInspector] public solvedPuzzles solved_puzzles = new solvedPuzzles();
    [HideInInspector] public Dictionary<string, UserDataRecord> userData = new Dictionary<string, UserDataRecord>();
    //Login

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
        }
        else
         {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
        .AddOauthScope("profile")
        .RequestServerAuthCode(false)
        .Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
    }

    public void autoLogin()
    {
        PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest()
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        },
        response =>
        {
            displayName = response.InfoResultPayload.PlayerProfile.DisplayName;
            loadUserData();
        },
        error =>
        {
            Debug.Log(error.Error);

            if (error.Error.ToString() == "AccountNotFound")     //No account found lets create one
            {
                signInPanel.SetActive(true);
                playfabConnectingScreen.SetActive(false);
            }
        });

    }
    #region GOOGLE LOGIN
    public void loginWithGoogle()   
    {
        loadingScreen.SetActive(true);
        Social.localUser.Authenticate((bool success) => {

            if (success)
            {
                var serverAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();
                PlayFabClientAPI.LoginWithGoogleAccount(new LoginWithGoogleAccountRequest()
                {
                    ServerAuthCode = serverAuthCode,
                    CreateAccount = true,
                    InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                    {
                        GetPlayerProfile = true
                    }
                },
                response =>
                {
                    NewlyCreatedAccount = response.NewlyCreated;
                    linkCustomDeviceId();
                    if (!response.NewlyCreated)
                    {
                        displayName = response.InfoResultPayload.PlayerProfile.DisplayName;
                    }
                },
                error =>
                {
                    Debug.Log(error.GenerateErrorReport());
                    loginError.SetActive(true);
                });
            }
            else
            {
                loginError.SetActive(true);
            }

        });
        
    }
    //for keed signed in a device
    private void linkCustomDeviceId()
    {
        PlayFabClientAPI.LinkCustomID(new LinkCustomIDRequest()
        {
            CustomId = SystemInfo.deviceUniqueIdentifier
        },
        response =>
        {
            if(NewlyCreatedAccount)
            {
                signInPanel.SetActive(false);
                loadingScreen.SetActive(false);
                displayNamePanel.SetActive(true);

                PlayerPrefs.SetInt("EmoteL", 4);// Default emotes for new account
                for (int i = 0; i < 4; i++)
                {
                    PlayerPrefs.SetInt("Emote" + i, i);
                }
            }
            else
                loadUserData();
        },
        OnError);
    }
    //Sign out from a device
    public bool UnlinkCustomID()
    {
        bool signOutResult = false;
        PlayFabClientAPI.UnlinkCustomID(new UnlinkCustomIDRequest()
        {
            CustomId = SystemInfo.deviceUniqueIdentifier
        },
        response =>
        {
            MenuManager.defaultCustomization();
            SceneManager.LoadScene("Login");
            signOutResult = true;
        },
        OnError);
        return signOutResult;
    }
    #endregion

    public void LoginAsGuest()
    {
        loadingScreen.SetActive(true);
        PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest()
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        },
        response =>
        {
            if (response.NewlyCreated)
            {
                updateNameInServer(new GuestNameGenerator().GuestName);
                PlayerPrefs.SetInt("EmoteL", 4);// Default emotes for new account
                for (int i = 0; i < 4; i++)
                {
                    PlayerPrefs.SetInt("Emote" + i, i);
                }
            }
            else
            {
                displayName = response.InfoResultPayload.PlayerProfile.DisplayName;
                signInPanel.SetActive(false);
                loadUserData();
            }
        },
        OnError);
    }
    private async void loadUserData()
    {
        await PlayfabCustomRequests.GetUserInventory(OnGetInventory, OnError);
        await PlayfabCustomRequests.GetUserCustomData(OnUserDataGet, OnError);
    }
    private void gotoMenu()
    {
        SceneManager.LoadScene("Menu");
    }
    public void updateDisplayName()
    {
        if (displayInputField.text.Length < 3) return;
        loadingScreen.SetActive(true);
        updateNameInServer(displayInputField.text);

    }
    private void updateNameInServer(string displayName)
    {
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest()
        {
            DisplayName = displayName
        },
        response =>
        {
            this.displayName = response.DisplayName;
            loadUserData();
        },
        OnError);
    }

    private void OnGetInventory(GetUserInventoryResult inventory)
    {
        coins = inventory.VirtualCurrency["CN"];
    }
    private void OnUserDataGet(GetUserDataResult response)
    {
        userData = response.Data;
        if (response.Data.ContainsKey("RankPoints")) rankPoints = int.Parse(response.Data["RankPoints"].Value);
        if (response.Data.ContainsKey("DonePuzzles")) solved_puzzles = JsonUtility.FromJson<solvedPuzzles>(response.Data["DonePuzzles"].Value);

        if (PlayerPrefs.GetInt("IsInMatch", 0) == 1)
        {
            updatePreviousMatchStatus(OnCoinsUpdate);
        }
        else
            gotoMenu();
    }
    public async void updatePreviousMatchStatus(Action<ModifyUserVirtualCurrencyResult> OnDoneAction = null)
    {
        if (rankPoints > 10)
        {
            int prevOpRank = PlayerPrefs.GetInt("PrevOpRank", 0);
            rankPoints = GameModel.CalculateRankPoints(rankPoints, prevOpRank, "Lost");
            await PlayfabCustomRequests.UpdateUserData("RankPoints", rankPoints.ToString(), OnUserDataUpdated, OnError);
        }
        await PlayfabCustomRequests.AddVirtualCurrency(CoinsReward.match_Lost_coins, OnDoneAction, OnError);
    }
    private void OnUserDataUpdated(UpdateUserDataResult obj)
    {
    }
    private void OnCoinsUpdate(ModifyUserVirtualCurrencyResult obj)
    {
        PlayerPrefs.SetInt("IsInMatch", 0);
        coins = obj.Balance;
        gotoMenu();
    }
    public void OnError(PlayFabError error)
    {
        print(error.ErrorMessage);
    }
}
