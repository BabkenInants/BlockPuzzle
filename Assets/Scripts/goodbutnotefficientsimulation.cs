using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SocialPlatforms.Impl;
using Random = UnityEngine.Random;

public class goodbutnotefficientsimulation : MonoBehaviour
{
    public bool isReady{get; private set;}
    [SerializeField] private Settings settings;
    [SerializeField] private Field field;
    public GameObject[] blocks;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject[] blockPrefabs;
    [SerializeField] private GameObject[] smallBlockPrefabs;
    [SerializeField] private Color[] colors;
    private bool _gameIsOver;
    
    private void Start()
    {
        blocks = new GameObject[spawnPoints.Length];
        isReady = true;
        SpawnBlocks();
    }

    private void OnEnable()
    {
        GameEvents.OnGameOver += OnGameOver;
    }

    private void OnDisable()
    {
        GameEvents.OnGameOver -= OnGameOver;
    }

    private void OnGameOver() => _gameIsOver = true;
    
    private void SpawnBlocks()
    {
        if(_gameIsOver) return;
        GameObject[] blocksToSpawn = GenerateNextBlocks();
        if (blocksToSpawn == null)
        {
            GameEvents.RaiseGameOver();
            return;
        }
        for (var i = 0; i < spawnPoints.Length; i++)
        {
            blocks[i] = Instantiate(blocksToSpawn[i], spawnPoints[i].position, Quaternion.identity);
            blocks[i].GetComponent<Block>().SetColor(colors[Random.Range(0, colors.Length)]);
            blocks[i].GetComponent<Block>().InitSettings(settings);
        }
    }

