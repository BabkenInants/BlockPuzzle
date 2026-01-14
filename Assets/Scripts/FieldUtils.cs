using System;
using UnityEngine;

public static class FieldUtils
{
    public static GridPos GetCellCoordinatesOnField(Vector3 position, Vector3 firstCell, float cellSize, 
        bool clamp = false, int cellsCountX = 8, int cellsCountY = 8)
    {
        float x = position.x - firstCell.x;
        float y = firstCell.y - position.y;
        x /= cellSize;
        y /= cellSize;
        int row = Mathf.RoundToInt(y);
        int col = Mathf.RoundToInt(x);
        if (!clamp) return new GridPos(row, col);
        row = Math.Clamp(row, 0, cellsCountY - 1);
        col = Math.Clamp(col, 0, cellsCountX - 1);
        return new GridPos(row, col);
    }
}

public struct ChangesAfterMove 
{
    //BlockPlacement
    public GridPos[] BlockCellsPositions;
    public Color BlockColor;
    //RowsAndColumnsRemoved
    public bool[] FullRows;
    public bool[] FullCols;
}

public struct GridPos
{
    public int Row;
    public int Column;
    
    public GridPos(int row, int column)
    {
        Row = row;
        Column = column;
    }
    
    public bool IsValid(int maxRows, int maxColumns)
    {
        return Row >= 0 && Row < maxRows && Column >= 0 && Column < maxColumns;
    }
}