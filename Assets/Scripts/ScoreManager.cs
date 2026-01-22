using System.Linq;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private Settings settings;
    private int _score;
    private int _combo = -1;
    private int _comboReset;

    private void UpdateScore(ChangesAfterMove changes)
    {
        //score += blockCellsCount
        _score += changes.BlockCellsPositions.Length;
        
        //for each removed line: score += (10 * lineLength)
        int rowsScore = changes.FullRows.Count(row => row);
        int colScore = changes.FullCols.Count(col => col);
        _score += rowsScore * 10 * settings.columnsCount;
        _score += colScore * 10 * settings.rowsCount;
        
        //if multiple lines removed: score += linesRemoved * linesRemoved * 50
        int totalLines = rowsScore + colScore;
        if (totalLines > 1) _score += totalLines * totalLines * 50;
        
        //if removed at least one line within next 3 moves: combo++ && score += totalLines * totalLines * 100 * combo
        //else combo = 0
        if (totalLines > 0)
        {
            _combo += totalLines;
            print(_combo);
            _comboReset = 0;
        }
        else if (_combo > -1 && ++_comboReset >= 2) { _combo = -1; _comboReset = 0; }
        if (_combo > 0) _score += totalLines * totalLines * 50 * _combo;
        
        //Updating UI
        
        //score
        GameEvents.RaiseUpdateScore(_score);
        
        //combo
        if (_combo > 0 && totalLines > 0) 
            GameEvents.RaiseShowCombo(_combo, _combo - totalLines); //UI
    }
    
    private void OnEnable() =>
        GameEvents.CalculateNewScore += UpdateScore;

    private void OnDisable() =>
        GameEvents.CalculateNewScore -= UpdateScore;
}