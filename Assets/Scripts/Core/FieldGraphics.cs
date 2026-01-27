using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using Saves;

namespace Core
{
    public class FieldGraphics : MonoBehaviour, ISavable
    {
        public bool isReady {get; private set;}
        [field:SerializeField] public Transform firstCell {get; private set;}
        [SerializeField] private Settings settings;
        [SerializeField] private ParticleSystem lineRemovalParticles;
        [SerializeField] private Animator lineRemovalAnimator;
        [SerializeField] private AnimationClip lineRemovalAnimation;
        private Transform[,] _fieldCells;
        private SpriteRenderer[,] _spriteRenderers;
        private List<GridPos> _lastPreviewedCells;
        private List<GridPos> _lastPreviewedPotentiallyRemovedLines;
        private List<Color> _lastPreviewedPotentiallyRemovedLinesColors;
        private Queue<ParticleSystem> _lineRemovalParticlesPool = new Queue<ParticleSystem>();
        private Queue<Animator> _lineRemovalAnimationPool = new Queue<Animator>();

        private void Awake()
        {
            _fieldCells = new Transform[settings.rowsCount, settings.columnsCount];
            _spriteRenderers = new SpriteRenderer[settings.rowsCount, settings.columnsCount];
            GenerateField();
        }

        private void OnEnable() => GameEvents.OnGameOver += FillFieldWithRandomBlocks;
    
        private void OnDisable() => GameEvents.OnGameOver -= FillFieldWithRandomBlocks;

        #region Particle System Pool

        private void AddParticle()
        {
            //particles
            ParticleSystem particles = Instantiate(lineRemovalParticles, transform);
            ParticleSystem.MainModule main = particles.main;
            main.playOnAwake = false;
            main.loop = false;
            particles.Stop();
            particles.gameObject.SetActive(false);
            _lineRemovalParticlesPool.Enqueue(particles);
        
            //animation
            Animator animator = Instantiate(lineRemovalAnimator, transform);
            animator.gameObject.SetActive(false);
            _lineRemovalAnimationPool.Enqueue(animator);
        }

        private void PlayParticles(Vector3 position, Quaternion rotation, Color color)
        {
            if(_lineRemovalParticlesPool.Count == 0)
                AddParticle();
            Animator animator = _lineRemovalAnimationPool.Dequeue();
            animator.transform.position =  position;
            animator.transform.rotation = rotation;
            animator.gameObject.SetActive(true);
            animator.GetComponent<SpriteRenderer>().color = color;
            StartCoroutine(PlayAnimationAndEnqueueAtTheEnd(animator));
            ParticleSystem particles = _lineRemovalParticlesPool.Dequeue();
            particles.transform.position = position;
            particles.transform.rotation = rotation;
            ParticleSystem.MainModule main = particles.main;
            main.startColor = color;
            particles.gameObject.SetActive(true);
            StartCoroutine(PlayParticlesAndEnqueueAtTheEnd(particles));
        }

        private IEnumerator PlayParticlesAndEnqueueAtTheEnd(ParticleSystem particles)
        {
            particles.Play();
            yield return new WaitForSeconds(particles.main.duration + .1f);
            particles.Stop();
            particles.Clear();
            particles.gameObject.SetActive(false);
            _lineRemovalParticlesPool.Enqueue(particles);
        }

        private IEnumerator PlayAnimationAndEnqueueAtTheEnd(Animator animator)
        {
            animator.Play("Appear");
            yield return new WaitForSeconds(lineRemovalAnimation.length);
            animator.StopPlayback();
            animator.gameObject.SetActive(false);
            _lineRemovalAnimationPool.Enqueue(animator);
        }

        #endregion
    
        #region Blocks Placement

        public void PlaceBlock(GridPos[] cells, Color color)
        {
            foreach (GridPos cell in cells)
            {
                _spriteRenderers[cell.Row, cell.Column].sprite = settings.busyCell;
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
            particlesPosition.x -= settings.cellSize / 2;
            PlayParticles(particlesPosition, Quaternion.identity, vfxColor);
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
            particlesPosition.y += settings.cellSize / 2;
            PlayParticles(particlesPosition, Quaternion.Euler(0, 0, 90), vfxColor);
        }

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
                _spriteRenderers[cells[i].Row, cells[i].Column].sprite = settings.busyCell;
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

        #region Random Field Fill
    
        private void FillFieldWithRandomBlocks() =>
            StartCoroutine(FillFieldWithRandomBlocksRoutine());

        private IEnumerator FillFieldWithRandomBlocksRoutine()
        {
            GameEvents.RaisePlaySfx(settings.fieldFillingSfx);
            for (int row = settings.rowsCount - 1; row >= 0; row--)
            {
                for (var col = 0; col < settings.columnsCount; col++)
                    if (_spriteRenderers[row, col].sprite == settings.emptyCell)
                    {
                        _spriteRenderers[row, col].sprite = settings.busyCell;
                        Color temp = settings.colors[Random.Range(0, settings.colors.Length)];
                        temp.a = 0;
                        _spriteRenderers[row, col].color = temp;
                        StartCoroutine(ColorAlphaTransition(_spriteRenderers[row, col], 0, 1,
                            (settings.waitBeforeGameOverMenuAppears - settings.rowsCount * settings.waitTimeBetweenRows) / 8));
                    }
                yield return new WaitForSeconds(settings.waitTimeBetweenRows);
            }
        }

        private IEnumerator ColorAlphaTransition(SpriteRenderer spriteRenderer, float startAlpha, float endAlpha, float duration)
        {
            var elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                Color temp = spriteRenderer.color;
                temp.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
                spriteRenderer.color = temp;
                yield return null;
            }
            Color endColor = spriteRenderer.color;
            endColor.a = endAlpha;
            spriteRenderer.color = endColor;
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
    
        #region Saves

        public void Save(SaveData saveData)
        {
            if (saveData.GameIsOver) return;
            saveData.SpriteRenderersColors = new SerializableColor[settings.rowsCount * settings.columnsCount];
            for (var row = 0; row < settings.rowsCount; row++)
            for (var col = 0; col < settings.columnsCount; col++)
                saveData.SpriteRenderersColors[row * settings.columnsCount + col] =
                    new SerializableColor(_spriteRenderers[row, col].color);
        }

        public void Load(SaveData saveData)
        {
            if (saveData.GameIsOver) return;
            for (var row = 0; row < settings.rowsCount; row++)
            for (var col = 0; col < settings.columnsCount; col++)
                if (!saveData.CellIsFree[row * settings.columnsCount + col])
                {
                    _spriteRenderers[row, col].color = saveData.SpriteRenderersColors
                        [row * settings.columnsCount + col].ToColor();
                    _spriteRenderers[row, col].sprite = settings.busyCell;
                }
        }
    
        #endregion
    }
}
