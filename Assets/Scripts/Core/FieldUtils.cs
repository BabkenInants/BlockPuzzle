using System;
using UnityEngine;

namespace Core
{
    public static class FieldUtils
    {
        public static GridPos GetCellCoordinatesOnField(Vector3 position, Vector3 firstCell, float cellSize, 
            int cellsCountX = 8, int cellsCountY = 8, bool clamp = false)
        {
            float x = (position.x - firstCell.x) / cellSize;
            float y = (firstCell.y - position.y) / cellSize;
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
            var fieldIsAllClear = true;
            
            for(var row = 0; row < field.GetLength(0); row++)
                for (var col = 0; col < field.GetLength(1); col++)
                {
                    if (!field[row, col])
                    {
                        fieldIsAllClear = false;
                        continue; //if cell is not free
                    }
                    
                    var temp = 1;
                    if(row > 0 && field[row - 1, col]) temp++;
                    if(row + 1 < field.GetLength(0) && field[row + 1, col]) temp++;
                    if(col > 0 && field[row, col - 1]) temp++;
                    if(col + 1 < field.GetLength(1) && field[row, col + 1]) temp++;
                    
                    score += temp * temp;
                }
            
            if (fieldIsAllClear) score *= 2;
            return score;
        }
    
        ///Don't use if the block is out of the field(use this function in loops, it's more efficient)
        public static bool CheckIfBlockCanBePlacedAtCell(bool[,] field, Block block, int row, int col)
        {
            for (var y = 0; y < block.sizeY; y++)
                for (var x = 0; x < block.sizeX; x++)
                {
                    if (!block.blockShape[y * block.sizeX + x]) 
                        continue;
                    //checking if the cell is not free
                    if (!field[row + y, col + x])
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
        //All clear bonus
        public bool FieldIsAllClear;
    }

    [Serializable]
    public struct GridPos : IEquatable<GridPos>
    {
        public int row;
        public int column;
    
        public GridPos(int row, int column)
        {
            this.row = row;
            this.column = column;
        }

        public override string ToString()
        {
            return $"({row}, {column})";
        }

        public static bool operator ==(GridPos a, GridPos b)
        {
            return a.row ==  b.row && a.column == b.column;
        }

        public static bool operator !=(GridPos a, GridPos b)
        {
            return a.row != b.row || a.column != b.column;
        }

        public bool IsValid(int maxRows, int maxColumns)
        {
            return row >= 0 && row < maxRows && column >= 0 && column < maxColumns;
        }

        public bool Equals(GridPos other)
        {
            return row == other.row && column == other.column;
        }

        public override bool Equals(object obj)
        {
            return obj is GridPos other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(row, column);
        }
    }
}