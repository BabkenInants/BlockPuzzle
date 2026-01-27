using System.Linq;
using UnityEngine;
using Core;
using Saves;

namespace Managers
{
    public class ScoreManager : MonoBehaviour, ISavable
    {
        [SerializeField] private Settings settings;
        private int _score;
        private int _combo = -1;
        private int _comboReset;
        private int _bestScore;

        private void UpdateScore(ChangesAfterMove changes)
        {
            //score += blockCellsCount
            _score += changes.BlockCellsPositions.Length;
        
            //for each removed line: score += (multiplier * lineLength)
            int rowsScore = changes.FullRows.Count(row => row);
            int colScore = changes.FullCols.Count(col => col);
            _score += rowsScore * settings.lineRemovalScoreMultiplier * settings.columnsCount;
            _score += colScore * settings.lineRemovalScoreMultiplier * settings.rowsCount;
        
            //if multiple lines removed: score += linesRemoved * linesRemoved * multiplier
            int totalLines = rowsScore + colScore;
            if (totalLines > 1) _score += totalLines * totalLines * settings.multipleLinesRemovalScoreMultiplier;
        
            //if removed at least one line within next 3 moves: combo++ && score += totalLines * totalLines * 100 * combo
            //else combo = 0
            if (totalLines > 0)
            {
                _combo += totalLines;
                _comboReset = 0;
            }
            else if (_combo > -1 && ++_comboReset >= settings.resetComboAfterMoves) { _combo = -1; _comboReset = 0; GameEvents.RaiseOnComboEnded();}
            if (_combo > 0) _score += totalLines * totalLines * settings.comboScoreMultiplier * _combo;
        
            //All clear bonus
            if (changes.FieldIsAllClear)
            {
                _score += settings.allClearBonus;
                GameEvents.RaiseShowAllClearBonus();
            }
        
            //Updating best score if needed
            bool updateBestScore = _score > _bestScore;
            if(updateBestScore) _bestScore = _score;
        
            //Updating UI
        
            //Score
            GameEvents.RaiseUpdateScore(_score, updateBestScore);
        
            //Combo
            if (_combo > 0 && totalLines > 0) 
                GameEvents.RaiseShowCombo(_combo, _combo - totalLines); //UI
        }

        private void OnEnable() => GameEvents.CalculateNewScore += UpdateScore;

        private void OnDisable() => GameEvents.CalculateNewScore -= UpdateScore;
    
        #region Saves

        public void Save(SaveData saveData)
        {
            saveData.BestScore = _bestScore;
            if(saveData.GameIsOver) return;
            saveData.Score = _score;
            saveData.Combo = _combo;
            saveData.ComboReset = _comboReset;
        }

        public void Load(SaveData saveData)
        {
            _bestScore = saveData.BestScore;
            if(saveData.GameIsOver) return;
            _score = saveData.Score;
            _combo = saveData.Combo;
            _comboReset = saveData.ComboReset;
        }
    
        #endregion
    }
}