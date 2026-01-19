using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Field field;
    [SerializeField] private FieldGraphics fieldGraphics;
    [SerializeField] private BlockSpawner blockSpawner;
    [SerializeField] private Settings settings;
    [SerializeField] private CameraShake cameraShake;
    private Block _pickedBlock;
    private bool _gameIsOver;

    private void OnEnable() => Subscribe();

    private void OnDisable() => Unsubscribe();

    private IEnumerator Start()
    {
        while(!field.isReady || !fieldGraphics.isReady || !blockSpawner.isReady) yield return null;
        field.InitFirstCell(fieldGraphics.firstCell);
    }

    #region Block Placement

    private void OnBlockPicked(Block block)
    {
        if (_gameIsOver) return;
        _pickedBlock = block;
    }
    
    private void OnBlockMoved()
    {
        if(!_pickedBlock || _gameIsOver) return;
        fieldGraphics.HideCellsPreview();
        fieldGraphics.HidePotentiallyRemovedLinesPreview();
        if (!field.CheckIfBlockCanBePlaced(_pickedBlock.cells, out GridPos[] cellsPositions)) return;
        fieldGraphics.PreviewCells(cellsPositions);
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
        if (field.CheckIfBlockCanBePlaced(block.cells, out GridPos[] cellsPositions))
        {
            ChangesAfterMove changes = field.PlaceBlock(cellsPositions, block.color);
            blockSpawner.RemoveBlock(block.gameObject);
            HandleChangesAfterMove(changes);
            GameEvents.RaiseCalculateNewScore(changes);
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
        
        if (rowsAndColsRemoved == 0) HapticManager.Light();
        else StartCoroutine(HapticManager.PlayHapticsInARowRoutine(HapticManager.HapticType.Heavy, 
                rowsAndColsRemoved));
        
        if (rowsAndColsRemoved == 2)
            cameraShake.ShakeForSeconds(.03f, false);
        else if (rowsAndColsRemoved >= 3)
            cameraShake.ShakeForSeconds(.04f, true);
        
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
