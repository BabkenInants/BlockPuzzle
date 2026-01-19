using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action<Block> OnBlockPicked;
    public static event Action OnBlockMoved;
    public static event Action<Block> OnBlockUnpicked;
    public static event Action OnGameOver;
    public static event Action CheckGameOver;
    public static event Action<int> UpdateScore;
    public static event Action<ChangesAfterMove> CalculateNewScore;

    public static void RaiseOnBlockPicked(Block block) =>
        OnBlockPicked?.Invoke(block);

    public static void RaiseOnBlockMoved() =>
        OnBlockMoved?.Invoke();

    public static void RaiseOnBlockUnpicked(Block block) =>
        OnBlockUnpicked?.Invoke(block);
    
    public static void RaiseGameOver() =>
        OnGameOver?.Invoke();

    public static void RaiseCheckGameOver() =>
        CheckGameOver?.Invoke();

    public static void RaiseUpdateScore(int score) =>
        UpdateScore?.Invoke(score);

    public static void RaiseCalculateNewScore(ChangesAfterMove obj) =>
        CalculateNewScore?.Invoke(obj);
}
