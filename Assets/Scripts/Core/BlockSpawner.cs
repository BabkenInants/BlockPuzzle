using System;
using System.Collections.Generic;
using System.Linq;
using Themes;
using UnityEngine;
using Tutorial;
using Random = UnityEngine.Random;

namespace Core
{
    public class BlockSpawner : MonoBehaviour, IThemeReceiver
    {
        public GameObject[] blocks { get; private set; }
        [SerializeField] private Settings settings;
        [SerializeField] private Field field;
        [SerializeField] private Transform[] spawnPoints;
        private bool _gameIsOver;
        private Theme _theme;
        private bool _tutorialMode;
        private GameObject _lastTutorialPreview;
        private bool[,] _tempField;
    
        private void Awake()
        {
            blocks = new GameObject[spawnPoints.Length];
            _tempField = new bool[settings.rowsCount, settings.columnsCount];
        }

        public void SpawnBlocks()
        {
            if(_gameIsOver || _tutorialMode) return;
            
            GameObject[] blocksToSpawn = GenerateNextBlocks();
            if (blocksToSpawn == null) { GameEvents.RaiseGameOver(); return; }
            ShuffleArray(ref blocksToSpawn);
            
            //calculating how much space is available for each block
            float distanceForBlock = (settings.screenWidth - settings.distanceBetweenSpawnedBlocks * 
                                         (2 + spawnPoints.Length - 1)) / spawnPoints.Length;
            // block horizontal length * cellSize * notPickedSize = distanceForBlock =>
            // notPickedSize = distanceForBlock/(length * cellSize)
            
            for (var i = 0; i < spawnPoints.Length; i++)
            {
                blocks[i] = Instantiate(blocksToSpawn[i], spawnPoints[i].position, Quaternion.identity);
                var block = blocks[i].GetComponent<Block>();
                
                //changing block size to look good on screen
                float notPickedSizeX = distanceForBlock / (block.sizeX * settings.cellSize);
                float notPickedSizeY = distanceForBlock / (block.sizeY * settings.cellSize);
                float notPickedSize = Mathf.Min(notPickedSizeX, notPickedSizeY);
                if (notPickedSize > settings.maxNotPickedBlockSize) notPickedSize = settings.maxNotPickedBlockSize;
                var color = _theme.blockColors[Random.Range(0, _theme.blockColors.Length)];
                
                block.Init(settings, notPickedSize, color);
            }
        }

        public void RemoveBlock(GameObject block)
        {
            if(_gameIsOver) return;
            
            var spawnNewBlocks = true;
            for (var i = 0; i < spawnPoints.Length; i++)
            {
                if (blocks[i] == block)
                    blocks[i] = null;
                else if (blocks[i]) spawnNewBlocks = false;
            }
            Destroy(block);
            
            if(spawnNewBlocks && !_tutorialMode) SpawnBlocks();
        }
        
        #region Events
        
        private void OnEnable()
        {
            GameEvents.StartTutorial += StartTutorial;
            GameEvents.FinishTutorial += EndTutorial;
            GameEvents.LoadTutorialExample += LoadTutorialExample;
            GameEvents.OnGameOver += OnGameOver;
        }

        private void OnDisable()
        {
            GameEvents.StartTutorial -= StartTutorial;
            GameEvents.FinishTutorial -= EndTutorial;
            GameEvents.LoadTutorialExample -= LoadTutorialExample;
            GameEvents.OnGameOver -= OnGameOver;
        }

        private void OnGameOver()
        {
            foreach (GameObject block in blocks) Destroy(block);
            _gameIsOver = true;
        }
        
        #endregion
        
        #region Field simulation and new blocks generation
        
        /// <returns>GameObject[] array of blocks that can be placed on current field</returns>
        private GameObject[] GenerateNextBlocks()
        {
            if (_tutorialMode) return null;
            var nextBlocks = new GameObject[spawnPoints.Length];
            Array.Copy(field.cellIsFree, _tempField, field.cellIsFree.Length);
        
            for (var i = 0; i < spawnPoints.Length; i++)
            {
                if (!FindBlockForField(_tempField, settings.blockPrefabs.ToList(), out GameObject tempBlock, out GridPos tempPosition))
                    if (!FindBlockForField(_tempField, settings.smallBlockPrefabs.ToList(), out tempBlock, out tempPosition))
                    {
                        Debug.LogError("No enough block prefabs");
                        return null;
                    }
                nextBlocks[i] = tempBlock;
                PlaceBlockAndUpdateField(ref _tempField, tempBlock.GetComponent<Block>(), tempPosition, out bool[] rowsRemoved, out bool[] colsRemoved);
            }
            
            return nextBlocks;
        }

