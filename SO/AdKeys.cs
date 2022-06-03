using UnityEngine;

namespace pow.addy
{
    [CreateAssetMenu(fileName = "AdKeys", menuName = "POW_SDK/AdKeys", order = 0)]
    public class AdKeys : ScriptableObject
    {
        [Header("IOS")] [SerializeField] internal string bannerIdIOS;
        [SerializeField] internal string rewardedIdIOS;
        [SerializeField] internal string interstitialIdIOS;
        [Header("ANDROID")] [SerializeField] internal string bannerIdAndroid;
        [SerializeField] internal string rewardedIdAndroid;
        [SerializeField] internal string interstitialIdAndroid;
    }
}