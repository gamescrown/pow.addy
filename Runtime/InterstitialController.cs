using System;
using pow.hermes;

namespace pow.addy
{
    public class InterstitialController : BaseAdController
    {
        private int _retryAttempt;
        private string _interstitialNetworkName;
        private string _latestInterstitialTag;

        public void InitializeInterstitialAds()
        {
            print("[ApplovinMAX] InitializeInterstitialAds");
            // Attach callback
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += AdRevenuePaidEvent;

            // Load the first interstitial
            LoadInterstitial();
        }

        private void LoadInterstitial()
        {
            print("[ApplovinMAX] LoadInterstitial");
            MaxSdk.LoadInterstitial(adID);
        }

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            print("[ApplovinMAX] OnInterstitialLoadedEvent");
            // Interstitial ad is ready for you to show. MaxSdk.IsInterstitialReady(adUnitId) now returns 'true'

            print("Waterfall Name: " + adInfo.WaterfallInfo.Name + " and Test Name: " + adInfo.WaterfallInfo.TestName);
            print("Waterfall latency was: " + adInfo.WaterfallInfo.LatencyMillis + " milliseconds");

            string waterfallInfoStr = "";
            foreach (var networkResponse in adInfo.WaterfallInfo.NetworkResponses)
            {
                waterfallInfoStr = "Network -> " + networkResponse.MediatedNetwork +
                                   "\n...adLoadState: " + networkResponse.AdLoadState +
                                   "\n...latency: " + networkResponse.LatencyMillis + " milliseconds" +
                                   "\n...credentials: " + networkResponse.Credentials;

                if (networkResponse.Error != null)
                {
                    waterfallInfoStr += "\n...error: " + networkResponse.Error;
                }
            }

            print(waterfallInfoStr);

            AdEventController.Instance.SendInterstitialLoadEvent(
                adInfo.NetworkName,
                InterstitialTag.untagged.ToString()
            );
            double cpm = adInfo.Revenue * (1000 * 100);
            AdEventController.Instance.SendEcpmEvent(cpm);
            AdEventController.Instance.SendEcpmEventExcludeBanner(cpm);

            // Reset retry attempt
            _retryAttempt = 0;
        }

        private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            print("[ApplovinMAX] OnInterstitialLoadFailedEvent");
            // Interstitial ad failed to load 
            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds)

            print("Waterfall Name: " + errorInfo.WaterfallInfo.Name + " and Test Name: " +
                  errorInfo.WaterfallInfo.TestName);
            print("Waterfall latency was: " + errorInfo.WaterfallInfo.LatencyMillis + " milliseconds");

            foreach (var networkResponse in errorInfo.WaterfallInfo.NetworkResponses)
            {
                print("Network -> " + networkResponse.MediatedNetwork +
                      "\n...latency: " + networkResponse.LatencyMillis + " milliseconds" +
                      "\n...credentials: " + networkResponse.Credentials +
                      "\n...error: " + networkResponse.Error);
            }

            _retryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, _retryAttempt));

            Invoke(nameof(LoadInterstitial), (float) retryDelay);
        }

        private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            print("[ApplovinMAX] OnInterstitialDisplayedEvent");
            AdEventController.Instance.SendInterstitialShowEvent(adInfo.NetworkName, _latestInterstitialTag);
        }

        private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo,
            MaxSdkBase.AdInfo adInfo)
        {
            print("[ApplovinMAX] OnInterstitialAdFailedToDisplayEvent");
            // TODO: Send error info to analytics with parameter
            AdEventController.Instance.SendInterstitialFailedShowEvent(adInfo.NetworkName, _latestInterstitialTag);
            // Interstitial ad failed to display. AppLovin recommends that you load the next ad.
            LoadInterstitial();
        }

        private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            print("[ApplovinMAX] OnInterstitialClickedEvent");
            AdEventController.Instance.SendInterstitialClickedEvent(adInfo.NetworkName, _latestInterstitialTag);
        }

        private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            print("[ApplovinMAX] OnInterstitialHiddenEvent");
            adEventHandler.RaiseInterstitialAdCompleteEvent();
            // Interstitial ad is hidden. Pre-load the next ad.
            LoadInterstitial();
        }

        public void ShowInterstitial(string interstitialTag)
        {
            print("[ApplovinMAX] ShowInterstitial");
            if (!MaxSdk.IsInterstitialReady(adID)) return;
            _latestInterstitialTag = interstitialTag;
            MaxSdk.ShowInterstitial(adID, interstitialTag);
        }
    }
}