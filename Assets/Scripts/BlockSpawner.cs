using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    public static BlockSpawner Instance;
    public GameObject[] blocks;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject[] blockPrefabs;
    [SerializeField] private Color[] colors;

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
        foreach (var block in blocks)
            if (block != null)
                return;
        SpawnBlocks();
    }

    private void SpawnBlocks()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            blocks[i] = Instantiate(blockPrefabs[Random.Range(0, blockPrefabs.Length)], spawnPoints[i].position,
                Quaternion.identity);
            blocks[i].GetComponent<Block>().SetColor(colors[Random.Range(0, colors.Length)]);
        }
        Field.Instance.CheckIfGameIsOver();
    }
}
