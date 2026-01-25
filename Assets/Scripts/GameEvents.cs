using System;
using UnityEngine;

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
    
    public static event Action LoadGame;
    public static void RaiseLoadGame() => LoadGame?.Invoke();

    #endregion
}
