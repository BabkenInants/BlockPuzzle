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

    public static int RateField(bool[,] field)
    {
        var score = 0;
        for(var row = 0; row < field.GetLength(0); row++)
            for (var col = 0; col < field.GetLength(1); col++)
            {
                if (!field[row, col]) continue; //if cell is not free
                var temp = 1;
                if(row > 0 && field[row - 1, col]) temp++;
                if(row + 1 < field.GetLength(0) && field[row + 1, col]) temp++;
                if(col > 0 && field[row, col - 1]) temp++;
                if(col + 1 < field.GetLength(1) && field[row, col + 1]) temp++;
                if (temp == 1) {score -= 5; continue;}
                score += temp * temp;
            }
        return score;
    }
    
    ///Don't use if the block is out of the field(use this function in loops, it's more efficient)
    public static bool CheckIfBlockCanBePlacedAtCell(bool[,] field, Block block, int row, int col)
    {
        for (int y = 0; y < block.sizeY; y++)
        for (int x = 0; x < block.sizeX; x++)
        {
            if (!block.blockShape[y * block.sizeX + x]) 
                continue;
            //checking if the cell is not free
            if (field[row + y, col + x] == false)
                return false;
        }
        return true;
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

    public override string ToString()
    {
        return $"({Row}, {Column})";
    }

    public bool IsValid(int maxRows, int maxColumns)
    {
        return Row >= 0 && Row < maxRows && Column >= 0 && Column < maxColumns;
    }
}