using pow.hermes;
using UnityEngine;

namespace pow.addy
{
    public class BaseAdController : MonoBehaviour
    {
        [SerializeField] protected AdEventHandler adEventHandler;

        protected string adID;

        internal void SetAdID(string id)
        {
            adID = id;
        }

        internal void AdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
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
    }
}