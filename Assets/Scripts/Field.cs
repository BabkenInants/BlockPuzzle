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

    public void InitFirstCell(Transform firstCell) =>
        _firstCell = firstCell;

    public List<GridPos> ReturnCellsOfPotentiallyRemovedLines(Block block)
    {
        var result = new List<GridPos>();
        var cellPositions = new GridPos[block.cells.Length];
        int minRow = settings.rowsCount, minColumn = settings.columnsCount, maxRow = -1, maxColumn = -1;
        var tempField = (bool[,])cellIsFree.Clone();
        for (var i = 0; i < block.cells.Length; i++)
        {
            cellPositions[i] =
                FieldUtils.GetCellCoordinatesOnField(block.cells[i].position, _firstCell.position, settings.cellSize);
            if(cellPositions[i].Row < minRow)
                minRow = cellPositions[i].Row;
            if(cellPositions[i].Column < minColumn)
                minColumn = cellPositions[i].Column;
            if(cellPositions[i].Row > maxRow)
                maxRow = cellPositions[i].Row;
            if(cellPositions[i].Column > maxColumn)
                maxColumn = cellPositions[i].Column;
            tempField[cellPositions[i].Row, cellPositions[i].Column] = false;
        }

        for (int row = minRow; row <= maxRow; row++)
        {
            var previewRow = true;
            for (var col = 0; col < settings.columnsCount; col++)
                if(tempField[row, col]) {previewRow = false; break;}
            if(previewRow)
                for(var col = 0; col < settings.columnsCount; col++)
                    if(!cellIsFree[row, col])
                        result.Add(new GridPos(row, col));
        }

        for (int col = minColumn; col <= maxColumn; col++)
        {
            var previewCol = true;
            for(var row = 0; row < settings.rowsCount; row++)
                if (tempField[row, col]) {previewCol = false; break;}
            if(previewCol)
                for (var row = 0; row < settings.rowsCount; row++)
                    if (!cellIsFree[row, col])
                        result.Add(new GridPos(row, col));
        }

        return result;
    }

    #region Placement

    ///Implement only after checking if the cells are free
    public ChangesAfterMove PlaceBlock(GridPos[] cells, Color color)
    {
        ChangesAfterMove changesAfterMove = new ChangesAfterMove
        {
            BlockCellsPositions = cells,
            BlockColor = color
        };
        foreach (GridPos cell in cells)
            cellIsFree[cell.Row, cell.Column] = false;
        int rowsAndColumnsRemoved = CheckForRowOrColumnRemoval(ref changesAfterMove);
        return changesAfterMove;
    }
    
    ///Used only for drag and drop
    public bool CheckIfBlockCanBePlaced(Transform[] cells)
    {
        //Trying to preview and also checking if the block can be placed in its current position
        foreach(Transform cell in cells)
        {
            GridPos position = FieldUtils.GetCellCoordinatesOnField(cell.position, _firstCell.position, settings.cellSize);
            if (!position.IsValid(settings.rowsCount, settings.columnsCount))
                return false;
            if (!cellIsFree[position.Row, position.Column]) return false;
        }
        return true;
    }

    #endregion
    
    #region Removing full rows and columns
    
    private int CheckForRowOrColumnRemoval(ref ChangesAfterMove changesAfterMove)
    {
        var fullRows = new bool[settings.rowsCount];
        var fullCols = new bool[settings.columnsCount];
        //Checking rows
        for (int i = 0; i < settings.rowsCount; i++)
        {
            bool rowIsFull = true;
            for (int j = 0; j < settings.columnsCount; j++)
                if (cellIsFree[i, j])
                {
                    rowIsFull = false;
                    break;
                }
            fullRows[i] = rowIsFull;
        }
        //Checking columns
        for (int j = 0; j < settings.columnsCount; j++)
        {
            bool colIsFull = true;
            for (int i = 0; i < settings.rowsCount; i++)
                if (cellIsFree[i, j])
                {
                    colIsFull = false;
                    break;
                }
            fullCols[j] = colIsFull;
        }
        
        var rowsNColsRemoved = 0;
        
        //Removing full rows
        for (var row = 0; row < fullRows.Length; row++)
            if (fullRows[row])
            {
                RemoveRow(row);
                rowsNColsRemoved++;
            }

        //Removing full columns
        for (var col = 0; col < fullCols.Length; col++)
            if (fullCols[col])
            {
                RemoveColumn(col, fullRows);
                rowsNColsRemoved++;
            }
        changesAfterMove.FullRows = (bool[]) fullRows.Clone();
        changesAfterMove.FullCols = (bool[]) fullCols.Clone();
        return rowsNColsRemoved;
    }

    private void RemoveRow(int row)
    {
        for(int j = 0; j < settings.columnsCount; j++)
            cellIsFree[row, j] = true;
    }
    
    private void RemoveColumn(int col, bool[] fullRows)
    {
        for (int i = 0; i < settings.rowsCount; i++)
        {
            if(fullRows[i]) continue;
            cellIsFree[i, col] = true;
        }
    } 

    #endregion
}
