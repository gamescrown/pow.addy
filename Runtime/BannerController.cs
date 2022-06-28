using pow.hermes;

namespace pow.addy
{
    public class BannerController : BaseAdController
    {
        public void InitializeBannerAds()
        {
            print("[ApplovinMAX] InitializeBannerAds");
            // Banners are automatically sized to 320×50 on phones and 728×90 on tablets
            // You may call the utility method MaxSdkUtils.isTablet() to help with view sizing adjustments
            MaxSdk.CreateBanner(adID, MaxSdkBase.BannerPosition.BottomCenter);
            //MaxSdk.SetBannerExtraParameter(adID, "adaptive_banner", "true");

            // For adaptive banners
            MaxSdk.SetBannerExtraParameter(adID, "adaptive_banner", "true");

            // Set background or background color for banners to be fully functional
            //MaxSdk.SetBannerBackgroundColor(adID, Color.white);

            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnBannerAdExpandedEvent;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnBannerAdCollapsedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += AdRevenuePaidEvent;
        }

        private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            print("[ApplovinMAX] OnBannerAdLoadedEvent");
            double cpm = adInfo.Revenue * (1000 * 100);
            AdEventController.Instance.SendEcpmEvent(cpm);
            AdEventController.Instance.SendBannerLoadedEvent(adInfo.NetworkName);
        }

        private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            print("[ApplovinMAX] OnBannerAdLoadFailedEvent");
        }

        private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            print("[ApplovinMAX] OnBannerAdClickedEvent");
            AdEventController.Instance.SendBannerClickedEvent(adInfo.NetworkName);
        }

        private void OnBannerAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            print("[ApplovinMAX] OnBannerAdExpandedEvent");
        }

        private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            print("[ApplovinMAX] OnBannerAdCollapsedEvent");
        }

        public void ShowBanner()
        {
            print("[ApplovinMAX] ShowBanner");
            MaxSdk.ShowBanner(adID);
        }

        public void HideBanner()
        {
            print("[ApplovinMAX] HideBanner");
            MaxSdk.HideBanner(adID);
        }
    }
}