    public void RemoveBlock(GameObject block)
    {
        if(_gameIsOver) return;
        var spawnNewBlocks = true;
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (blocks[i] == block)
                blocks[i] = null;
            else if (blocks[i] != null) spawnNewBlocks = false;
        }
        Destroy(block);
        if(spawnNewBlocks) SpawnBlocks();
    }

    #region Field simulation and new blocks generation

    private GameObject[] GenerateNextBlocks()
    {
        Debug.Log("Generating new blocks");
        var nextBlocks = new GameObject[spawnPoints.Length];
        var tempField = (bool[,]) field.cellIsFree.Clone();
        
        for (var i = 0; i < spawnPoints.Length; i++)
        {
            GameObject tempBlock;
            GridPos tempPosition;
            if (!FindBlockForField(tempField, blockPrefabs.ToList(), out tempBlock, out tempPosition))
            {
                //Fallback(small blocks)
                if (!FindBlockForField(tempField, smallBlockPrefabs.ToList(), out tempBlock, out tempPosition))
                {
                    Debug.Log("No enough block prefabs");
                    return null;
                }
            }
            nextBlocks[i] = tempBlock;
            PlaceBlockAndRemoveColsAndRows(ref tempField, tempBlock.GetComponent<Block>(), tempPosition);
        }
        Debug.Log("generation ended");
        return nextBlocks;
    }
    
    ///true - found, false - no position for this block
    private bool GetBestPositionForBlock(Block block, bool[,] tempField, out GridPos position, out int bestGrade)
    {
        var tempPos = new GridPos(-1, -1);
        int maxGrade = -1;
        var foundPosition = false;
        for (var row = 0; row <= settings.rowsCount - block.sizeY; row++)
        {
            for (var col = 0; col <= settings.columnsCount - block.sizeX; col++)
            {
                if (FieldUtils.CheckIfBlockCanBePlacedAtCell(tempField, block, row, col))
                {
                    foundPosition = true;
                    int grade = RatePosition(tempField, block, new GridPos(row, col));
                    if (grade > maxGrade)
                    {
                        maxGrade = grade;
                        tempPos = new GridPos(row, col);
                        if (grade == block.sizeX + block.sizeY)
                        {
                            position = tempPos;
                            bestGrade = grade;
                            return true;
                        }
                    }
                }
            }
        }
        position = tempPos;
        bestGrade = maxGrade;
        return foundPosition;
    }

    ///Use only with valid positions
    private int RatePosition(bool[,] tempField, Block block, GridPos position)
    {
        var linesRemoved = 0;
        for (int row = position.Row; row < position.Row + block.sizeY; row++)
        {
            var addLine = true;
            for (var col = 0; col < settings.columnsCount; col++)
            {
                if (CellIsBusy(tempField, block, row, col, position)) continue;
                addLine = false;
                break;
            }
            if (addLine) linesRemoved++;
        }

        for (int col = position.Column; col < position.Column + block.sizeX; col++)
        {
            var addLine = true;
            for (var row = 0; row < settings.rowsCount; row++)
            {
                if (CellIsBusy(tempField, block, row, col, position)) continue;
                addLine = false;
                break;
            }
            if (addLine) linesRemoved++;
        }
        
        return linesRemoved * 10 + 1;
    }

    ///Used in RatePosition
    private bool CellIsBusy(bool[,] tempField, Block block, int row, int col, GridPos position)
    {
        if (tempField[row, col])
        {
            if (col >= position.Column && col < position.Column + block.sizeX)
            {
                if (row >= position.Row && row < position.Row + block.sizeY)
                {
                    int blockRow = row - position.Row;
                    int blockCol = col - position.Column;
                    return block.blockShape[blockRow * block.sizeX + blockCol];
                }
            }
            return false;
        }
        return true;
    }

    ///true - found, false - no blocks for this field
    private bool FindBlockForField(bool[,] tempField, List<GameObject> blocksArr, out GameObject tempBlock, 
        out GridPos tempPosition)
    { 
        tempBlock = null;
        tempPosition = new GridPos();
        var candidates = new List<BlockCandidate>();
        foreach (GameObject block in blocksArr)
        {
            tempBlock = block;
            if (GetBestPositionForBlock(tempBlock.GetComponent<Block>(), tempField, out tempPosition, out int grade))
                candidates.Add(new BlockCandidate(tempBlock, tempPosition, grade));
        }
        BlockCandidate bestCandidate = MaxScore(candidates);
        tempBlock = bestCandidate.Block;
        tempPosition = bestCandidate.Position;
        return candidates.Count > 0;
    }

    private BlockCandidate MaxScore(List<BlockCandidate> candidates)
    {
        BlockCandidate best = candidates[0];
        foreach (BlockCandidate candidate in candidates)
            if (candidate.Score > best.Score)
                best = candidate;
        return best;
    }

    private void PlaceBlockAndRemoveColsAndRows(ref bool[,] tempField, Block block, GridPos position)
    {
        //true - free, false - busy
        
        // Placing block
        for (int y = 0; y < block.sizeY; y++)
            for (int x = 0; x < block.sizeX; x++)
            {
                if (!block.blockShape[y * block.sizeX + x]) continue;
                tempField[y + position.Row, x + position.Column] = false;
            }

        var rowsToRemove = new bool[settings.rowsCount];
        var colsToRemove = new bool[settings.columnsCount];

        int h = tempField.GetLength(0); // rows (Y)
        int w = tempField.GetLength(1); // cols (X)

        // Rows
        for (int y = 0; y < h; y++)
        {
            bool rowIsFull = true;
            for (int x = 0; x < w; x++)
                if (tempField[y, x]) { rowIsFull = false; break; }
            if (rowIsFull) rowsToRemove[y] = true;
        }

        // Cols
        for (int x = 0; x < w; x++)
        {
            bool colIsFull = true;
            for (int y = 0; y < h; y++)
                if (tempField[y, x]) { colIsFull = false; break; }
            if (colIsFull) colsToRemove[x] = true;
        }

        // Remove rows
        for (int y = 0; y < rowsToRemove.Length; y++)
            if(rowsToRemove[y])
                for (int x = 0; x < w; x++)
                    tempField[y, x] = true;

        // Remove cols
        for (int x = 0; x < colsToRemove.Length; x++)
            if(colsToRemove[x])
                for (int y = 0; y < h; y++)
                {
                    if (rowsToRemove[y]) continue;
                    tempField[y, x] = true;
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
