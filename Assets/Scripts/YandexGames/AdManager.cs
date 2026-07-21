using System;
using Core;
using UnityEngine;
using YG;

public class AdManager : MonoBehaviour
{
    private void OnEnable()
    {
        GameEvents.ShowRewardedAd += ShowRewardedAd;
        GameEvents.OnGameOver += YG2.InterstitialAdvShow;
    }

    void OnDisable()
    {
        GameEvents.ShowRewardedAd -= ShowRewardedAd;
        GameEvents.OnGameOver -= YG2.InterstitialAdvShow;
    }

    private void ShowRewardedAd(string rewardId, Action callback) =>
        YG2.RewardedAdvShow(rewardId, callback??null);
}
