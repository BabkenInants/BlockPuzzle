using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FieldGraphics : MonoBehaviour
{
    public bool isReady{get; private set;}
    public int activeAnimationCoroutines { get; private set; } = 0;
    [field:SerializeField] public Transform firstCell{get; private set;}
    [SerializeField] private Settings settings;
    [SerializeField] private ParticleSystem lineRemovalParticles;
    private Transform[,] _fieldCells;
    private SpriteRenderer[,] _spriteRenderers;
    private List<GridPos> _lastPreviewedCells;
    private List<GridPos> _lastPreviewedPotentiallyRemovedLines;
    private List<Color> _lastPreviewedPotentiallyRemovedLinesColors;

    private void Awake()
    {
        _fieldCells = new Transform[settings.rowsCount, settings.columnsCount];
        _spriteRenderers = new SpriteRenderer[settings.rowsCount, settings.columnsCount];
    }

    private void Start() => GenerateField();
    
    #region Blocks Placement

    public void PlaceBlock(GridPos[] cells, Color color)
    {
        _lastPreviewedCells = null;
        foreach (GridPos cell in cells)
        {
            _spriteRenderers[cell.Row, cell.Column].sprite = settings.notEmptyCell;
            _spriteRenderers[cell.Row, cell.Column].color = color;
        }
    }

    public void RemoveRow(int row, Color vfxColor)
    {
        activeAnimationCoroutines++;
        for (int j = 0; j < settings.columnsCount; j++)
        {
            _spriteRenderers[row, j].sprite = settings.emptyCell;
            _spriteRenderers[row, j].color = settings.defaultCellColor;
        }
        Vector3 particlesPosition = firstCell.position + new Vector3(settings.cellSize * (settings.columnsCount / 2f), -row * settings.cellSize, 0);
        ParticleSystem particles = Instantiate(lineRemovalParticles, particlesPosition, Quaternion.identity);
        particles.startColor = vfxColor;
        particles.Play();
        activeAnimationCoroutines--;
    }
    
    public void RemoveColumn(int col, bool[] fullRows, Color vfxColor)
    {
        activeAnimationCoroutines++;
        for (int i = 0; i < settings.rowsCount; i++)
        {
            if(fullRows[i]) continue;
            _spriteRenderers[i, col].sprite = settings.emptyCell;
            _spriteRenderers[i, col].color = settings.defaultCellColor;
        }
        Vector3 particlesPosition = firstCell.position + new Vector3(col * settings.cellSize, -settings.cellSize * (settings.rowsCount / 2f),  0);
        ParticleSystem particles = Instantiate(lineRemovalParticles, particlesPosition, Quaternion.Euler(0, 0, 90));
        particles.startColor = vfxColor;
        particles.Play();
        activeAnimationCoroutines--;
    } 

    #endregion
    
    #region Previewing
    
    //Implement only after checking if the cells are free
    public void PreviewCells(Transform[] cells)
    {
        if (_lastPreviewedCells != null)
            HideCellsPreview();
        _lastPreviewedCells = new List<GridPos>();
        for (int i = 0; i < cells.Length; i++)
        {
            GridPos position = FieldUtils.GetCellCoordinatesOnField(cells[i].position, firstCell.position, 
                settings.cellSize, true, settings.columnsCount, settings.rowsCount);
            _lastPreviewedCells.Add(position);
            _spriteRenderers[position.Row, position.Column].color = settings.cellPreviewColor;
        }
    }

    public void HideCellsPreview()
    {
        List<GridPos> cells = _lastPreviewedCells;
        if (cells == null) return;
        foreach (GridPos cell in cells)
            _spriteRenderers[cell.Row, cell.Column].color = settings.defaultCellColor;
        _lastPreviewedCells = null;
    }

    public void PreviewPotentiallyRemovedLines(List<GridPos> cells, Color color)
    {
        if(_lastPreviewedPotentiallyRemovedLines != null)
            HidePotentiallyRemovedLinesPreview();
        _lastPreviewedPotentiallyRemovedLines = new List<GridPos>();
        _lastPreviewedPotentiallyRemovedLinesColors = new List<Color>();
        for (int i = 0; i < cells.Count; i++)
        {
            _lastPreviewedPotentiallyRemovedLines.Add(cells[i]);
            _lastPreviewedPotentiallyRemovedLinesColors.Add(_spriteRenderers[cells[i].Row, cells[i].Column].color);
            _spriteRenderers[cells[i].Row, cells[i].Column].color = color;
        }
    }

    public void HidePotentiallyRemovedLinesPreview()
    {
        if(_lastPreviewedPotentiallyRemovedLines == null ||
           _lastPreviewedPotentiallyRemovedLinesColors == null) return;
        for (var i = 0; i < _lastPreviewedPotentiallyRemovedLines.Count; i++)
        {
            _spriteRenderers[_lastPreviewedPotentiallyRemovedLines[i].Row, 
                    _lastPreviewedPotentiallyRemovedLines[i].Column].color = 
                _lastPreviewedPotentiallyRemovedLinesColors[i];
        }
        _lastPreviewedPotentiallyRemovedLines = null;
        _lastPreviewedPotentiallyRemovedLinesColors = null;
    }

    #endregion
    
    private void GenerateField()
    {
        _fieldCells[0, 0] = firstCell;
        _spriteRenderers[0, 0] = firstCell.GetComponent<SpriteRenderer>();
        for (int i = 0; i < settings.rowsCount; i++)
        {
            for (int j = 0; j < settings.columnsCount; j++)
            {
                if (i == 0 && j == 0) continue;
                Vector3 position = firstCell.position + new Vector3(j * settings.cellSize, -i * settings.cellSize, 0f);
                _fieldCells[i, j] = Instantiate(settings.cellPrefab, position, 
                    Quaternion.identity, transform).transform;
                _spriteRenderers[i, j] = _fieldCells[i, j].GetComponent<SpriteRenderer>();
            }
        }
        isReady = true;
    }
}
