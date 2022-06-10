using System;
using pow.aidkit;
using pow.hermes;
using UnityEngine;

namespace pow.addy
{
    public class RewardedController : BaseAdController
    {
        // TODO: Call this events for static functions
        [SerializeField] private GameEvent onRewardedAdDisplayed;
        [SerializeField] private GameEvent onRewardedAdCompleted;
        [SerializeField] private GameEvent onRewardedAdFailed;
        private int _retryAttempt;
        private string _latestRewardedVideoTag;

        public void InitializeRewardedAds()
        {
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
            MaxSdk.LoadRewardedAd(adID);
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad is ready for you to show. MaxSdk.IsRewardedAdReady(adUnitId) now returns 'true'.
            AdEventController.Instance.SendRewardedVideoLoadedEvent("null", RewardedVideoTag.untagged);
            double ecpm = adInfo.Revenue * (1000 * 100);
            AdEventController.Instance.SendEcpmEvent(ecpm);
            AdEventController.Instance.SendEcpmEventExcludeBanner(ecpm);
            // Reset retry attempt
            _retryAttempt = 0;
        }

        private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Rewarded ad failed to load 
            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds).

            _retryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, _retryAttempt));

            Invoke("LoadRewardedAd", (float) retryDelay);
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            AdEventController.Instance.SendRewardedVideoDisplayedEvent(adInfo.NetworkName, _latestRewardedVideoTag);
            // start timer for this ad identifier
            //GameAnalytics.StartTimer(_latestRewardedVideoTag.ToString());
            //_isSoundAlreadyOn = settings.IsMusicOn;
            //Debug.Log("Is sound already on " + _isSoundAlreadyOn);
            //if (settings.IsMusicOn) settings.ToggleMusicWithoutSaving();
            //interstitialAdTracker.DelayRewardedToInt();
        }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo,
            MaxSdkBase.AdInfo adInfo)
        {
            AdEventController.Instance.SendRewardedVideoFailedShowEvent(adInfo.NetworkName,
                _latestRewardedVideoTag);
            // Rewarded ad failed to display. AppLovin recommends that you load the next ad.
            LoadRewardedAd();
        }

        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            AdEventController.Instance.SendRewardedVideoClickedEvent(adInfo.NetworkName, _latestRewardedVideoTag);
        }

        private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad is hidden. Pre-load the next ad
            LoadRewardedAd();
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdkBase.Reward reward,
            MaxSdkBase.AdInfo adInfo)
        {
            AdEventController.Instance.SendRewardedVideoReceivedRewardEvent(adInfo.NetworkName,
                _latestRewardedVideoTag);
            if (
                _latestRewardedVideoTag == RewardedVideoTag.ingame_booster_popup_use_hint_rew.ToString() ||
                _latestRewardedVideoTag == RewardedVideoTag.ingame_booster_popup_use_shuffle_rew.ToString() ||
                _latestRewardedVideoTag == RewardedVideoTag.level_fail_popup_clean_all_slots_rew.ToString() ||
                _latestRewardedVideoTag == RewardedVideoTag.level_fail_popup_times_up_extra_time_rew.ToString()
            )
            {
                double ecpm = adInfo.Revenue * (1000 * 100);
                AdEventController.Instance.SendEcpmEventOnlyRewarded(ecpm);
            }

            onRewardedAdCompleted?.Invoke();
            adEventHandler.RaiseRewardedAdCompleteEvent();
            //GiveReward();
            //if (_isSoundAlreadyOn) settings.ToggleMusicWithoutSaving();

            // The rewarded ad displayed and the user should receive the reward.
            print("Rewarded user: " + reward.Amount + " " + reward.Label);
        }

        public void ShowRewardedAd(string tag)
        {
            if (!MaxSdk.IsRewardedAdReady(adID)) return;
            _latestRewardedVideoTag = tag;
            MaxSdk.ShowRewardedAd(adID);
        }
    }
}