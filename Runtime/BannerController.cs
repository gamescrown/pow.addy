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
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += AdRevenuePaidEvent;
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