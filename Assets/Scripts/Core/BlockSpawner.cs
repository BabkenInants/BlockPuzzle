using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core
{
    public class BlockSpawner : MonoBehaviour
    {
        public GameObject[] blocks { get; private set; }
        [SerializeField] private Settings settings;
        [SerializeField] private Field field;
        [SerializeField] private Transform[] spawnPoints;
        private bool _gameIsOver;
    
        private void Awake()
        {
            blocks = new GameObject[spawnPoints.Length];
        }

        private void OnEnable() => GameEvents.OnGameOver += OnGameOver;

        private void OnDisable() => GameEvents.OnGameOver -= OnGameOver;

        private void OnGameOver()
        {
            foreach (GameObject block in blocks) Destroy(block);
            _gameIsOver = true;
        }
    
        public void SpawnBlocks()
        {
            if(_gameIsOver) return;
            GameObject[] blocksToSpawn = GenerateNextBlocks();
            if (blocksToSpawn == null)
            {
                GameEvents.RaiseGameOver();
                return;
            }
            ShuffleArray(ref blocksToSpawn);
            for (var i = 0; i < spawnPoints.Length; i++)
            {
                blocks[i] = Instantiate(blocksToSpawn[i], spawnPoints[i].position, Quaternion.identity);
                blocks[i].GetComponent<Block>().SetColor(settings.colors[Random.Range(0, settings.colors.Length)]);
                blocks[i].GetComponent<Block>().InitSettings(settings);
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
                else if (blocks[i] != null) spawnNewBlocks = false;
            }
            Destroy(block);
            if(spawnNewBlocks) SpawnBlocks();
        }

        private void ShuffleArray<T>(ref T[] array)
        {
            for (var i = 0; i < array.Length; i++)
            {
                int r = Random.Range(0, array.Length);
                (array[i], array[r]) = (array[r], array[i]);
            }
        }

        #region Field simulation and new blocks generation

        private GameObject[] GenerateNextBlocks()
        {
            var nextBlocks = new GameObject[spawnPoints.Length];
            var tempField = (bool[,]) field.cellIsFree.Clone();
        
            for (var i = 0; i < spawnPoints.Length; i++)
            {
                if (!FindBlockForField(tempField, settings.blockPrefabs.ToList(), out GameObject tempBlock, out GridPos tempPosition))
                    if (!FindBlockForField(tempField, settings.smallBlockPrefabs.ToList(), out tempBlock, out tempPosition))
                    {
                        Debug.LogError("No enough block prefabs");
                        return null;
                    }
                nextBlocks[i] = tempBlock;
                PlaceBlockAndUpdateField(ref tempField, tempBlock.GetComponent<Block>(), tempPosition, out bool[] rowsRemoved, out bool[] colsRemoved);
            }
            return nextBlocks;
        }

        ///true - found, false - no blocks for this field
        private bool FindBlockForField(bool[,] tempField, List<GameObject> blocksArr, out GameObject tempBlock, out GridPos tempPosition)
        { 
            tempBlock = null;
            tempPosition = new GridPos();
            var candidates = new List<BlockCandidate>();
            int length = blocksArr.Count;
            for(var i = 0; i < length; i++)
            {
                tempBlock = blocksArr[i];
                if (GetBestPositionForBlock(tempBlock.GetComponent<Block>(), tempField, out GridPos position,  out int grade))
                    candidates.Add(new BlockCandidate(tempBlock, position, grade));
            }

            if (candidates.Count == 0) return false;

            int bestGrade = candidates.Max(candidate => candidate.Score);
            //counting busy cells then calculating field busyness percentage
            float fieldBusynessPercentage = (float) tempField.Cast<bool>().Count(cell => !cell) * 100 / (settings.rowsCount * settings.columnsCount);
            float betterBlockGenerationProbability = fieldBusynessPercentage >= 60 ? 1f : .85f;
            List<BlockCandidate> bestCandidates = candidates.Where(candidate => candidate.Score >= bestGrade * betterBlockGenerationProbability).ToList();
            BlockCandidate bestCandidate = bestCandidates[Random.Range(0, bestCandidates.Count)];
        
            tempBlock = bestCandidate.Block;
            tempPosition = bestCandidate.Position;
            //Debug.Log(tempBlock.name + ": " + tempPosition);
            return true;
        }
    
        ///true - found, false - no position for this block
        private bool GetBestPositionForBlock(Block block, bool[,] tempField, out GridPos position, out int bestGrade)
        {
            var tempPos = new GridPos(-1, -1);
            int maxGrade = -1;
            var foundPosition = false;
            for (var row = 0; row <= settings.rowsCount - block.sizeY; row++)
            for (var col = 0; col <= settings.columnsCount - block.sizeX; col++)
                if (FieldUtils.CheckIfBlockCanBePlacedAtCell(tempField, block, row, col))
                {
                    foundPosition = true;
                    PlaceBlockAndUpdateField(ref tempField, block, new GridPos(row, col), out bool[] rowWasRemoved, out bool[] colWasRemoved);
                    int grade = FieldUtils.RateField(tempField);
                    // bigger block => better field score so it will give you bigger blocks all the time
                    grade += block.cells.Length * 10; 
                    RemoveBlockAndRevertField(ref tempField, block, new GridPos(row, col), rowWasRemoved, colWasRemoved);
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

        private void PlaceBlockAndUpdateField(ref bool[,] tempField, Block block, GridPos position, out bool[] rowWasRemoved, out bool[] colWasRemoved)
        {
            // Placing block
            for (var y = 0; y < block.sizeY; y++)
            for (var x = 0; x < block.sizeX; x++)
            {
                if (!block.blockShape[y * block.sizeX + x]) continue;
                tempField[y + position.Row, x + position.Column] = false;
            }

            var rowsToRemove = new bool[settings.rowsCount];
            var colsToRemove = new bool[settings.columnsCount];

            // Rows
            for (var y = 0; y < settings.rowsCount; y++)
            {
                var rowIsFull = true;
                for (var x = 0; x < settings.columnsCount; x++)
                    if (tempField[y, x]) { rowIsFull = false; break; }
                if (rowIsFull) rowsToRemove[y] = true;
            }

            // Cols
            for (var x = 0; x < settings.columnsCount; x++)
            {
                var colIsFull = true;
                for (var y = 0; y < settings.rowsCount; y++)
                    if (tempField[y, x]) { colIsFull = false; break; }
                if (colIsFull) colsToRemove[x] = true;
            }

            // Remove rows
            for (var y = 0; y < settings.rowsCount; y++)
                if(rowsToRemove[y])
                    for (var x = 0; x < settings.columnsCount; x++)
                        tempField[y, x] = true;

            // Remove cols
            for (var x = 0; x < settings.columnsCount; x++)
                if(colsToRemove[x])
                    for (var y = 0; y < settings.rowsCount; y++)
                    {
                        if (rowsToRemove[y]) continue;
                        tempField[y, x] = true;
                    }
        
            rowWasRemoved = rowsToRemove;
            colWasRemoved = colsToRemove;
        }

        private void RemoveBlockAndRevertField(ref bool[,] tempField, Block block, GridPos position, bool[] rowWasRemoved, bool[] colWasRemoved)
        {
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
                tempField[y + position.Row, x + position.Column] = true;
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