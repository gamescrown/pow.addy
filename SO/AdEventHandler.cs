using System;
using UnityEngine;

namespace pow.addy
{
    [CreateAssetMenu(fileName = "AdEventHandler", menuName = "POW_SDK/Addy/AdEventHandler", order = 0)]
    public class AdEventHandler : ScriptableObject
    {
        private Action onInterstitialAdCompleted;
        private Action onRewardedAdCompleted;
        private Action onRewardedAdFailed;

        public void SetInterstitialCompletedAction(Action action)
        {
            onInterstitialAdCompleted = action;
        }

        public void SetRewardedCompletedAction(Action action)
        {
            onRewardedAdCompleted = action;
        }

        public void SetRewardedFailedAction(Action action)
        {
            onRewardedAdFailed = action;
        }

        public void RaiseInterstitialAdCompleteEvent()
        {
            onInterstitialAdCompleted?.Invoke();
            onInterstitialAdCompleted = null;
        }

        public void RaiseRewardedAdCompleteEvent()
        {
            onRewardedAdCompleted?.Invoke();
            onRewardedAdCompleted = null;
        }

        public void RaiseRewardedAdFailedEvent()
        {
            onRewardedAdFailed?.Invoke();
            onRewardedAdFailed = null;
        }
    }
}