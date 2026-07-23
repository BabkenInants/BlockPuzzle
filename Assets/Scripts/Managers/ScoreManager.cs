using System.Linq;
using UnityEngine;
using Core;
using Saves;
using YG;

namespace Managers
{
    public class ScoreManager : MonoBehaviour, ISavable
    {
        [SerializeField] private Settings settings;
        private int _score;
        private int _combo = -1;
        private int _comboReset;
        private int _bestScore;
        private bool _tutorialMode;
        private bool _newBestScore;

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

            //Combo and all clear bonuses
            if (!_tutorialMode)
            {
                //if removed at least one line within next 3 moves: combo++ && score += totalLines * totalLines * 100 * combo
                //else combo = 0
                if (totalLines > 0)
                {
                    _combo += totalLines;
                    _comboReset = 0;
                }
                else if (_combo > -1 && ++_comboReset >= settings.resetComboAfterMoves)
                {
                    _combo = -1;
                    _comboReset = 0;
                    GameEvents.RaiseOnComboEnded();
                }

                if (_combo > 0) _score += totalLines * totalLines * settings.comboScoreMultiplier * _combo;

                //All clear bonus
                if (changes.FieldIsAllClear)
                {
                    _score += settings.allClearBonus;
                    GameEvents.RaiseShowAllClearBonus();
                }
            }

            //Updating best score if needed
            _newBestScore = _score > _bestScore;
            if(_newBestScore) _bestScore = _score;
        
            //Updating UI
        
            //Score
            GameEvents.RaiseUpdateScore(_score, _newBestScore);
        
            //Combo
            if (_combo > 0 && totalLines > 0 && !_tutorialMode) 
                GameEvents.RaiseShowCombo(_combo, _combo - totalLines); //UI
        }

        private void UpdateLeaderBoard()
        {
            if(_newBestScore) YG2.SetLeaderboard("BestPlayers", _bestScore);
        }

        #region Events

        private void OnEnable()
        {
            GameEvents.StartTutorial += StartTutorial;
            GameEvents.FinishTutorial += EndTutorial;
            GameEvents.CalculateNewScore += UpdateScore;
            GameEvents.OnGameOver += UpdateLeaderBoard;
        }

        private void OnDisable()
        {
            GameEvents.StartTutorial -= StartTutorial;
            GameEvents.FinishTutorial -= EndTutorial;
            GameEvents.CalculateNewScore -= UpdateScore;
            GameEvents.OnGameOver -= UpdateLeaderBoard;
        }

        #endregion
    
        #region Tutorial

        private void StartTutorial() => _tutorialMode = true;

        private void EndTutorial() => _tutorialMode = false;
        
        #endregion
        
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