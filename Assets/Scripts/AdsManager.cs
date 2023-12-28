using System;
using UnityEngine;
using GoogleMobileAds.Api;
using Random = UnityEngine.Random;

public class AdsManager : MonoBehaviour
{
    public static AdsManager instance;

    [Header("Android")]
    public string android_bannerAdId;
    public string android_interstitialAdId;
    public string android_RewardedVideoAdId;

    [Space(15)]

    [Header("IOS")]
    public string ios_bannerAdId;
    public string ios_interstitialAdId;
    public string ios_RewardedVideoAdId;

    [HideInInspector] public int rewardAdslimit;

    public bool showAds;

    private BannerView bannerView;
    private InterstitialAd interstitial;
    private RewardedAd rewardedAd;
    [HideInInspector] public bool isAdShown;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
    }

    public void Start()
    {
        //PlayerPrefs.DeleteKey("RewardTime");
        //PlayerPrefs.DeleteKey("RewardAdLimit");

        isAdShown = false;
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(initStatus => { });

        RequestBanner();
        RequestInterstitial();
        RequestRewardVideoAd();

        string previousTime = PlayerPrefs.GetString("RewardTime", "01/01/0001 00:00:00");
        TimeSpan timeSpan = DateTime.Now - DateTime.Parse(previousTime);
        if (timeSpan.TotalHours > 23)
        {
            string currentTime = string.Format("{0}/{1}/{2} {3}", DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Year, "00:00:00");
            rewardAdslimit = 5;
            PlayerPrefs.SetInt("RewardAdLimit", rewardAdslimit);
            PlayerPrefs.SetString("RewardTime", currentTime);
        }
        else
        {
            rewardAdslimit = PlayerPrefs.GetInt("RewardAdLimit", 5);
        }
        InventoryManager.Instance.remVidTxt.text = rewardAdslimit.ToString();
    }

    #region Banner_Ad

    private void RequestBanner()
    {
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-3940256099942544/6300978111";

        if (showAds) adUnitId = android_bannerAdId;
#elif UNITY_IPHONE
            string adUnitId = "ca-app-pub-3940256099942544/2934735716";

            if (showAds) adUnitId = ios_bannerAdId;
#else
            string adUnitId = "unexpected_platform";
#endif

        // Create a 320x50 banner at the top of the screen.
        this.bannerView = new BannerView(adUnitId, AdSize.IABBanner, AdPosition.Bottom);

        this.bannerView.OnAdLoaded += this.HandleOnAdLoaded;
        this.bannerView.OnAdFailedToLoad += this.HandleOnAdFailedToLoad;
        //this.bannerView.OnAdOpening += this.HandleOnAdOpened;
        //this.bannerView.OnAdClosed += this.HandleOnAdClosed;


        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();

        // Load the banner with the request.
        this.bannerView.LoadAd(request);
    }
    public void KillBannerAd()
    {
        bannerView.Destroy();
    }

    public void showBannerAd()
    {
        if (bannerView != null)
            KillBannerAd();

        RequestBanner();
    }

    public void HandleOnAdLoaded(object sender, EventArgs args)
    {
        //MonoBehaviour.print("HandleAdLoaded event received");
    }

    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {

    }

    //public void HandleOnAdOpened(object sender, EventArgs args)
    //{
    //    MonoBehaviour.print("HandleAdOpened event received");
    //}

    //public void HandleOnAdClosed(object sender, EventArgs args)
    //{
    //    MonoBehaviour.print("HandleAdClosed event received");
    //}
    #endregion

    #region Interstitial_Ad

    private void RequestInterstitial()
    {
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-3940256099942544/1033173712";

        if (showAds) adUnitId = android_interstitialAdId;
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3940256099942544/4411468910";
        
        if (showAds) adUnitId = ios_interstitialAdId;
#else
        string adUnitId = "unexpected_platform";
#endif

        // Initialize an InterstitialAd.
        this.interstitial = new InterstitialAd(adUnitId);

        this.interstitial.OnAdLoaded += I_HandleOnAdLoaded;
        this.interstitial.OnAdFailedToLoad += I_HandleOnAdFailedToLoad;
        //this.interstitial.OnAdOpening += I_HandleOnAdOpened;
        //this.interstitial.OnAdClosed += I_HandleOnAdClosed;


        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the interstitial with the request.
        this.interstitial.LoadAd(request);
    }


    public bool showInterstitialAd()
    {
        if (!showAds) return false;
        if (isAdShown) return false;

        int showAd = Random.Range(0, 5);

        if (showAd == 0 && interstitial.IsLoaded())
        {
            this.interstitial.Show();
            return true;
        }
        else
            return false;
    }

    public void I_HandleOnAdLoaded(object sender, EventArgs args)
    {
        //MonoBehaviour.print("HandleAdLoaded event received");
    }

    public void I_HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {

    }

    public void I_HandleOnAdOpened(object sender, EventArgs args)
    {
        isAdShown = true;
    }

    public void I_HandleOnAdClosed(object sender, EventArgs args)
    {
        isAdShown = true;
    }
    #endregion

