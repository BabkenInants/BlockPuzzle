using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class BlockSpawner : MonoBehaviour
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
        List<GameObject> blocksToSpawn = GenerateNextBlocks();
        //ToDo switch to events
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
        bool spawnNewBlocks = true;
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

    private List<GameObject> GenerateNextBlocks()
    {
        var nextBlocks = new List<GameObject>();
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
            nextBlocks.Add(tempBlock);
            PlaceBlockAndRemoveColsAndRows(ref tempField, tempBlock.GetComponent<Block>(), tempPosition);
        }
        return nextBlocks;
    }

    //true - found, false - no blocks for this field
    private bool FindBlockForField(bool[,] tempField, List<GameObject> blocksArr, out GameObject tempBlock, 
        out GridPos tempPosition)
    { 
        tempBlock = null;
        tempPosition = new GridPos();
        while(blocksArr.Count > 0)
        {
            tempBlock = blocksArr[Random.Range(0, blocksArr.Count)];
            if (!CheckIfBlockCanBePlacedInAnyCell(tempField, tempBlock.GetComponent<Block>(), ref tempPosition))
                blocksArr.Remove(tempBlock);
            else break;
        }
        return blocksArr.Count > 0;
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
    
    private bool CheckIfBlockCanBePlacedInAnyCell(bool[,] tempField, Block block, ref GridPos position)
    {
        for (var y = 0; y <= tempField.GetLength(0) - block.sizeY; y++)
            for (var x = 0; x <= tempField.GetLength(1) - block.sizeX; x++)
                if (field.CheckIfBlockCanBePlacedAtCell(tempField, block, y, x))
                {
                    position = new GridPos(y, x);
                    return true;
                }
        return false;
    }
    
    #endregion
}
