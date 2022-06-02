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
        private InterstitialController _interstitialController;
        private RewardedController _rewardedController;
        private BannerController _bannerController;


        private void Awake()
        {
            _rewardedController = GetComponent<RewardedController>();
            _interstitialController = GetComponent<InterstitialController>();
            _bannerController = GetComponent<BannerController>();
        }

        void Start()
        {
            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
            {
                // AppLovin SDK is initialized, start loading ads

                _interstitialController.InitializeInterstitialAds();
                _rewardedController.InitializeRewardedAds();
                _bannerController.InitializeBannerAds();
                MaxSdk.SetVerboseLogging(true);
                MaxSdk.ShowMediationDebugger();


                if (sdkConfiguration.ConsentDialogState == MaxSdkBase.ConsentDialogState.Applies)
                {
#if UNITY_ANDROID
                    MaxSdk.UserService.ShowConsentDialog();
#endif
                }
                else if (sdkConfiguration.ConsentDialogState == MaxSdkBase.ConsentDialogState.DoesNotApply)
                {
                    // No need to show consent dialog, proceed with initialization
                }
            };

            MaxSdk.SetSdkKey("YOUR_SDK_KEY_HERE");
            MaxSdk.SetUserId("USER_ID");
            MaxSdk.InitializeSdk();
        }

        public void ShowInterstitial(Action onCompleteAction)
        {
            adEventHandler.SetInterstitialCompletedAction(onCompleteAction);
            _interstitialController.ShowInterstitial();
        }

        public void ShowRewarded(Action onCompleteAction)
        {
            adEventHandler.SetRewardedCompletedAction(onCompleteAction);
            _rewardedController.ShowRewardedAd();
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