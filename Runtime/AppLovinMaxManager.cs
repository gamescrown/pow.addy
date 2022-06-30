using System;
using System.Collections;
using com.adjust.sdk;
using pow.aidkit;
using pow.hermes;
using UnityEngine;

namespace pow.addy
{
    [RequireComponent(
            typeof(BannerController),
            typeof(InterstitialController),
            typeof(RewardedController)
        )
    ]
    public class AppLovinMaxManager : Singleton<AppLovinMaxManager>
    {
        [SerializeField] private AdEventHandler adEventHandler;
        [SerializeField] private TestDeviceHandler testDeviceHandler;
        [SerializeField] private Policies policies;
        [SerializeField] private GameEvent onSetUserConsentStatus;
        [SerializeField] private GameEvent onShowUserConsentPopup;
        [SerializeField] internal string maxSdkKey;

        [Header("IOS")] [SerializeField] internal string bannerIdIOS;
        [SerializeField] internal string rewardedIdIOS;
        [SerializeField] internal string interstitialIdIOS;
        [Header("ANDROID")] [SerializeField] internal string bannerIdAndroid;
        [SerializeField] internal string rewardedIdAndroid;
        [SerializeField] internal string interstitialIdAndroid;

        private InterstitialController _interstitialController;
        private RewardedController _rewardedController;
        private BannerController _bannerController;

        protected override void Awake()
        {
            base.Awake();
            _bannerController = GetComponent<BannerController>();
            _rewardedController = GetComponent<RewardedController>();
            _interstitialController = GetComponent<InterstitialController>();

#if UNITY_ANDROID
            _bannerController.SetAdID(bannerIdAndroid);
            _rewardedController.SetAdID(rewardedIdAndroid);
            _interstitialController.SetAdID(interstitialIdAndroid);
#elif UNITY_IOS
            _bannerController.SetAdID(bannerIdIOS);
            _rewardedController.SetAdID(rewardedIdIOS);
            _interstitialController.SetAdID(interstitialIdIOS);
#endif
        }

        private void Start()
        {
            MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
            {
                print("[ApplovinMAX] MaxSdk Initialized");
                // You can check app transparency tracking authorization in sdkConfiguration.AppTrackingStatus for Unity Editor and iOS targets.
                // Initialize other third-party SDKs; do not initialize mediated advertising SDKs (MAX does that for you). Not following this step will result in noticeable integration issues.

                // Initialize the Adjust SDK inside the Applovin SDK's initialization callback
                //string adjustAppToken = "{YOUR_ADJUST_APP_TOKEN}";
                //AdjustEnvironment adjustEnvironment = AdjustEnvironment.Production;
                //AdjustConfig config = new AdjustConfig(adjustAppToken, adjustEnvironment);

                //Adjust.start(config);

                // Start loading ads
                // AppLovin SDK is initialized, start loading ads
                _interstitialController.InitializeInterstitialAds();
                _rewardedController.InitializeRewardedAds();
                _bannerController.InitializeBannerAds();

                MaxSdk.SetVerboseLogging(true);

                // TODO: Remove on release build
                // TODO: Control test devices adId from remote config and show applovin debugger on only this devices
                // TODO: Add POW_DEBUG flag to project settings and activate it on development builds
                //MaxSdk.ShowMediationDebugger();
#if UNITY_IOS
                // Use this code blocks for ios only,
                // because att permission popup show at the first open and cant get adID from device if user's not allow tracking permission
                if (testDeviceHandler.AdId.StartsWith("0000") || string.IsNullOrEmpty(testDeviceHandler.AdId))
                {
                    testDeviceHandler.GetDeviceAdIdOnIOS();
                }
#endif
            };

            MaxSdkCallbacks.OnSdkInitializedEvent += (sdkConfiguration) =>
            {
                if (sdkConfiguration.ConsentDialogState == MaxSdkBase.ConsentDialogState.Applies)
                {
                    // Show user consent dialog

#if UNITY_ANDROID
                    if (policies.HasUserConsent == -1)
                    {
                        print("[ApplovinMAX] MaxSdkBase.ConsentDialogState.Applies ShowConsentDialog on Android");
                        onShowUserConsentPopup?.Invoke();
                    }
                    else
                    {
                        print("[ApplovinMAX] MaxSdkBase.ConsentDialogState.Applies user already see consent popup");
                        SetConsentStatus(policies.HasUserConsent == 1);
                    }

#elif UNITY_IOS
                    SetConsentStatus(true);
#else
                    print("[ApplovinMAX] MaxSdkBase.ConsentDialogState.Applies not show consent dialog on editor");
                    SetConsentStatus(true);
#endif
                }
                else if (sdkConfiguration.ConsentDialogState == MaxSdkBase.ConsentDialogState.DoesNotApply)
                {
                    // No need to show consent dialog, proceed with initialization
                    print("[ApplovinMAX] MaxSdkBase.ConsentDialogState.DoesNotApply");
                    SetConsentStatus(true);
                }
                else
                {
                    // Consent dialog state is unknown. Proceed with initialization, but check if the consent
                    // dialog should be shown on the next application initialization
                    print("[ApplovinMAX] MaxSdkBase.ConsentDialogState.Unknown");
                    SetConsentStatus(true);
                }
            };

            MaxSdk.SetSdkKey(maxSdkKey);
            MaxSdk.InitializeSdk();
        }

        public bool IsInitialized()
        {
            return MaxSdk.IsInitialized();
        }

        // Called from OnAdjustInitialized game event
        public void SetUserSegment(string segment)
        {
            MaxSdk.UserSegment.Name = segment;
        }

        public void SetUserId()
        {
            MaxSdk.SetUserId(Adjust.getAdid());
        }

        public void SetConsentStatus(bool hasUserConsent)
        {
            policies.HasUserConsent = hasUserConsent ? 1 : 0;
            MaxSdk.SetHasUserConsent(hasUserConsent);
            print($"[ApplovinMAX] OnSetUserConsentStatus");
            onSetUserConsentStatus?.Invoke();
        }

        public void ShowInterstitial(InterstitialTag interstitialTag, Action onCompleteAction)
        {
            adEventHandler.SetInterstitialCompletedAction(onCompleteAction);
            _interstitialController.ShowInterstitial(interstitialTag.ToString());
        }

        public void ShowRewarded(RewardedVideoTag rewardedTag, Action onCompletedAction, Action onFailedAction)
        {
            adEventHandler.SetRewardedCompletedAction(onCompletedAction);
            adEventHandler.SetRewardedFailedAction(onFailedAction);
            _rewardedController.ShowRewardedAd(rewardedTag.ToString());
        }

        public void ShowBanner()
        {
            _bannerController.ShowBanner();
        }

        public void HideBanner()
        {
            _bannerController.HideBanner();
        }

        public float GetAdaptiveBannerHeight()
        {
            return MaxSdkUtils.GetAdaptiveBannerHeight(Screen.width);
        }

        // Trigger Max Debugger after fetched test devices from firebase remote config
        // Used from onTestUserFetched GameEvent
        public void TriggerMaxDebugger()
        {
            print("[ApplovinMAX] Trigger Max Debugger");
            StartCoroutine(WaitToMaxInitializedForShowDebugger());
        }

        private IEnumerator WaitToMaxInitializedForShowDebugger()
        {
            print("[ApplovinMAX] Trigger Max Debugger waiting sdk initializing...");
            yield return new WaitUntil(() => MaxSdk.IsInitialized());
            print("[ApplovinMAX] Show Max Debugger");
            MaxSdk.ShowMediationDebugger();
        }
    }
}