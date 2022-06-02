using System;
using UnityEngine;

namespace PowSDK.Addy.SO
{
    [CreateAssetMenu(fileName = "AdEventHandler", menuName = "POW_SDK/Addy", order = 0)]
    public class AdEventHandler : ScriptableObject
    {
        private Action onInterstitialAdCompleted;
        private Action onRewardedAdCompleted;

        public void SetInterstitialCompletedAction(Action action)
        {
            onInterstitialAdCompleted = action;
        }

        public void SetRewardedCompletedAction(Action action)
        {
            onRewardedAdCompleted = action;
        }

        public void RaiseInterstitialAdCompleteEvent()
        {
            onInterstitialAdCompleted?.Invoke();
        }

        public void RaiseRewardedAdCompleteEvent()
        {
            onRewardedAdCompleted?.Invoke();
        }
    }
}