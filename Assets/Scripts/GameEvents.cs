using System;
using UnityEngine;

public static class GameEvents
{
    //Block Placement
    public static event Action<Block> OnBlockPicked;
    public static event Action OnBlockMoved;
    public static event Action<Block> OnBlockUnpicked;
    
    //UI
    public static event Action<int> UpdateScore;
    public static event Action<int, int> ShowCombo;
    
    //Game Flow
    public static event Action OnGameOver;
    public static event Action<ChangesAfterMove> CalculateNewScore;
    
    //SFX
    public static event Action<bool> SetSfxState;
    public static event Action<AudioClip> PlaySfx;

    public static void RaiseOnBlockPicked(Block block) =>
        OnBlockPicked?.Invoke(block);

    public static void RaiseOnBlockMoved() =>
        OnBlockMoved?.Invoke();

    public static void RaiseOnBlockUnpicked(Block block) =>
        OnBlockUnpicked?.Invoke(block);
    
    public static void RaiseGameOver() =>
        OnGameOver?.Invoke();

    public static void RaiseUpdateScore(int score) =>
        UpdateScore?.Invoke(score);

    public static void RaiseCalculateNewScore(ChangesAfterMove changes) =>
        CalculateNewScore?.Invoke(changes);
    
    /// <param name="state">true - on, false - off</param>
    public static void RaiseSetSfxState(bool state) =>
        SetSfxState?.Invoke(state);
    
    public static void RaisePlaySfx(AudioClip clip) =>
        PlaySfx?.Invoke(clip);

    public static void RaiseShowCombo(int combo, int lastCombo) =>
        ShowCombo?.Invoke(combo, lastCombo);
}
