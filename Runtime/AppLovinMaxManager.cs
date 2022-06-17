using System;
using System.Collections;
using com.adjust.sdk;
using pow.aidkit;
using pow.hermes;
using Unity.Notifications.iOS;
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
            MaxSdk.SetSdkKey("YOUR_SDK_KEY_HERE");

            MaxSdk.SetUserId("USER_ID");
            Adjust.addSessionCallbackParameter("YOUR_USER_ID_KEY", "USER_ID");

            MaxSdk.InitializeSdk();
            MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
            {
                // You can check app transparency tracking authorization in sdkConfiguration.AppTrackingStatus for Unity Editor and iOS targets.
                // Initialize other third-party SDKs; do not initialize mediated advertising SDKs (MAX does that for you). Not following this step will result in noticeable integration issues.

                // Initialize the Adjust SDK inside the AppLovin SDK's initialization callback
                string adjustAppToken = "{YOUR_ADJUST_APP_TOKEN}";
                AdjustEnvironment adjustEnvironment = AdjustEnvironment.Production;
                AdjustConfig config = new AdjustConfig(adjustAppToken, adjustEnvironment);

                Adjust.start(config);

                // Start loading ads
                // AppLovin SDK is initialized, start loading ads
                _interstitialController.InitializeInterstitialAds();
                _rewardedController.InitializeRewardedAds();
                _bannerController.InitializeBannerAds();

                MaxSdk.SetVerboseLogging(true);

                // TODO: Remove on release build
                MaxSdk.ShowMediationDebugger();
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
                    print("[ApplovinMAX] MaxSdkBase.ConsentDialogState.Applies not show consent dialog on ios");
                    StartCoroutine(NotificationsRequestAuthorization());
                    print("[ApplovinMAX] NotificationsRequestAuthorization on ios");
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

        // Called from OnAdjustInitiazlied game event
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

        private IEnumerator NotificationsRequestAuthorization()
        {
            var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge;
            using (var req = new AuthorizationRequest(authorizationOption, true))
            {
                if (BaseEventController.InstanceExists)
                    BaseEventController.Instance.SendNotificationPermissionViewEvent();
                yield return new WaitUntil(() => req.IsFinished);

                string res = "\n RequestAuthorization:";
                res += "\n finished: " + req.IsFinished;
                res += "\n granted :  " + req.Granted;
                res += "\n error:  " + req.Error;
                res += "\n deviceToken:  " + req.DeviceToken;
                Debug.Log(res);
                if (BaseEventController.InstanceExists)
                    BaseEventController.Instance.SendNotificationPermissionPassEvent(req.Granted);
            }
        }
    }
}