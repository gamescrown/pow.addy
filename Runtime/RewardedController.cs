using System;
using pow.hermes;

namespace pow.addy
{
    public class RewardedController : BaseAdController
    {
        private int _retryAttempt;
        private string _latestRewardedVideoTag;

        public void InitializeRewardedAds()
        {
            print("[ApplovinMAX] InitializeRewardedAds");
            // Attach callback
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += AdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;

            // Load the first rewarded ad
            LoadRewardedAd();
        }

        private void LoadRewardedAd()
        {
            print("[ApplovinMAX] LoadRewardedAd");
            MaxSdk.LoadRewardedAd(adID);
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            print("[ApplovinMAX] OnRewardedAdLoadedEvent");
            // Rewarded ad is ready for you to show. MaxSdk.IsRewardedAdReady(adUnitId) now returns 'true'.
            AdEventController.Instance.SendRewardedVideoLoadedEvent("null", RewardedVideoTag.untagged);
            double cpm = adInfo.Revenue * (1000 * 100);
            AdEventController.Instance.SendEcpmEvent(cpm);
            AdEventController.Instance.SendEcpmEventExcludeBanner(cpm);
            // Reset retry attempt
            _retryAttempt = 0;
        }

        private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            print("[ApplovinMAX] OnRewardedAdLoadFailedEvent");
            // Rewarded ad failed to load 
            // Applovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds).

            _retryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, _retryAttempt));

            Invoke(nameof(LoadRewardedAd), (float) retryDelay);
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            print("[ApplovinMAX] OnRewardedAdDisplayedEvent");
            AdEventController.Instance.SendRewardedVideoDisplayedEvent(adInfo.NetworkName, _latestRewardedVideoTag);
            //_isSoundAlreadyOn = settings.IsMusicOn;
            //if (settings.IsMusicOn) settings.ToggleMusicWithoutSaving();
            //interstitialAdTracker.DelayRewardedToInt();
        }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo,
            MaxSdkBase.AdInfo adInfo)
        {
            print("[ApplovinMAX] OnRewardedAdFailedToDisplayEvent");
            adEventHandler.RaiseRewardedAdFailedEvent();
            AdEventController.Instance.SendRewardedVideoFailedShowEvent(adInfo.NetworkName,
                _latestRewardedVideoTag);
            // Rewarded ad failed to display. Applovin recommends that you load the next ad.
            LoadRewardedAd();
        }

        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            print("[ApplovinMAX] OnRewardedAdClickedEvent");
            AdEventController.Instance.SendRewardedVideoClickedEvent(adInfo.NetworkName, _latestRewardedVideoTag);
        }

        private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            print("[ApplovinMAX] OnRewardedAdHiddenEvent");
            // Rewarded ad is hidden. Pre-load the next ad
            LoadRewardedAd();
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdkBase.Reward reward,
            MaxSdkBase.AdInfo adInfo)
        {
            print("[ApplovinMAX] OnRewardedAdReceivedRewardEvent");
            AdEventController.Instance.SendRewardedVideoReceivedRewardEvent(adInfo.NetworkName,
                _latestRewardedVideoTag);
            if (
                _latestRewardedVideoTag == RewardedVideoTag.ingame_booster_popup_use_hint_rew.ToString() ||
                _latestRewardedVideoTag == RewardedVideoTag.ingame_booster_popup_use_shuffle_rew.ToString() ||
                _latestRewardedVideoTag == RewardedVideoTag.level_fail_popup_clean_all_slots_rew.ToString() ||
                _latestRewardedVideoTag == RewardedVideoTag.level_fail_popup_times_up_extra_time_rew.ToString()
            )
            {
                double cpm = adInfo.Revenue * (1000 * 100);
                AdEventController.Instance.SendEcpmEventOnlyRewarded(cpm);
            }

            adEventHandler.RaiseRewardedAdCompleteEvent();
            //if (_isSoundAlreadyOn) settings.ToggleMusicWithoutSaving();

            // The rewarded ad displayed and the user should receive the reward.
            print("Rewarded user: " + reward.Amount + " " + reward.Label);
        }

        public void ShowRewardedAd(string rewardedTag)
        {
            print("[ApplovinMAX] ShowRewardedAd");
            if (!MaxSdk.IsRewardedAdReady(adID))
            {
                print("[ApplovinMAX] Rewarded video ad not ready");
                adEventHandler.RaiseRewardedAdFailedEvent();
                return;
            }

            _latestRewardedVideoTag = rewardedTag;
            MaxSdk.ShowRewardedAd(adID, rewardedTag);
        }
    }
}