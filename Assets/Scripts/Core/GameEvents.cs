using System;
using UnityEngine;
using Managers;
using Tutorial;

namespace Core
{
    public static class GameEvents
    {
        #region Block Placement
    
        public static event Action<Block> OnBlockPicked;
        public static void RaiseOnBlockPicked(Block block) => OnBlockPicked?.Invoke(block);
    
        public static event Action OnBlockMoved;
        public static void RaiseOnBlockMoved() => OnBlockMoved?.Invoke();
    
        public static event Action<Block> OnBlockUnpicked;
        public static void RaiseOnBlockUnpicked(Block block) => OnBlockUnpicked?.Invoke(block);
    
        #endregion

        #region UI
    
        public static event Action<int, bool> UpdateScore;
        public static void RaiseUpdateScore(int score, bool updateBestScore) => UpdateScore?.Invoke(score, updateBestScore);
    
        public static event Action<int, int> ShowCombo;
        public static void RaiseShowCombo(int combo, int lastCombo) => ShowCombo?.Invoke(combo, lastCombo);
    
        public static event Action ShowAllClearBonus;
        public static void RaiseShowAllClearBonus() => ShowAllClearBonus?.Invoke();

        public static event Action OnComboEnded;
        public static void RaiseOnComboEnded() => OnComboEnded?.Invoke();
    
        #endregion

        #region Game Flow

        public static event Action OnReviveSuggestion;
        public static void RaiseOnReviveSuggestion() => OnReviveSuggestion?.Invoke();
        
        public static event Action OnGameOver;
        public static void RaiseGameOver() => OnGameOver?.Invoke();
    
        public static event Action<ChangesAfterMove> CalculateNewScore;
        public static void RaiseCalculateNewScore(ChangesAfterMove changes) => CalculateNewScore?.Invoke(changes);
    
        #endregion

        #region SFX
    
        public static event Action<bool> SetSfxState;
        /// <param name="state">true - on, false - off</param>
        public static void RaiseSetSfxState(bool state) => SetSfxState?.Invoke(state);
    
        public static event Action<AudioClip> PlaySfx;
        public static void RaisePlaySfx(AudioClip clip) => PlaySfx?.Invoke(clip);
    
        #endregion

        #region Saves

        public static event Action SaveGame;
        public static void RaiseSaveGame() => SaveGame?.Invoke();

        public static event Action SaveGameForRestart;
        public static void RaiseSaveGameForRestart() => SaveGameForRestart?.Invoke();
    
        public static event Action LoadGame;
        public static void RaiseLoadGame() => LoadGame?.Invoke();

        #endregion

        #region Haptics
    
        /// true - on, false - off
        public static event Action<bool> SetHapticsState;
        public static void RaiseSetHapticsState(bool state) => SetHapticsState?.Invoke(state);
        public static event Action<HapticManager.HapticType> PlayHaptics;
        public static void RaisePlayHaptics(HapticManager.HapticType type) => PlayHaptics?.Invoke(type);
        public static event Action<HapticManager.HapticType, int> PlayHapticsInARow;
        public static void RaisePlayHapticsInARow(HapticManager.HapticType type, int amount) => PlayHapticsInARow?.Invoke(type, amount);

        #endregion

        #region Themes

        public static event Action SetNextTheme;
        public static void RaiseSetNextTheme() => SetNextTheme?.Invoke();

        #endregion

        #region Tutorial

        public static event Action StartTutorial;
        public static void RaiseStartTutorial() => StartTutorial?.Invoke();
        
        public static event Action FinishTutorial;
        public static void RaiseFinishTutorial() => FinishTutorial?.Invoke();

        public static event Action<TutorialExample> LoadTutorialExample;
        public static void RaiseLoadTutorialExample(TutorialExample example) => LoadTutorialExample?.Invoke(example);

        public static event Action OnTutorialExampleCompleted;
        public static void RaiseOnTutorialExampleCompleted() => OnTutorialExampleCompleted?.Invoke();

        #endregion

        #region Ads

        public static event Action<string, Action> ShowRewardedAd;
        public static void RaiseShowRewardedAd(string rewardId, Action callback) => ShowRewardedAd?.Invoke(rewardId, callback);

        public static event Action SpawnNewBlocksForRevival;
        public static void RaiseSpawnNewBlocksForRevival() => SpawnNewBlocksForRevival?.Invoke();

        #endregion
    }
}
