using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action<Transform[]> PreviewCells;
    public static event Action HideCellsPreview;
    public static event Action<Block> OnBlockPicked;
    public static event Action OnBlockMoved;
    public static event Action<Block> OnBlockUnpicked;
    public static event Action OnGameOver;
    public static event Action RequestGameOverCheck;
    public static event Action<ChangesAfterMove> ChangesAfterMoveReport;

    public static void RaiseHideCellsPreview() =>
        HideCellsPreview?.Invoke();
    
    public static void RaisePreviewCells(Transform[] transforms) =>
        PreviewCells?.Invoke(transforms);

    public static void RaiseOnBlockPicked(Block block) =>
        OnBlockPicked?.Invoke(block);

    public static void RaiseOnBlockMoved() =>
        OnBlockMoved?.Invoke();

    public static void RaiseOnBlockUnpicked(Block block) =>
        OnBlockUnpicked?.Invoke(block);
    
    public static void RaiseGameOver() =>
        OnGameOver?.Invoke();

    public static void RaiseRequestGameOverCheck() =>
        RequestGameOverCheck?.Invoke();

    public static void RaiseChangesAfterMoveReport(ChangesAfterMove changes) =>
        ChangesAfterMoveReport?.Invoke(changes);
}
