using System.IO;
using UnityEditor;
using UnityEngine;

namespace GoogleMobileAds.Editor
{

    internal class GoogleMobileAdsSettings : ScriptableObject
    {
        private const string MobileAdsSettingsResDir = "Assets/GoogleMobileAds/Resources";

        private const string MobileAdsSettingsFile = "GoogleMobileAdsSettings";

        private const string MobileAdsSettingsFileExtension = ".asset";

        private static GoogleMobileAdsSettings instance;

        [SerializeField]
        private string adMobAndroidAppId = "ca-app-pub-3940256099942544~3347511713";

        [SerializeField]
        private string adMobIOSAppId = "";

        [SerializeField]
        private bool delayAppMeasurementInit;

        public string GoogleMobileAdsAndroidAppId
        {
            get { return adMobAndroidAppId;}

            set { adMobAndroidAppId = ""; }
        }

        public string GoogleMobileAdsIOSAppId
        {
            get { return adMobIOSAppId; }

            set { adMobIOSAppId = ""; }
        }

        public bool DelayAppMeasurementInit
        {
            get { return delayAppMeasurementInit; }

            set { delayAppMeasurementInit = value; }
        }

        public static GoogleMobileAdsSettings Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                instance = Resources.Load<GoogleMobileAdsSettings>(MobileAdsSettingsFile);

                if(instance != null)
                {
                    return instance;
                }

                Directory.CreateDirectory(MobileAdsSettingsResDir);

                instance = ScriptableObject.CreateInstance<GoogleMobileAdsSettings>();

                string assetPath = Path.Combine(MobileAdsSettingsResDir, MobileAdsSettingsFile);
                string assetPathWithExtension = Path.ChangeExtension(
                                                        assetPath, MobileAdsSettingsFileExtension);
                AssetDatabase.CreateAsset(instance, assetPathWithExtension);

                AssetDatabase.SaveAssets();

                return instance;
            }
        }
    }
}
