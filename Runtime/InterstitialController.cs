using System;
using UnityEngine;

namespace pow.addy
{
    public class InterstitialController : BaseAdController
    {
        private int _retryAttempt;
        
        public void InitializeInterstitialAds()
        {
            // Attach callback
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterstitialAdRevenuePaidEvent;

            // Load the first interstitial
            LoadInterstitial();
        }

        private void LoadInterstitial()
        {
            MaxSdk.LoadInterstitial(adID);
        }

        private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
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

            // Reset retry attempt
            _retryAttempt = 0;
        }

        private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
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

            Invoke("LoadInterstitial", (float) retryDelay);
        }

        private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
        }

        private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo,
            MaxSdkBase.AdInfo adInfo)
        {
            // Interstitial ad failed to display. AppLovin recommends that you load the next ad.
            LoadInterstitial();
        }

        private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
        }

        private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            adEventHandler.RaiseInterstitialAdCompleteEvent();
            // Interstitial ad is hidden. Pre-load the next ad.
            LoadInterstitial();
        }

        private void OnInterstitialAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            double revenue = adInfo.Revenue;

            // Miscellaneous data
            string
                countryCode =
                    MaxSdk.GetSdkConfiguration()
                        .CountryCode; // "US" for the United States, etc - Note: Do not confuse this with currency code which is "USD" in most cases!
            string networkName = adInfo.NetworkName; // Display name of the network that showed the ad (e.g. "AdColony")
            string adUnitIdentifier = adInfo.AdUnitIdentifier; // The MAX Ad Unit ID
            string placement = adInfo.Placement; // The placement this ad's postbacks are tied to
            string networkPlacement = adInfo.NetworkPlacement; // The placement ID from the network that showed the ad
        }

        public void ShowInterstitial()
        {
            if (MaxSdk.IsInterstitialReady(adID))
            {
                MaxSdk.ShowInterstitial(adID);
            }
        }
    }
}