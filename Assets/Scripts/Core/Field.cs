using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Saves;
using Tutorial;

namespace Core
{
    public class Field : MonoBehaviour, ISavable
    {
        public bool isReady{get; private set;}
        /// true - free, false - busy
        public bool[,] cellIsFree { get; private set; }
        [SerializeField] private Settings settings;
        private Transform _firstCell;
        private bool _tutorialMode;

        private void Awake()
        {
            isReady = true;
            cellIsFree = new bool[settings.rowsCount, settings.columnsCount];
            for (int i = 0; i < settings.rowsCount; i++) 
            for(int j = 0; j < settings.columnsCount; j++)
                cellIsFree[i, j] = true;
        }

        public void InitFirstCell(Transform firstCell) => _firstCell = firstCell;

        #region Tutorial
        
        private void StartTutorial() => _tutorialMode = true;

        private void EndTutorial() => _tutorialMode = false;

        private void LoadTutorialExample(TutorialExample example)
        {
            if (!_tutorialMode) return;
            for (var row = 0; row < settings.rowsCount; row++)
                for (var col = 0; col < settings.columnsCount; col++)
                    cellIsFree[row, col] = example.cellIsFree[row * settings.columnsCount + col];
        }

        private void OnEnable()
        {
            GameEvents.StartTutorial += StartTutorial;
            GameEvents.FinishTutorial += EndTutorial;
            GameEvents.LoadTutorialExample += LoadTutorialExample;
        }

        private void OnDisable()
        {
            GameEvents.StartTutorial -= StartTutorial;
            GameEvents.FinishTutorial -= EndTutorial;
            GameEvents.LoadTutorialExample -= LoadTutorialExample;
        }
        
        #endregion
        
        #region Placement

        ///Implement only after checking if the cells are free
        public ChangesAfterMove PlaceBlock(GridPos[] cells, Color color)
        {
            foreach (GridPos cell in cells)
                cellIsFree[cell.row, cell.column] = false;
            
            var changesAfterMove = new ChangesAfterMove { BlockCellsPositions = cells, BlockColor = color };
            RemoveFullRowsAndColumns(ref changesAfterMove);
            
            changesAfterMove.FieldIsAllClear = cellIsFree.Cast<bool>().All(x => x);
            return changesAfterMove;
        }
    
        ///Used only for drag and drop because of transform calculation
        public bool CheckIfBlockCanBePlaced(Transform[] cells, out GridPos[] cellPositions)
        {
            cellPositions = new GridPos[cells.Length];
            
            //Checking if the block can be placed in its current position
            var result = true;
            for (var i = 0; i < cells.Length; i++)
            {
                GridPos position = FieldUtils.GetCellCoordinatesOnField(cells[i].position, _firstCell.position, settings.cellSize);
                cellPositions[i] = position;
                if (!position.IsValid(settings.rowsCount, settings.columnsCount))
                {
                    result = false;
                    continue;
                }
                if (!cellIsFree[position.row, position.column]) result = false;
            }
            return result;
        }

        #endregion
    
        #region Removing full rows and columns

        private void RemoveFullRowsAndColumns(ref ChangesAfterMove changesAfterMove)
        {
            var removeRow = new bool[settings.rowsCount];
            var removeCol = new bool[settings.columnsCount];
            
            //checking which rows and cols should be removed and removing them are
            //written separately so there won't be a situation(example) when a row is removed
            //and a column is not because of already missing cell
            
            //Checking rows
            for (var row = 0; row < settings.rowsCount; row++)
            {
                var rowIsFull = true;
                for (var col = 0; col < settings.columnsCount; col++)
                    if (cellIsFree[row, col]) { rowIsFull = false; break; }
                removeRow[row] = rowIsFull;
            }
        
            //Checking columns
            for (var col = 0; col < settings.columnsCount; col++)
            {
                var colIsFull = true;
                for (var row = 0; row < settings.rowsCount; row++)
                    if (cellIsFree[row, col]) { colIsFull = false; break; }
                removeCol[col] = colIsFull;
            }
            
            //Removing full rows
            for (var row = 0; row < settings.rowsCount; row++)
                if (removeRow[row])
                    for(var j = 0; j < settings.columnsCount; j++)
                        cellIsFree[row, j] = true;

            //Removing full columns
            for (var col = 0; col < settings.columnsCount; col++)
                if (removeCol[col])
                    for (var i = 0; i < settings.rowsCount; i++)
                        cellIsFree[i, col] = true;
        
            changesAfterMove.FullRows = removeRow;
            changesAfterMove.FullCols = removeCol;
        }
    
        public List<GridPos> ReturnCellsOfPotentiallyRemovedLines(Block block, GridPos[] cellPositions)
        {
            var result = new List<GridPos>();
            int minRow = settings.rowsCount, minColumn = settings.columnsCount, maxRow = -1, maxColumn = -1;
            var tempField = (bool[,]) cellIsFree.Clone();
            
            for (var i = 0; i < block.cells.Length; i++)
            {
                //calculating minRow/minCol and maxRow/maxCol
                if(cellPositions[i].row < minRow)
                    minRow = cellPositions[i].row;
                if(cellPositions[i].column < minColumn)
                    minColumn = cellPositions[i].column;
                if(cellPositions[i].row > maxRow)
                    maxRow = cellPositions[i].row;
                if(cellPositions[i].column > maxColumn)
                    maxColumn = cellPositions[i].column;
            
                //placing block on tempField
                tempField[cellPositions[i].row, cellPositions[i].column] = false;
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

        #region Saves

        public void Save(SaveData saveData)
        {
            if (saveData.GameIsOver) return;
            saveData.CellIsFree = new bool[settings.rowsCount * settings.columnsCount];
            for (var row = 0; row < settings.rowsCount; row++)
                for (var col = 0; col < settings.columnsCount; col++)
                    saveData.CellIsFree[row * settings.columnsCount + col] = cellIsFree[row, col];
        }

        public void Load(SaveData saveData)
        {
            if (saveData.GameIsOver) return;
            for (var row = 0; row < settings.rowsCount; row++)
                for (var col = 0; col < settings.columnsCount; col++) 
                    cellIsFree[row, col] = saveData.CellIsFree[row * settings.columnsCount + col];
        }
    
        #endregion
    }
}