        /// <returns>true - found, false - no blocks for this field</returns>
        private bool FindBlockForField(bool[,] tempField, List<GameObject> blocksArr, 
            out GameObject tempBlock, out GridPos tempPosition)
        { 
            tempBlock = null;
            tempPosition = new GridPos();
            if (_tutorialMode) return false;
            
            //finding candidates - blocks that can be placed on field with their best positions and grades
            var candidates = new List<BlockCandidate>();
            foreach (GameObject block in blocksArr)
            {
                tempBlock = block;
                if (GetBestPositionForBlock(tempBlock.GetComponent<Block>(), tempField, out GridPos position, out int grade))
                    candidates.Add(new BlockCandidate(tempBlock, position, grade));
            }
            if (candidates.Count == 0) return false;
            
            int bestGrade = candidates.Max(candidate => candidate.Score);
            
            //calculating field business percentage
            int busyCellsCount = tempField.Cast<bool>().Count(cell => !cell);
            int cellsCount = settings.rowsCount * settings.columnsCount;
            float fieldBusynessPercentage = busyCellsCount * 100f / cellsCount;
            
            //choosing candidate from best candidates
            float betterBlockGenerationProbability = fieldBusynessPercentage >= settings.requiredFieldBusinessPercentageForBestBlock ? 1f : settings.betterBlockGenerationProbability;
            List<BlockCandidate> bestCandidates = candidates.Where(candidate => candidate.Score >= bestGrade * betterBlockGenerationProbability).ToList();
            BlockCandidate bestCandidate = bestCandidates[Random.Range(0, bestCandidates.Count)];
        
            tempBlock = bestCandidate.Block;
            tempPosition = bestCandidate.Position;
            //Debug.Log(tempBlock.name + ": " + tempPosition);
            return true;
        }
    
        /// <returns>true - found, false - no position for this block</returns>
        private bool GetBestPositionForBlock(Block block, bool[,] tempField, out GridPos position, 
            out int bestGrade)
        {
            position = new GridPos();
            bestGrade = 0;
            if (_tutorialMode) return false;
            
            var tempPos = new GridPos(-1, -1);
            int maxGrade = -1;
            var foundPosition = false;
            
            for (var row = 0; row <= settings.rowsCount - block.sizeY; row++)
                for (var col = 0; col <= settings.columnsCount - block.sizeX; col++)
                    if (FieldUtils.CheckIfBlockCanBePlacedAtCell(tempField, block, row, col))
                    {
                        foundPosition = true;
                        PlaceBlockAndUpdateField(ref tempField, block, new GridPos(row, col), 
                            out bool[] rowWasRemoved, out bool[] colWasRemoved);
                        int grade = FieldUtils.RateField(tempField, settings);
                        // bigger block => better field score so it will give you bigger blocks all the time
                        grade += block.cells.Length * settings.blockSizeFieldGradeMultiplier;
                        RemoveBlockAndRevertField(ref tempField, block, new GridPos(row, col), 
                            rowWasRemoved, colWasRemoved);
                        if (grade > maxGrade)
                        {
                            maxGrade = grade;
                            tempPos = new GridPos(row, col);
                        }
                    }

            position = tempPos;
            bestGrade = maxGrade;
            return foundPosition;
        }

        private void PlaceBlockAndUpdateField(ref bool[,] tempField, Block block, GridPos position, 
            out bool[] rowWasRemoved, out bool[] colWasRemoved)
        {
            rowWasRemoved = null;
            colWasRemoved = null;
            if(_tutorialMode) return;
            
            var removeRow = new bool[settings.rowsCount];
            var removeCol = new bool[settings.columnsCount];
            
            //checking which rows and cols should be removed and removing them are
            //written separately so there won't be a situation(example) when a row is removed
            //and a column is not because of already missing cell
            
            //Placing block
            for (var y = 0; y < block.sizeY; y++)
                for (var x = 0; x < block.sizeX; x++)
                {
                    if (!block.blockShape[y * block.sizeX + x]) continue;
                    tempField[y + position.row, x + position.column] = false;
                }
            
            //Checking rows
            for (var y = 0; y < settings.rowsCount; y++)
            {
                var rowIsFull = true;
                for (var x = 0; x < settings.columnsCount; x++)
                    if (tempField[y, x]) { rowIsFull = false; break; }
                removeRow[y] = rowIsFull;
            }

            //Checking cols
            for (var x = 0; x < settings.columnsCount; x++)
            {
                var colIsFull = true;
                for (var y = 0; y < settings.rowsCount; y++)
                    if (tempField[y, x]) { colIsFull = false; break; }
                removeCol[x] = colIsFull;
            }
            
            //Removing rows
            for (var y = 0; y < settings.rowsCount; y++)
                if(removeRow[y])
                    for (var x = 0; x < settings.columnsCount; x++)
                        tempField[y, x] = true;

            //Removing cols
            for (var x = 0; x < settings.columnsCount; x++)
                if(removeCol[x])
                    for (var y = 0; y < settings.rowsCount; y++)
                        tempField[y, x] = true;
        
            rowWasRemoved = removeRow;
            colWasRemoved = removeCol;
        }

