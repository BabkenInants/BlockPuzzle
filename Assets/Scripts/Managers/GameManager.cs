using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core;
using Tutorial;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Field field;
        [SerializeField] private FieldGraphics fieldGraphics;
        [SerializeField] private BlockSpawner blockSpawner;
        [SerializeField] private Settings settings;
        [SerializeField] private CameraShakeManager cameraShakeManager;
        private Block _pickedBlock;
        private bool _gameIsOver;
        private GridPos[] _lastValidPos;
        private bool _tutorialMode;
        private TutorialExample _tutorialExample;

        private IEnumerator Start()
        {
            Application.targetFrameRate = (int) Screen.currentResolution.refreshRateRatio.value;
            while(!field.isReady || !fieldGraphics.isReady) yield return null;
            field.InitFirstCell(fieldGraphics.firstCell);
        }

        #region Block Placement

        private void OnBlockPicked(Block block)
        {
            if (_gameIsOver) return;
            _pickedBlock = block;
            GameEvents.RaisePlaySfx(settings.blockPickupSfx);
        }

        private void OnBlockMoved()
        {
            if (!_pickedBlock || _gameIsOver) return;
            
            HidePreviews();
            
            if (!field.CheckIfBlockCanBePlaced(_pickedBlock.cells, out GridPos[] cellsPositions))
            {
                if (_lastValidPos == null) return;
                
                int lastValidMinRow = _lastValidPos.Min(pos => pos.row);
                int lastValidMinCol = _lastValidPos.Min(pos => pos.column);
                int currMinRow = cellsPositions.Min(pos => pos.row);
                int currMinCol = cellsPositions.Min(pos => pos.column);
                int deltaRow = currMinRow - lastValidMinRow;
                int deltaCol = currMinCol - lastValidMinCol;
                
                //if block was dragged from last valid pos more than 1 cell by diagonal preview disappears
                if (Math.Abs(deltaRow) > 1 || Math.Abs(deltaCol) > 1)
                {
                    _lastValidPos = null;
                    return;
                }
                
                //sifting block in moved directions by delta to see if it fits somewhere
                int minRowAfterShift = lastValidMinRow + deltaRow;
                int maxRowAfterShift = _lastValidPos.Max(pos => pos.row) + deltaRow;
                int minColAfterShift = lastValidMinCol + deltaCol;
                int maxColAfterShift = _lastValidPos.Max(pos => pos.column) + deltaCol;

                var foundPos = false;
                
                //rows
                if (minRowAfterShift >= 0 && maxRowAfterShift < settings.rowsCount)
                {
                    if (FieldUtils.CheckIfBlockCanBePlacedAtCell(field.cellIsFree, _pickedBlock,
                            minRowAfterShift, lastValidMinCol))
                    {
                        foundPos = true;
                        for (var i = 0; i < _lastValidPos.Length; i++)
                            _lastValidPos[i].row += deltaRow;
                    }
                }
                
                //cols
                if (!foundPos && minColAfterShift >= 0 && maxColAfterShift < settings.columnsCount)
                {
                    if (FieldUtils.CheckIfBlockCanBePlacedAtCell(field.cellIsFree, _pickedBlock,
                            lastValidMinRow, minColAfterShift))
                    {
                        for (var i = 0; i < _lastValidPos.Length; i++)
                            _lastValidPos[i].column += deltaCol;
                    }
                }
                ShowPreviews();
                return;
            }

            _lastValidPos = cellsPositions.ToArray();
            
            //in tutorial the block can be placed only in correct place
            if (_tutorialMode)
            {
                int lastValidMinRow = _lastValidPos.Min(pos => pos.row);
                int lastValidMinCol = _lastValidPos.Min(pos => pos.column);
                if (_tutorialExample.targetPos != new GridPos(lastValidMinRow, lastValidMinCol))
                {
                    _lastValidPos = null;
                    return;
                }
            }
            
            ShowPreviews();
        }

        private void OnBlockUnpicked(Block block)
        {
            HidePreviews();
            if (!block || _gameIsOver || _pickedBlock != block) { _pickedBlock = null; return; }
            
            if (_lastValidPos != null) PlaceBlock(block);
            else block.PutBlockBack();
            
            _pickedBlock = null;
        }

        private void PlaceBlock(Block block)
        {
            GameEvents.RaisePlaySfx(settings.blockPlacementSfx);
            ChangesAfterMove changes = field.PlaceBlock(_lastValidPos, block.color);
            blockSpawner.RemoveBlock(block.gameObject);
            GameEvents.RaiseCalculateNewScore(changes);
            HandleChangesAfterMove(changes);
            _lastValidPos = null;
        }

        private void HidePreviews()
        {
            fieldGraphics.HideCellsPreview();
            fieldGraphics.HidePotentiallyRemovedLinesPreview();
        }

        private void ShowPreviews()
        {
            if(!_pickedBlock || _lastValidPos == null) return;
            fieldGraphics.PreviewCells(_lastValidPos, _pickedBlock.color);
            List<GridPos> potentiallyRemovedLines = field.ReturnCellsOfPotentiallyRemovedLines(_pickedBlock, _lastValidPos);
            fieldGraphics.PreviewPotentiallyRemovedLines(potentiallyRemovedLines, _pickedBlock.color);
        }
        
        private void HandleChangesAfterMove(ChangesAfterMove changes)
        {
            //placing block and removing lines in field graphics
            fieldGraphics.PlaceBlock(changes.BlockCellsPositions, changes.BlockColor);
        
            var rowsAndColsRemoved = 0;
        
            for (var i = 0; i < changes.FullRows.Length; i++)
                if (changes.FullRows[i])
                {
                    rowsAndColsRemoved++;
                    fieldGraphics.RemoveRow(i, changes.BlockColor);
                }
        
            for(var i = 0; i < changes.FullCols.Length; i++)
                if (changes.FullCols[i])
                {
                    rowsAndColsRemoved++;
                    fieldGraphics.RemoveColumn(i, changes.FullRows, changes.BlockColor);
                }
        
            //playing haptics and sfx
            if (rowsAndColsRemoved == 0) GameEvents.RaisePlayHaptics(HapticManager.HapticType.Light);
            else 
            {
                GameEvents.RaisePlayHapticsInARow(HapticManager.HapticType.Heavy, rowsAndColsRemoved);
                GameEvents.RaisePlaySfx(settings.lineRemovalSfx);
            }
            
            //changing theme if necessary  
            if (rowsAndColsRemoved >= 2 && !_tutorialMode) GameEvents.RaiseSetNextTheme();

            //shaking camera
            bool heavyShake = rowsAndColsRemoved >= 3;
            if(rowsAndColsRemoved > 0)
                cameraShakeManager.ShakeForSeconds(heavyShake? settings.heavyShakeDuration : settings.shakeDuration, heavyShake);
        
            if(_tutorialMode) GameEvents.RaiseOnTutorialExampleCompleted();
            else CheckGameOver();
        }

        #endregion
    
        #region Game Over
    
        private void CheckGameOver()
        {
            //if all blocks are placed than it can't be a game over, new ones are being generated
            if(blockSpawner.blocks.All(block => !block)) return;
            
            //checking if any block can be placed in any cell with early exit
            var atLeastOneBlockCanBePlaced = false;
            foreach(GameObject obj in blockSpawner.blocks)
            {
                if(!obj) continue;
                var block = obj.GetComponent<Block>();
                for (var row = 0; row <= settings.rowsCount - block.sizeY; row++)
                {
                    for (var col = 0; col <= settings.columnsCount - block.sizeX; col++)
                        if (FieldUtils.CheckIfBlockCanBePlacedAtCell(field.cellIsFree, block, row, col))
                        {
                            atLeastOneBlockCanBePlaced = true;
                            break;
                        }
                    if (atLeastOneBlockCanBePlaced) break;
                }
                if (atLeastOneBlockCanBePlaced) break;
            }

            if (!atLeastOneBlockCanBePlaced)
                GameEvents.RaiseOnReviveSuggestion();
            GameEvents.RaiseSaveGame();
        }
    
        #endregion

        #region Tutorial

        private void StartTutorial() => _tutorialMode = true;

        private void EndTutorial() => _tutorialMode = false;

        private void LoadTutorialExample(TutorialExample example)
        {
            if(!_tutorialMode) return;
            _tutorialExample = example;
        }

        #endregion

        #region Events
        
        private void OnEnable()
        {
            GameEvents.OnBlockPicked += OnBlockPicked;
            GameEvents.OnBlockMoved += OnBlockMoved;
            GameEvents.OnBlockUnpicked += OnBlockUnpicked;
            GameEvents.StartTutorial += StartTutorial;
            GameEvents.FinishTutorial += EndTutorial;
            GameEvents.LoadTutorialExample += LoadTutorialExample;
        }

        private void OnDisable()
        {
            GameEvents.OnBlockPicked -= OnBlockPicked;
            GameEvents.OnBlockMoved -= OnBlockMoved;
            GameEvents.OnBlockUnpicked -= OnBlockUnpicked;
            GameEvents.StartTutorial -= StartTutorial;
            GameEvents.FinishTutorial -= EndTutorial;
            GameEvents.LoadTutorialExample -= LoadTutorialExample;
        }
        
        #endregion
    }
}
