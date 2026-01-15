using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private Settings settings;
    private int _score = 0;

    private void UpdateScore(ChangesAfterMove changes)
    {
        _score += changes.BlockCellsPositions.Length;
        
        var rowsScore = 0;
        var colScore = 0;
        foreach(bool row in changes.FullRows)
            if(row) rowsScore++;
        foreach(bool col in changes.FullCols)
            if(col) colScore++;
        _score += rowsScore * 10 * settings.columnsCount;
        _score += colScore * 10 * settings.rowsCount;
        
        int totalLines = rowsScore + colScore;
        if (totalLines > 1) _score += totalLines * totalLines * 50;
        
        GameEvents.RaiseUpdateScore(_score);
    }
    
    private void OnEnable() =>
        GameEvents.CalculateNewScore += UpdateScore;

    private void OnDisable() =>
        GameEvents.CalculateNewScore -= UpdateScore;
}