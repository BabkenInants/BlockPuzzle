using System.Collections;
using UnityEngine;
using Core;

namespace Tutorial
{
    public class TutorialBlock : MonoBehaviour
    {
        public Transform[] cells;
        public Vector3 positionOffset;
        private Camera _mainCam;
        private float _notPickedSize;
        private Vector3 _endPos;
        private bool _isPicked;
        private Vector3 _startPos;
        private Settings _settings;
        private IEnumerator _sizeChangeCoroutine;
        private IEnumerator _previewCoroutine;

        private void Start()
        {
            _mainCam = Camera.main;
            _previewCoroutine = PreviewRoutine();
            StartCoroutine(_previewCoroutine);
        }

        private IEnumerator PreviewRoutine()
        {
            float duration = _settings.blockPlacementPreviewDuration;
            while (true)
            {
                var elapsedTime = 0f;
                PickBlock(transform.position);
                while (elapsedTime < duration)
                {
                    elapsedTime += Time.deltaTime;
                    Vector3 position = Vector3.Lerp(_startPos, _endPos, elapsedTime / duration);
                    UpdatePosition(position);
                    yield return null;
                }
                yield return new WaitForSeconds(_settings.waitForSecondsBeforePuttingBlockBack);
                PutBlockBack();
                yield return new WaitForSeconds(_settings.blockPickingAnimationDuration);
            }
        }
        
        private void HandleOnBlockPicked(Block block)
        {
            if(_previewCoroutine != null)
                StopCoroutine(_previewCoroutine);
            PutBlockBack(false);
        }

        private void HandeOnBlockUnpicked(Block block)
        {
            _previewCoroutine = PreviewRoutine();
            StartCoroutine(_previewCoroutine);
        }

        private void OnEnable()
        {
            GameEvents.OnBlockPicked += HandleOnBlockPicked;
            GameEvents.OnBlockUnpicked += HandeOnBlockUnpicked;
        }

        private void OnDisable()
        {
            GameEvents.OnBlockPicked -= HandleOnBlockPicked;
            GameEvents.OnBlockUnpicked -= HandeOnBlockUnpicked;
        }

        #region Init

        public void Init(Settings settings, float notPickedSize, Vector3 endPos, Block block, Color color)
        {
            _settings = settings;
            _notPickedSize = notPickedSize;
            SetSize(notPickedSize);
            _endPos = endPos;
            SetColor(color);
        }

        #endregion

        #region Preview

        private void PickBlock(Vector2 pos)
        {
            if (!_settings || !_mainCam) return;
            _isPicked = true;
            _startPos = transform.position;
            SetSize(_settings.cellSize * 2, _settings.blockPickingAnimationDuration);
            foreach (Transform cell in cells)
            {
                cell.GetComponent<SpriteRenderer>().enabled = true;
                cell.GetComponent<SpriteRenderer>().sortingOrder = _settings.tutorialBlockCellsPickedSpriteLayer;
            }
        }

        private void UpdatePosition(Vector2 pos)
        {
            if (!_isPicked) return;
            transform.position = pos;
        }

        private void PutBlockBack(bool spriteRendererIsEnabled = true)
        {
            _isPicked = false;
            SetSize(_notPickedSize, 0);
            foreach (Transform cell in cells)
            {
                cell.GetComponent<SpriteRenderer>().sortingOrder = _settings.notPickedBlockCellsSpriteLayer;
                cell.GetComponent<SpriteRenderer>().enabled = spriteRendererIsEnabled;
            }
            transform.position = _startPos;
        }

        #endregion

        #region Visuals

        private void SetColor(Color newColor)
        {
            foreach (var cell in cells)
                cell.GetComponent<SpriteRenderer>().color = newColor;
        }

        private void SetSize(float size, float duration = .05f)
        {
            if (_sizeChangeCoroutine != null) StopCoroutine(_sizeChangeCoroutine);
            _sizeChangeCoroutine = SizeChangeRoutine(transform.localScale, new Vector3(size, size, 1), duration);
            StartCoroutine(_sizeChangeCoroutine);
        }

        #endregion

        #region Coroutines

        private IEnumerator SizeChangeRoutine(Vector3 startSize, Vector3 endSize, float duration)
        {
            var estimatedTime = 0f;
            while (estimatedTime < duration)
            {
                estimatedTime += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startSize, endSize, estimatedTime / duration);
                yield return null;
            }
            transform.localScale = endSize;
            _sizeChangeCoroutine = null;
        }

        #endregion
    }
}
