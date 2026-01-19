using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    public bool isReady{get; private set;}
    /// true - free, false - busy
    public bool[,] cellIsFree { get; private set; }
    [SerializeField] private Settings settings;
    private Transform _firstCell;

    private void Awake()
    {
        isReady = true;
        cellIsFree = new bool[settings.rowsCount, settings.columnsCount];
        for (int i = 0; i < settings.rowsCount; i++) 
            for(int j = 0; j < settings.columnsCount; j++)
                cellIsFree[i, j] = true;
    }

    public void InitFirstCell(Transform firstCell) => _firstCell = firstCell;
    
    #region Placement

    ///Implement only after checking if the cells are free
    public ChangesAfterMove PlaceBlock(GridPos[] cells, Color color)
    {
        var changesAfterMove = new ChangesAfterMove
        {
            BlockCellsPositions = cells,
            BlockColor = color
        };
        foreach (GridPos cell in cells)
            cellIsFree[cell.Row, cell.Column] = false;
        CheckForRowOrColumnRemoval(ref changesAfterMove);
        return changesAfterMove;
    }
    
    ///Used only for drag and drop because of transform calculation
    public bool CheckIfBlockCanBePlaced(Transform[] cells, out GridPos[] cellPositions)
    {
        cellPositions = new GridPos[cells.Length];
        //Trying to preview and also checking if the block can be placed in its current position
        for (var i = 0; i < cells.Length; i++)
        {
            GridPos position = FieldUtils.GetCellCoordinatesOnField(cells[i].position, _firstCell.position, settings.cellSize);
            cellPositions[i] = position;
            if (!position.IsValid(settings.rowsCount, settings.columnsCount))
                return false;
            if (!cellIsFree[position.Row, position.Column]) return false;
        }
        return true;
    }

    #endregion
    
    #region Removing full rows and columns

    private void CheckForRowOrColumnRemoval(ref ChangesAfterMove changesAfterMove)
    {
        var fullRows = new bool[settings.rowsCount];
        var fullCols = new bool[settings.columnsCount];
        
        //Checking rows
        for (var row = 0; row < settings.rowsCount; row++)
        {
            var rowIsFull = true;
            for (var col = 0; col < settings.columnsCount; col++)
                if (cellIsFree[row, col]) { rowIsFull = false; break; }
            fullRows[row] = rowIsFull;
        }
        
        //Checking columns
        for (var col = 0; col < settings.columnsCount; col++)
        {
            var colIsFull = true;
            for (var row = 0; row < settings.rowsCount; row++)
                if (cellIsFree[row, col]) { colIsFull = false; break; }
            fullCols[col] = colIsFull;
        }
        
        //Removing full rows
        for (var row = 0; row < fullRows.Length; row++)
            if (fullRows[row])
                RemoveRow(row);

        //Removing full columns
        for (var col = 0; col < fullCols.Length; col++)
            if (fullCols[col])
                RemoveColumn(col, fullRows);
        
        changesAfterMove.FullRows = (bool[]) fullRows.Clone();
        changesAfterMove.FullCols = (bool[]) fullCols.Clone();
    }

    private void RemoveRow(int row)
    {
        for(var j = 0; j < settings.columnsCount; j++)
            cellIsFree[row, j] = true;
    }
    
    private void RemoveColumn(int col, bool[] fullRows)
    {
        for (var i = 0; i < settings.rowsCount; i++)
        {
            if(fullRows[i]) continue;
            cellIsFree[i, col] = true;
        }
    } 
    
    public List<GridPos> ReturnCellsOfPotentiallyRemovedLines(Block block, GridPos[] cellPositions)
    {
        var result = new List<GridPos>();
        int minRow = settings.rowsCount, minColumn = settings.columnsCount, maxRow = -1, maxColumn = -1;
        var tempField = (bool[,])cellIsFree.Clone();
            
        for (var i = 0; i < block.cells.Length; i++)
        {
            //calculating minRow/minCol and maxRow/maxCol
            if(cellPositions[i].Row < minRow)
                minRow = cellPositions[i].Row;
            if(cellPositions[i].Column < minColumn)
                minColumn = cellPositions[i].Column;
            if(cellPositions[i].Row > maxRow)
                maxRow = cellPositions[i].Row;
            if(cellPositions[i].Column > maxColumn)
                maxColumn = cellPositions[i].Column;
            
            //placing block on tempField
            tempField[cellPositions[i].Row, cellPositions[i].Column] = false;
        }

        //adding rows
        for (int row = minRow; row <= maxRow; row++)
        {
            var previewRow = true;
            for (var col = 0; col < settings.columnsCount; col++)
                if(tempField[row, col]) {previewRow = false; break;}
            
            if (!previewRow) continue;
            
            //adding each busy cell to result list
            for(var col = 0; col < settings.columnsCount; col++)
                if(!cellIsFree[row, col])
                    result.Add(new GridPos(row, col));
        }

        //adding cols
        for (int col = minColumn; col <= maxColumn; col++)
        {
            var previewCol = true;
            for(var row = 0; row < settings.rowsCount; row++)
                if (tempField[row, col]) {previewCol = false; break;}
            
            if(!previewCol) continue;
            
            //adding each busy cell to result list
            for (var row = 0; row < settings.rowsCount; row++)
                if (!cellIsFree[row, col])
                    result.Add(new GridPos(row, col));
        }

        return result;
    }

    #endregion
}
