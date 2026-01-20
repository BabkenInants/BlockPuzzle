using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FieldGraphics : MonoBehaviour
{
    public bool isReady {get; private set;}
    [field:SerializeField] public Transform firstCell {get; private set;}
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
        foreach (GridPos cell in cells)
        {
            _spriteRenderers[cell.Row, cell.Column].sprite = settings.notEmptyCell;
            _spriteRenderers[cell.Row, cell.Column].color = color;
        }
    }

    public void RemoveRow(int row, Color vfxColor)
    {
        for (var j = 0; j < settings.columnsCount; j++)
        {
            _spriteRenderers[row, j].sprite = settings.emptyCell;
            _spriteRenderers[row, j].color = settings.defaultCellColor;
        }
        Vector3 particlesPosition = firstCell.position + new Vector3(settings.cellSize * (settings.columnsCount / 2f), -row * settings.cellSize, 0);
        SpawnParticles(vfxColor, particlesPosition, Quaternion.identity);
    }
    
    public void RemoveColumn(int col, bool[] fullRows, Color vfxColor)
    {
        for (var i = 0; i < settings.rowsCount; i++)
        {
            if(fullRows[i]) continue;
            _spriteRenderers[i, col].sprite = settings.emptyCell;
            _spriteRenderers[i, col].color = settings.defaultCellColor;
        }
        Vector3 particlesPosition = firstCell.position + new Vector3(col * settings.cellSize, -settings.cellSize * (settings.rowsCount / 2f),  0);
        SpawnParticles(vfxColor, particlesPosition, Quaternion.Euler(0, 0, 90));
    }

    #region VFX

    private void SpawnParticles(Color color, Vector3 particlesPosition, Quaternion particlesRotation)
    {
        ParticleSystem particles = Instantiate(lineRemovalParticles, particlesPosition, particlesRotation);
        ParticleSystem.MainModule mainModule = particles.main;
        mainModule.startColor = color;
        particles.Play();
    }

    #endregion

    #endregion
    
    #region Previewing
    
    ///Implement only after checking if the cells are free
    public void PreviewCells(GridPos[] cells, Color color)
    {
        if (_lastPreviewedCells != null)
            HideCellsPreview();
        _lastPreviewedCells = cells.ToList();
        Color tempColor = color;
        tempColor.a = .25f;
        for (var i = 0; i < cells.Length; i++)
        {
            _spriteRenderers[cells[i].Row, cells[i].Column].color = tempColor;
            _spriteRenderers[cells[i].Row, cells[i].Column].sprite = settings.notEmptyCell;
        }
    }

    public void HideCellsPreview()
    {
        if (_lastPreviewedCells == null) return;
        foreach (GridPos cell in _lastPreviewedCells)
        {
            _spriteRenderers[cell.Row, cell.Column].color = settings.defaultCellColor;
            _spriteRenderers[cell.Row, cell.Column].sprite = settings.emptyCell;
        }
        _lastPreviewedCells = null;
    }

    public void PreviewPotentiallyRemovedLines(List<GridPos> cells, Color previewColor)
    {
        if(_lastPreviewedPotentiallyRemovedLines != null)
            HidePotentiallyRemovedLinesPreview();
        _lastPreviewedPotentiallyRemovedLines = new List<GridPos>();
        _lastPreviewedPotentiallyRemovedLinesColors = new List<Color>();
        foreach (GridPos cell in cells)
        {
            _lastPreviewedPotentiallyRemovedLines.Add(cell);
            _lastPreviewedPotentiallyRemovedLinesColors.Add(_spriteRenderers[cell.Row, cell.Column].color);
            _spriteRenderers[cell.Row, cell.Column].color = previewColor;
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
        for (var row = 0; row < settings.rowsCount; row++)
        {
            for (var col = 0; col < settings.columnsCount; col++)
            {
                if (row == 0 && col == 0) continue;
                Vector3 position = firstCell.position + new Vector3(col * settings.cellSize, -row * settings.cellSize, 0f);
                _fieldCells[row, col] = Instantiate(settings.cellPrefab, position, 
                    Quaternion.identity, transform).transform;
                _spriteRenderers[row, col] = _fieldCells[row, col].GetComponent<SpriteRenderer>();
            }
        }
        isReady = true;
    }
}
