using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core;

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

        private void OnEnable() => Subscribe();

        private void OnDisable() => Unsubscribe();

        private IEnumerator Start()
        {
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
            if(!_pickedBlock || _gameIsOver) return;
            fieldGraphics.HideCellsPreview();
            fieldGraphics.HidePotentiallyRemovedLinesPreview();
            if (!field.CheckIfBlockCanBePlaced(_pickedBlock.cells, out GridPos[] cellsPositions))
            {
                if (_lastValidPos == null) return;
                int lastValidMinRow = _lastValidPos.Min(pos => pos.Row);
                int lastValidMinCol = _lastValidPos.Min(pos => pos.Column);
                int currMinRow = cellsPositions.Min(pos => pos.Row);
                int currMinCol = cellsPositions.Min(pos => pos.Column);
                int deltaRow = currMinRow - lastValidMinRow;
                int deltaCol = currMinCol - lastValidMinCol;
                if (Math.Abs(deltaRow) > 1 || Math.Abs(deltaCol) > 1)
                {
                    _lastValidPos = null;
                    return;
                }
                int minRowAfterShift = lastValidMinRow + deltaRow;
                int maxRowAfterShift = _lastValidPos.Max(pos => pos.Row) + deltaRow;
                int minColAfterShift = lastValidMinCol + deltaCol;
                int maxColAfterShift = _lastValidPos.Max(pos => pos.Column) + deltaCol;

                var foundPos = false;
                if (minRowAfterShift >= 0 && maxRowAfterShift < settings.rowsCount)
                {
                    if (FieldUtils.CheckIfBlockCanBePlacedAtCell(field.cellIsFree, _pickedBlock,
                            minRowAfterShift, lastValidMinCol))
                    {
                        foundPos = true;
                        for (var i = 0; i < _lastValidPos.Length; i++)
                            _lastValidPos[i].Row += deltaRow;
                    }
                }
                if (!foundPos && minColAfterShift >= 0 && maxColAfterShift < settings.columnsCount)
                {
                    if (FieldUtils.CheckIfBlockCanBePlacedAtCell(field.cellIsFree, _pickedBlock,
                            lastValidMinRow, minColAfterShift))
                    {
                        for (var i = 0; i < _lastValidPos.Length; i++)
                            _lastValidPos[i].Column += deltaCol;
                    }
                }
                fieldGraphics.PreviewCells(_lastValidPos, _pickedBlock.color);
                fieldGraphics.PreviewPotentiallyRemovedLines(field.ReturnCellsOfPotentiallyRemovedLines
                        (_pickedBlock, _lastValidPos), _pickedBlock.color);
                return;
            }
            _lastValidPos = cellsPositions.ToArray();
            fieldGraphics.PreviewCells(cellsPositions, _pickedBlock.color);
            fieldGraphics.PreviewPotentiallyRemovedLines(field.ReturnCellsOfPotentiallyRemovedLines(_pickedBlock, cellsPositions), _pickedBlock.color);
        }

        private void OnBlockUnpicked(Block block)
        {
            fieldGraphics.HideCellsPreview();
            fieldGraphics.HidePotentiallyRemovedLinesPreview();
            if (!block || _gameIsOver || _pickedBlock != block)
            {
                _pickedBlock = null;
                return;
            }
            if (_lastValidPos != null)
            {
                GameEvents.RaisePlaySfx(settings.blockPlacementSfx);
                ChangesAfterMove changes = field.PlaceBlock(_lastValidPos, block.color);
                blockSpawner.RemoveBlock(block.gameObject);
                GameEvents.RaiseCalculateNewScore(changes);
                HandleChangesAfterMove(changes);
                _lastValidPos = null;
            }
            else block.PutBlockBack();
            _pickedBlock = null;
        }

        #endregion
    
        #region Game Over
    
        private void CheckGameOver()
        {
            List<Block> currentBlocks = (from block in blockSpawner.blocks where block != null 
                select block.GetComponent<Block>()).ToList();
            if(currentBlocks.Count == 0) return;
            var atLeastOneBlockCanBePlaced = false;
            foreach(Block block in currentBlocks)
            {
                for (var row = 0; row <= settings.rowsCount - block.sizeY; row++)
                {
                    for (var col = 0; col <= settings.columnsCount - block.sizeX; col++)
                    {
                        if (FieldUtils.CheckIfBlockCanBePlacedAtCell(field.cellIsFree, block, row, col))
                        {
                            atLeastOneBlockCanBePlaced = true;
                            break;
                        }
                    }
                    if (atLeastOneBlockCanBePlaced) break;
                }
                if (atLeastOneBlockCanBePlaced) break;
            }

            if (!atLeastOneBlockCanBePlaced)
                GameEvents.RaiseGameOver();
            GameEvents.RaiseSaveGame();
        }
    
        #endregion

        private void HandleChangesAfterMove(ChangesAfterMove changes)
        {
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
        
            if (rowsAndColsRemoved == 0) GameEvents.RaisePlayHaptics(HapticManager.HapticType.Light);
            else 
            {
                GameEvents.RaisePlayHapticsInARow(HapticManager.HapticType.Heavy, rowsAndColsRemoved);
                GameEvents.RaisePlaySfx(settings.lineRemovalSfx);
                GameEvents.RaiseSetNextTheme();
            }

            bool heavyShake = rowsAndColsRemoved >= 3;
            if(rowsAndColsRemoved > 0)
                cameraShakeManager.ShakeForSeconds(heavyShake? settings.heavyShakeDuration : settings.shakeDuration, heavyShake);
        
            CheckGameOver();
        }
    
        private void Subscribe()
        {
            GameEvents.OnBlockPicked += OnBlockPicked;
            GameEvents.OnBlockMoved += OnBlockMoved;
            GameEvents.OnBlockUnpicked += OnBlockUnpicked;
        }

        private void Unsubscribe()
        {
            GameEvents.OnBlockPicked -= OnBlockPicked;
            GameEvents.OnBlockMoved -= OnBlockMoved;
            GameEvents.OnBlockUnpicked -= OnBlockUnpicked;
        }
    }
}
