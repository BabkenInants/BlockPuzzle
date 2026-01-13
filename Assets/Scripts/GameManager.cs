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
            var cellsPositions = new Vector2Int[block.cells.Length];
            for(int i = 0; i < cellsPositions.Length; i++)
                cellsPositions[i] = field.GetCellCoordinatesOnField(block.cells[i].position);
            field.PlaceBlock(cellsPositions, block.cells[0].GetComponent<SpriteRenderer>().color);
            blockSpawner.RemoveBlock(block.gameObject);
            GameEvents.RaiseRequestGameOverCheck();
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
    
    private void GameOverCheck()
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
            for (int row = 0; row <= settings.cellsCountY - block.sizeY; row++)
            {
                for (int col = 0; col <= settings.cellsCountX - block.sizeX; col++)
                {
                    if (field.CheckIfBlockCanBePlacedAtCell(field.cellIsFree, block, row, col))
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

    private void EndGame()
    {
        if (_gameIsOver) return;
        gameOverMenu.SetActive(true);
        _gameIsOver = true;
    }
    
    #endregion
    
    private void GetChangesAfterMove(ChangesAfterMove changes)
    {
        fieldGraphics.PlaceBlock(changes.BlockCellsPositions, changes.BlockColor);
        for (var i = 0; i < changes.FullRows.Length; i++)
            if (changes.FullRows[i]) StartCoroutine(fieldGraphics.RemoveRow(i));
        for(var i = 0; i < changes.FullCols.Length; i++)
            if (changes.FullCols[i]) StartCoroutine(fieldGraphics.RemoveColumn(i, changes.FullRows));
    }

    private void Subscribe()
    {
        GameEvents.OnBlockPicked += OnBlockPicked;
        GameEvents.OnBlockMoved += OnBlockMoved;
        GameEvents.OnBlockUnpicked += OnBlockUnpicked;
        GameEvents.RequestGameOverCheck += GameOverCheck;
        GameEvents.OnGameOver += EndGame;
        GameEvents.ChangesAfterMoveReport += GetChangesAfterMove;
    }

    private void Unsubscribe()
    {
        GameEvents.OnBlockPicked -= OnBlockPicked;
        GameEvents.OnBlockMoved -= OnBlockMoved;
        GameEvents.OnBlockUnpicked -= OnBlockUnpicked;
        GameEvents.RequestGameOverCheck -= GameOverCheck;
        GameEvents.OnGameOver -= EndGame;
        GameEvents.ChangesAfterMoveReport -= GetChangesAfterMove;
    }
}