#region RewardVideoAd

    private void RequestRewardVideoAd()
    {
        string adUnitId;
    #if UNITY_ANDROID   
        adUnitId = "ca-app-pub-3940256099942544/5224354917";
            if (showAds) adUnitId = android_RewardedVideoAdId;
    #elif UNITY_IOS
                adUnitId = "ca-app-pub-3940256099942544/1712485313";
                //if (showAds) adUnitId = ios_bannerAdId;
    #else
                adUnitId = "unexpected_platform";
    #endif

        this.rewardedAd = new RewardedAd(adUnitId);

        // Called when an ad request has successfully loaded.
        this.rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        // Called when an ad request failed to load.
        this.rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        // Called when an ad is shown.
        this.rewardedAd.OnAdOpening += HandleRewardedAdOpening;
        // Called when an ad request failed to show.
        this.rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
        // Called when the user should be rewarded for interacting with the ad.
        this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        // Called when the ad is closed.
        this.rewardedAd.OnAdClosed += HandleRewardedAdClosed;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded ad with the request.
        this.rewardedAd.LoadAd(request);
    }

    public bool WatchRewardedVideoAd()
    {
        if (rewardAdslimit > 0)
        {
            if (this.rewardedAd.IsLoaded())
            {
                this.rewardedAd.Show();
            }
            return true;
        }
        else
            return false;
    }


    public void HandleRewardedAdLoaded(object sender, EventArgs args)
    {
        //MonoBehaviour.print("HandleRewardedAdLoaded event received");
    }

    public void HandleRewardedAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        InventoryManager.Instance.videoAdFailed();

        MonoBehaviour.print(
            "HandleRewardedAdFailedToLoad event received with message: "
                             + args.LoadAdError.GetMessage());
    }

    public void HandleRewardedAdOpening(object sender, EventArgs args)
    {
        //MonoBehaviour.print("HandleRewardedAdOpening event received");
    }

    public void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args)
    {
        InventoryManager.Instance.videoAdFailed();
        MonoBehaviour.print(
            "HandleRewardedAdFailedToShow event received with message: "
                             + args.AdError.GetMessage());
    }

    public void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        InventoryManager.Instance.videoAdFailed();
        print("HandleRewardedAdClosed event received");
    }


    public void HandleUserEarnedReward(object sender, Reward args)
    {
        rewardAdslimit--;
        PlayerPrefs.SetInt("RewardAdLimit", rewardAdslimit);

        string type = args.Type;
        double amount = args.Amount;
        print(
            "HandleRewardedAdRewarded event received for "
                        + amount.ToString() + " " + type);

        InventoryManager.Instance.addCoins(20);
        RequestRewardVideoAd();
    }


#endregion
}




