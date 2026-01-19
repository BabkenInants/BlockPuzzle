using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverMenu;
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
        if(_pickedBlock == null || _gameIsOver) return;
        GameEvents.RaiseHideCellsPreview();
        if (!field.CheckIfBlockCanBePlaced(_pickedBlock.cells)) return;
        GameEvents.RaisePreviewCells(_pickedBlock.cells);
    }

    private void OnBlockUnpicked(Block block)
    {
        if (block == null || _gameIsOver || _pickedBlock != block)
        {
            GameEvents.RaiseHideCellsPreview();
            _pickedBlock = null;
            return;
        }
        if (field.CheckIfBlockCanBePlaced(block.cells))
        {
            var cellsPositions = new GridPos[block.cells.Length];
            for(int i = 0; i < cellsPositions.Length; i++)
                cellsPositions[i] = FieldUtils.GetCellCoordinatesOnField(block.cells[i].position, 
                    fieldGraphics.firstCell.position, settings.cellSize);
            ChangesAfterMove changes = field.PlaceBlock(cellsPositions, block.cells[0].GetComponent<SpriteRenderer>().color);
            blockSpawner.RemoveBlock(block.gameObject);
            StartCoroutine(HandleChangesAfterMove(changes));
            GameEvents.RaiseCalculateNewScore(changes);
        }
        else
        {
            GameEvents.RaiseHideCellsPreview();
            block.PutBlockBack();
        }
        _pickedBlock = null;
    }

    #endregion
    
    #region Game Over
    
    private void CheckGameOver()
    {
        var currentBlocks = new List<Block>();
        foreach(var block in blockSpawner.blocks)
        {
            if (block == null) continue;
            var currentBlock = block.GetComponent<Block>();
            currentBlocks.Add(currentBlock);
        }
        if(currentBlocks.Count == 0) return;
        var atLeastOneBlockCanBePlaced = false;
        foreach(var block in  currentBlocks)
        {
            for (int row = 0; row <= settings.rowsCount - block.sizeY; row++)
            {
                for (int col = 0; col <= settings.columnsCount - block.sizeX; col++)
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

    private IEnumerator HandleChangesAfterMove(ChangesAfterMove changes)
    {
        fieldGraphics.PlaceBlock(changes.BlockCellsPositions, changes.BlockColor);
        int rowsAndColsRemoved = 0;
        
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
        
        yield return new WaitForSeconds(1f);
        
        GameEvents.RaiseCheckGameOver();
    }
    
    private void Subscribe()
    {
        GameEvents.OnBlockPicked += OnBlockPicked;
        GameEvents.OnBlockMoved += OnBlockMoved;
        GameEvents.OnBlockUnpicked += OnBlockUnpicked;
        GameEvents.CheckGameOver += CheckGameOver;
    }

    private void Unsubscribe()
    {
        GameEvents.OnBlockPicked -= OnBlockPicked;
        GameEvents.OnBlockMoved -= OnBlockMoved;
        GameEvents.OnBlockUnpicked -= OnBlockUnpicked;
        GameEvents.CheckGameOver -= CheckGameOver;
    }
}
