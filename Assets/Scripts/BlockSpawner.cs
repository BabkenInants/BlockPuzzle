using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class BlockSpawner : MonoBehaviour
{
    public static BlockSpawner Instance;
    public GameObject[] blocks;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject[] blockPrefabs;
    [SerializeField] private GameObject[] smallBlockPrefabs;
    [SerializeField] private Color[] colors;
    private bool gameIsRunning = true;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    private void Start() =>
        blocks = new GameObject[spawnPoints.Length];

    public void RemoveBlock(GameObject block)
    {
        for(int i = 0; i < spawnPoints.Length; i++)
            if(blocks[i] == block)
                blocks[i] = null;
        Destroy(block);
    }

    private void Update()
    {
        if (!gameIsRunning) return;
        foreach (var block in blocks)
            if (block != null)
                return;
        SpawnBlocks();
    }

    private List<GameObject> GenerateNextBlocks()
    {
        var nextBlocks = new List<GameObject>();
        var tempField = (bool[,])Field.Instance.cellIsFree.Clone();
        
        for (var i = 0; i < spawnPoints.Length; i++)
        {
            GameObject tempBlock = null;
            var tempPosition = new Vector2Int();
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
    private bool FindBlockForField(bool[,] field, List<GameObject> blocksArr, out GameObject tempBlock, 
        out Vector2Int tempPosition)
    { 
        tempBlock = null;
        tempPosition = new Vector2Int();
        while(blocksArr.Count > 0)
        {
            tempBlock = blocksArr[Random.Range(0, blocksArr.Count)];
            if (!CheckIfBlockCanBePlacedInAnyCell(field, tempBlock.GetComponent<Block>(), ref tempPosition))
                blocksArr.Remove(tempBlock);
            else break;
        }
        return blocksArr.Count > 0;
    }

    private void PlaceBlockAndRemoveColsAndRows(ref bool[,] field, Block block, Vector2Int position)
    {
        //true - free, false - busy
        
        // Placing block
        for (int y = 0; y < block.sizeY; y++)
            for (int x = 0; x < block.sizeX; x++)
            {
                if (!block.blockShape[y * block.sizeX + x]) continue;
                field[y + position.y, x + position.x] = false;
            }

        var rowsToRemove = new List<int>();
        var colsToRemove = new List<int>();

        int h = field.GetLength(0); // rows (Y)
        int w = field.GetLength(1); // cols (X)

        // Rows
        for (int y = 0; y < h; y++)
        {
            bool rowIsFull = true;
            for (int x = 0; x < w; x++)
                if (field[y, x]) { rowIsFull = false; break; }
            if (rowIsFull) rowsToRemove.Add(y);
        }

        // Cols
        for (int x = 0; x < w; x++)
        {
            bool colIsFull = true;
            for (int y = 0; y < h; y++)
                if (field[y, x]) { colIsFull = false; break; }
            if (colIsFull) colsToRemove.Add(x);
        }

        // Remove rows
        foreach (int y in rowsToRemove)
            for (int x = 0; x < w; x++)
                field[y, x] = true;

        // Remove cols
        foreach (int x in colsToRemove)
            for (int y = 0; y < h; y++)
                field[y, x] = true;
    }
    
    private bool CheckIfBlockCanBePlacedInAnyCell(bool[,] field, Block block, ref Vector2Int position)
    {
        for (var y = 0; y <= field.GetLength(0) - block.sizeY; y++)
            for (var x = 0; x <= field.GetLength(1) - block.sizeX; x++)
                if (Field.Instance.CheckIfBlockCanBePlacedAtCell(field, block, y, x))
                {
                    position = new Vector2Int(x, y);
                    return true;
                }
        return false;
    }
    
    private void SpawnBlocks()
    {
        List<GameObject> blocksToSpawn = GenerateNextBlocks();
        //ToDo switch to events
        if (blocksToSpawn == null)
        {
            Field.Instance.EndGame();
            gameIsRunning = false;
            return;
        }
        for (var i = 0; i < spawnPoints.Length; i++)
        {
            blocks[i] = Instantiate(blocksToSpawn[i], spawnPoints[i].position, Quaternion.identity);
            blocks[i].GetComponent<Block>().SetColor(colors[Random.Range(0, colors.Length)]);
        }
    }
}
