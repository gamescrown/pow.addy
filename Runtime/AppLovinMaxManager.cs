using System;
using pow.aidkit;
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
                // AppLovin SDK is initialized, start loading ads

                _interstitialController.InitializeInterstitialAds();
                _rewardedController.InitializeRewardedAds();
                _bannerController.InitializeBannerAds();

                MaxSdk.SetVerboseLogging(true);

                // TODO: Remove on release build
                MaxSdk.ShowMediationDebugger();


//                if (sdkConfiguration.ConsentDialogState == MaxSdkBase.ConsentDialogState.Applies)
//                {
//#if UNITY_ANDROID
//                    MaxSdk.UserService.ShowConsentDialog();
//#endif
//                }
//                else if (sdkConfiguration.ConsentDialogState == MaxSdkBase.ConsentDialogState.DoesNotApply)
//                {
//                    // No need to show consent dialog, proceed with initialization
//                }
            };

            MaxSdk.InitializeSdk();
        }

        public void ShowInterstitial(Action onCompleteAction, string tag)
        {
            adEventHandler.SetInterstitialCompletedAction(onCompleteAction);
            _interstitialController.ShowInterstitial(tag);
        }

        public void ShowRewarded(Action onCompletedAction, Action onFailedAction, string tag)
        {
            adEventHandler.SetRewardedCompletedAction(onCompletedAction);
            adEventHandler.SetRewardedFailedAction(onFailedAction);
            _rewardedController.ShowRewardedAd(tag);
        }

        public void ShowBanner()
        {
            _bannerController.ShowBanner();
        }

        public void HideBanner()
        {
            _bannerController.HideBanner();
        }
    }
}