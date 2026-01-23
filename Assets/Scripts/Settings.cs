using UnityEngine;

[CreateAssetMenu(fileName = "Settings", menuName = "Settings")]
public class Settings : ScriptableObject
{
    [field: Header("Blocks")]
    [field: SerializeField] public GameObject[] blockPrefabs { get; private set; }
    [field: SerializeField] public GameObject[] smallBlockPrefabs { get; private set; }
    [field: SerializeField] public Color[] colors { get; private set; }
    
    [field: Space(10f)]
    [field: Header("Field")]
    [field: SerializeField] public float cellSize { get; private set; } = .5f;
    [field: SerializeField] public int columnsCount { get; private set; } = 8;
    [field: SerializeField] public int rowsCount { get; private set; } = 8;
    [field: SerializeField] public Sprite emptyCell { get; private set; }
    [field: SerializeField] public Sprite notEmptyCell { get; private set; }
    [SerializeField] public GameObject cellPrefab;
    [SerializeField] public Color defaultCellColor;
    
    [field: Space(10f)]
    [field: Header("Block Placement")]
    [field: SerializeField] public float minBlockDistanceFromCursorY {get; private set; } = 1f;
    [field: SerializeField] public float maxBlockDistanceFromCursorY {get; private set; } = 5f;
    [field: SerializeField] public float minBlockDistanceFromCursorX {get; private set; } = -.5f;
    [field: SerializeField] public float maxBlockDistanceFromCursorX {get; private set; } = 1f;
    [field: SerializeField] public int blockCellsDefaultSpriteLayer {get; private set; } = 1;
    [field: SerializeField] public int blockCellsPickedSpriteLayer {get; private set; } = 3;
    
    [field: Space(10f)]
    [field: Header("Camera")]
    [field: SerializeField] public float width {get; private set; } = 6.5f;
    [field: SerializeField] public float height {get; private set; } = 12f;
    [field: SerializeField] public Vector3 camCenter {get; private set; } = new Vector3(0, -1, -10);
    
    [field: Header("Camera Shake")]
    [field: SerializeField] public float minDist {get; private set; } = 0.1f;
    [field: SerializeField] public float maxDist {get; private set; } = 0.5f;

    [field: Space(10f)]
    [field: Header("SFX")]
    [field: SerializeField] public AudioClip blockPickupSfx { get; private set; }
    [field: SerializeField] public AudioClip blockPlacementSfx { get; private set; }
    [field: SerializeField] public AudioClip lineRemovalSfx { get; private set; }
    [field: SerializeField] public AudioClip gameOverSfx { get; private set; }
    
    [field: Space(10f)]
    [field: Header("Score")]
    [field: SerializeField] public int lineRemovalScoreMultiplier {get; private set;} = 10;
    [field: SerializeField] public int multipleLinesRemovalScoreMultiplier {get; private set;} = 50;
    [field: SerializeField] public int comboScoreMultiplier {get; private set;} = 50;
    [field: SerializeField] public int resetComboAfterMoves {get; private set;} = 3;
    [field: SerializeField] public int allClearBonus {get; private set;} = 1000;
    
    [field: Space(10f)]
    [field: Header("UI Animations")]
    [field: SerializeField] public float comboAnimationDuration {get; private set;} = .6f;
    [field: SerializeField] public float allClearTextAnimationDuration {get; private set;} = .6f;
    [field: SerializeField] public float scoreUpdateAnimationDuration {get; private set;} = 2.5f;
    [field: SerializeField] public float waitBeforeGameOverMenuAppears {get; private set;} = 1f;
    [field: SerializeField] public float scoreHeartBeatFrequency {get; private set;} = .2f;
}
