using UnityEngine;
using UnityEngine.Serialization;

namespace Core
{
    [CreateAssetMenu(fileName = "Settings", menuName = "Settings")]
    public class Settings : ScriptableObject
    {
        #region Blocks

        [field: Header("Blocks")]
        [field: SerializeField] public GameObject[] blockPrefabs { get; private set; }
        [field: SerializeField] public GameObject[] smallBlockPrefabs { get; private set; }
        [field: SerializeField] public float distanceBetweenSpawnedBlocks { get; private set; } = .4f;
        [field: SerializeField] public float maxNotPickedBlockSize { get; private set; } = .7f;

        #endregion

        #region Block Generation

        [field: Space(10f)]
        [field: Header("Block Generation")]
        [field: Tooltip("Grade += blockCellsCount * multiplier")]
        [field: SerializeField] public int blockSizeFieldGradeMultiplier { get; private set; } = 10;
        [field: SerializeField] public float requiredFieldBusinessPercentageForBestBlock { get; private set; } = 60;
        [field: SerializeField] public float betterBlockGenerationProbability { get; private set; } = .85f;
        [field: SerializeField] public int fieldIsAllClearMultiplier { get; private set; } = 2;

        #endregion
    
        #region Field
        
        [field: Space(10f)]
        [field: Header("Field")]
        [field: SerializeField] public float cellSize { get; private set; } = .65f;
        [field: SerializeField] public int columnsCount { get; private set; } = 8;
        [field: SerializeField] public int rowsCount { get; private set; } = 8;
        [field: SerializeField] public Sprite emptyCell { get; private set; }
        [field: FormerlySerializedAs("<notEmptyCell>k__BackingField")] 
        [field: SerializeField] public Sprite busyCell { get; private set; }
        [SerializeField] public GameObject cellPrefab;
        [field: SerializeField] public float waitTimeBetweenRows {get; private set;} = .1f;
    
        #endregion
    
        #region Block Placement
        
        [field: Space(10f)]
        [field: Header("Block Placement")]
        [field: SerializeField] public float minBlockDistanceFromCursorY {get; private set; } = 1f;
        [field: SerializeField] public float maxBlockDistanceFromCursorY {get; private set; } = 5f;
        [field: SerializeField] public float minBlockDistanceFromCursorX {get; private set; } = -.5f;
        [field: SerializeField] public float maxBlockDistanceFromCursorX {get; private set; } = 1f;
        [field: SerializeField] public int notPickedBlockCellsSpriteLayer {get; private set; } = 1;
        [field: SerializeField] public int pickedBlockCellsSpriteLayer {get; private set; } = 2;
        [Range(0, 1)]
        [field: SerializeField] public float blockPreviewColorTransparency {get; private set; } = .6f;
    
        #endregion

        #region Camera
        
        [field: Space(10f)]
        [field: Header("Camera")]
        [field: SerializeField] public float screenWidth {get; private set; } = 6f;
        [field: SerializeField] public float screenHeight {get; private set; } = 12f;
        [field: SerializeField] public Vector3 camCenter {get; private set; } = new Vector3(0, -1, -10);
        [field: SerializeField] public float shakeDuration {get; private set; } = .1f;
        [field: SerializeField] public float heavyShakeDuration {get; private set;} = .4f;
        
        #endregion

        #region Camera Shake
        
        [field: Header("Camera Shake")]
        [field: SerializeField] public float minDist {get; private set; } = 0.04f;
        [field: SerializeField] public float maxDist {get; private set; } = 0.05f;

        #endregion
        
        #region SFX
        
        [field: Space(10f)]
        [field: Header("SFX")]
        [field: SerializeField] public AudioClip blockPickupSfx { get; private set; }
        [field: SerializeField] public AudioClip blockPlacementSfx { get; private set; }
        [field: SerializeField] public AudioClip lineRemovalSfx { get; private set; }
        [field: SerializeField] public AudioClip gameOverSfx { get; private set; }
        [field: SerializeField] public AudioClip buttonSfx { get; private set; }
        [field: SerializeField] public AudioClip scoreCountingSfx { get; private set; }
        [field: SerializeField] public AudioClip fieldFillingSfx { get; private set; }
        [field: SerializeField] public AudioClip newBestSfx { get; private set; }
    
        #endregion
        
        #region Score
        
        [field: Space(10f)]
        [field: Header("Score")]
        [field: SerializeField] public int lineRemovalScoreMultiplier {get; private set;} = 10;
        [field: SerializeField] public int multipleLinesRemovalScoreMultiplier {get; private set;} = 50;
        [field: SerializeField] public int comboScoreMultiplier {get; private set;} = 50;
        [field: SerializeField] public int resetComboAfterMoves {get; private set;} = 3;
        [field: SerializeField] public int allClearBonus {get; private set;} = 1000;
    
        #endregion
        
        #region UI Animations
        
        [field: Space(10f)]
        [field: Header("UI Animations")]
        [field: SerializeField] public float comboAnimationDuration {get; private set;} = .75f;
        [field: SerializeField] public float allClearTextAnimationDuration {get; private set;} = .75f;
        [field: SerializeField] public float scoreUpdateAnimationDuration {get; private set;} = 2.5f;
        [field: SerializeField] public float scoreHeartBeatFrequency {get; private set;} = .15f;
        [field: SerializeField] public float waitBeforeGameOverMenuAppears {get; private set;} = 2.5f;
        [field: SerializeField] public float gameOverMenuScoreAnimationDuration {get; private set;} = 1f;
        [field: SerializeField] public float newBestAnimationDuration {get; private set;} = .5f;
        [Range(0, 1)]
        [field: SerializeField] public float newBestAnimationMinAlpha {get; private set;} = .2f;
        [Range(0, 1)]
        [field: SerializeField] public float newBestAnimationMaxAlpha {get; private set;} = 1f;
    
        #endregion

        #region Saves
        
        [field: Space(10f)]
        [field: Header("Saves")]
        [field: SerializeField] public string savesFolder {get; private set;} = "Saves";
        [field: SerializeField] public string saveFileName {get; private set;} = "save.json";
        
        #endregion
        
        #region Themes
        
        [field: Space(10f)]
        [field: Header("Themes")]
        [field: SerializeField] public float themeChangeDuration {get; private set;} = 1f;
        [field: SerializeField] public int tutorialBlockCellsDefaultSpriteLayer {get; private set; } = 2;
        [field: SerializeField] public int tutorialBlockCellsPickedSpriteLayer {get; private set; } = 3;
        [field: SerializeField] public float blockPlacementPreviewDuration {get; private set;} = 1.5f;
        [field: SerializeField] public float waitForSecondsBeforePuttingBlockBack {get; private set;} = .5f;
        [field: SerializeField] public float blockPickingAnimationDuration {get; private set;} = .3f;
        
        #endregion

        #region Ads

        public int reviveSuggestionDuration = 5;

        #endregion
    }
}
