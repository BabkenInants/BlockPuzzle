using UnityEngine;

[CreateAssetMenu(fileName = "Settings", menuName = "Settings")]
public class Settings : ScriptableObject
{
    [field: Header("Field")]
    [field: SerializeField] public float cellSize { get; private set; } = .5f;
    [field: SerializeField] public int columnsCount { get; private set; } = 8;
    [field: SerializeField] public int rowsCount { get; private set; } = 8;
    [field: SerializeField] public Sprite emptyCell { get; private set; }
    [field: SerializeField] public Sprite notEmptyCell { get; private set; }
    [SerializeField] public GameObject cellPrefab;
    [SerializeField] public Color defaultCellColor;
    [SerializeField] public Color cellPreviewColor;
    
    [field: Space(10f)]
    [field: Header("Block Placement")]
    [field: SerializeField] public float minBlockDistanceFromCursorY {get; private set; } = 1f;
    [field: SerializeField] public float maxBlockDistanceFromCursorY {get; private set; } = 5f;
    [field: SerializeField] public float minBlockDistanceFromCursorX {get; private set; } = -.5f;
    [field: SerializeField] public float maxBlockDistanceFromCursorX {get; private set; } = 1f;
}