        private void RemoveBlockAndRevertField(ref bool[,] tempField, Block block, GridPos position, 
            bool[] rowWasRemoved, bool[] colWasRemoved)
        {
            if(_tutorialMode) return;
            
            //restoring removed rows
            for(var row = 0; row < rowWasRemoved.Length; row++)
                if (rowWasRemoved[row])
                    for(var col = 0; col < colWasRemoved.Length; col++)
                        tempField[row, col] = false;
        
            //restoring removed cols
            for (var col = 0; col < colWasRemoved.Length; col++)
                if(colWasRemoved[col])
                    for (var row = 0; row < rowWasRemoved.Length; row++)
                        tempField[row, col] = false;
        
            //removing block
            for (var y = 0; y < block.sizeY; y++)
                for (var x = 0; x < block.sizeX; x++)
                {
                    if (!block.blockShape[y * block.sizeX + x]) continue;
                    tempField[y + position.row, x + position.column] = true;
                }
        }
    
        #endregion
        
        #region Themes
        
        public void ReceiveTheme(Theme theme) => _theme = theme;

        public void ReceiveThemeOnGameStart(Theme theme) => _theme = theme;

        #endregion

        #region Tutorial

        private void StartTutorial() => _tutorialMode = true;

        private void EndTutorial()
        {
            _tutorialMode = false;
            if(_lastTutorialPreview) Destroy(_lastTutorialPreview);
            SpawnBlocks();
        }

        private void LoadTutorialExample(TutorialExample example)
        {
            if (!_tutorialMode) return;
            
            //destroying preview from previous tutorial example
            if(_lastTutorialPreview) Destroy(_lastTutorialPreview);
            
            //destroying blocks generated on game start
            foreach (GameObject b in blocks) Destroy(b);
            
            //getting position of the central spawn point
            Vector3 pos = spawnPoints[Mathf.FloorToInt(spawnPoints.Length / 2f)].position;
            
            //instantiating and initializing block
            GameObject obj = Instantiate(example.blockPrefab, pos, Quaternion.identity);
            var block = obj.GetComponent<Block>();
            var blockColor = _theme.blockColors[Random.Range(0, _theme.blockColors.Length)];
            block.Init(settings, settings.maxNotPickedBlockSize, blockColor);
            
            //instantiating and initializing tutorial block preview
            var offset = new Vector3(-(block.sizeX - 1) / 2f, (block.sizeY - 1) / 2f, 0) * .5f;
            pos += offset * settings.maxNotPickedBlockSize; 
            obj = Instantiate(example.previewBlockPrefab, pos, Quaternion.identity);
            var tutorialBlock = obj.GetComponent<TutorialBlock>();
            Vector3 endPos = example.firstCellPosition;
            endPos.y -= settings.cellSize * example.targetPos.row;
            endPos.x += settings.cellSize * example.targetPos.column;
            Color color = block.color;
            color.a = settings.blockPreviewColorTransparency;
            tutorialBlock.Init(settings, settings.maxNotPickedBlockSize, endPos, color);
            _lastTutorialPreview = obj;
        }

        #endregion

        #region Utils
        
        private static void ShuffleArray<T>(ref T[] array)
        {
            for (var i = 0; i < array.Length; i++)
            {
                int r = Random.Range(0, array.Length);
                (array[i], array[r]) = (array[r], array[i]);
            }
        }

        #endregion
    }
    
    public struct BlockCandidate
    {
        public GameObject Block;
        public GridPos Position;
        public int Score;

        public BlockCandidate(GameObject block, GridPos position, int score)
        {
            Block = block;
            Position = position;
            Score = score;
        }
    }
}