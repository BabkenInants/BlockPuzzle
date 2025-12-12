using UnityEngine;
using Random = UnityEngine.Random;

public class BlockSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject[] blockPrefabs;
    private GameObject[] blocks;
    [SerializeField] private Color[] colors;

    private void Start()
    {
        blocks = new GameObject[spawnPoints.Length];
    }

    private void Update()
    {
        foreach (var block in blocks)
            if (block != null)
                return;
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            blocks[i] = Instantiate(blockPrefabs[Random.Range(0, blockPrefabs.Length)], spawnPoints[i].position,
                Quaternion.identity);
            blocks[i].GetComponent<Block>().SetColor(colors[Random.Range(0, colors.Length)]);
        }
    }
}
