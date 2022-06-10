using pow.hermes;
using UnityEngine;

namespace pow.addy
{
    public class BannerController : BaseAdController
    {
        // Retrieve the ID from your account

        public void InitializeBannerAds()
        {
            // Banners are automatically sized to 320×50 on phones and 728×90 on tablets
            // You may call the utility method MaxSdkUtils.isTablet() to help with view sizing adjustments
            MaxSdk.CreateBanner(adID, MaxSdkBase.BannerPosition.BottomCenter);

            // For adaptive banners
            //MaxSdk.SetBannerExtraParameter(bannerAdUnitId, "adaptive_banner", "true");

            // Set background or background color for banners to be fully functional
            MaxSdk.SetBannerBackgroundColor(adID, Color.white);

            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnBannerAdExpandedEvent;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnBannerAdCollapsedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
        }

        private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            double ecpm = adInfo.Revenue * (1000 * 100);
            AdEventController.Instance.SendEcpmEvent(ecpm);
            AdEventController.Instance.SendBannerLoadedEvent(adInfo.NetworkName);
        }

        private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
        }

        private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            AdEventController.Instance.SendBannerClickedEvent(adInfo.NetworkName);
        }

        private void OnBannerAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
        }

        private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
        }

        private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
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
            EventSender.AdjustApplovinAdRevenueEvent(revenue, networkName, adUnitIdentifier, placement);
        }

        public void ShowBanner()
        {
            MaxSdk.ShowBanner(adID);
        }

        public void HideBanner()
        {
            MaxSdk.HideBanner(adID);
        }
    }
}