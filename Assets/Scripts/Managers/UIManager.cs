using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Core;
using Saves;
using Themes;
using UnityEngine.UI;
using YG.LanguageLegacy;

namespace Managers
{
    public class UIManager : MonoBehaviour, ISavable, IThemeReceiver
    {
        #region Variables
        
        [SerializeField] private Settings settings;
        [SerializeField] private GameObject gameOverMenu;
        [SerializeField] private GameObject settingsMenu;
        [SerializeField] private TextMeshProUGUI gameOverScoreText;
        [SerializeField] private TextMeshProUGUI gameOverBestScoreText;
        [SerializeField] private TextMeshProUGUI gameOverNewBestText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI bestScoreText;
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private TextMeshProUGUI allClearText;
        [Header("Theme")] 
        [SerializeField] private Image[] fieldColor;
        [SerializeField] private Image[] backgroundColor;
        [SerializeField] private Image[] emptyCellColor;
        [SerializeField] private Image[] primaryTextColorImages;
        [SerializeField] private TextMeshProUGUI[] primaryTextColorTexts;
        [SerializeField] private TextMeshProUGUI[] secondaryTextColorTexts;
        [SerializeField] private TextMeshProUGUI[] tertiaryTextColorTexts;
        [Header("Tutorial")] 
        [SerializeField] private GameObject settingsButton;
        private bool _gameIsOver;
        private IEnumerator _scoreUpdateCoroutine;
        private IEnumerator _comboCoroutine;
        private IEnumerator _allClearCoroutine;
        private IEnumerator _comboAnimationCoroutine;
        private IEnumerator _restartButtonCoroutine;
        private int _lastScore;
        private int _endScore;
        private int _bestScore;
        private int _lastCombo;
        private bool _isCombo;
        private bool _tutorialMode;

        #endregion

        private void ButtonFeedback()
        {
            GameEvents.RaisePlayHaptics(HapticManager.HapticType.Light);
            GameEvents.RaisePlaySfx(settings.buttonSfx);
        }
    
        #region Game Over Menu

        private void EndGame()
        {
            if (_gameIsOver) return;
            StartCoroutine(EndGameRoutine());
        }

        private IEnumerator EndGameRoutine()
        {
            _gameIsOver = true;
            
            //waiting for field filling animation
            yield return new WaitForSeconds(settings.waitBeforeGameOverMenuAppears - (settings.gameOverSfx? settings.gameOverSfx.length : 0));
            GameEvents.RaisePlaySfx(settings.gameOverSfx);
            if(settings.buttonSfx)
                yield return new WaitForSeconds(settings.gameOverSfx.length);
            gameOverMenu.SetActive(true);
            
            //score animation
            var elapsedTime = 0f;
            float duration = settings.gameOverMenuScoreAnimationDuration;
            GameEvents.RaisePlaySfx(settings.scoreCountingSfx);
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                int tempScore = Mathf.FloorToInt(Mathf.Lerp(0, _endScore, elapsedTime / duration));
                int tempBestScore = Mathf.FloorToInt(Mathf.Lerp(0, _bestScore, elapsedTime / duration));
                gameOverScoreText.text = tempScore.ToString();
                gameOverBestScoreText.text = tempBestScore.ToString();
                yield return null;
            }
            
            //showing new best text if necessary
            if (_endScore == _bestScore)
            {
                gameOverNewBestText.gameObject.SetActive(true);
                GameEvents.RaisePlaySfx(settings.newBestSfx);
                StartCoroutine(ColorAlphaBlinkRoutine(gameOverNewBestText, settings.newBestAnimationMinAlpha, 
                    settings.newBestAnimationMaxAlpha));
            }
            
            gameOverScoreText.text = _endScore.ToString();
            gameOverBestScoreText.text = _bestScore.ToString();
        }

        public void Restart()
        {
            if (_restartButtonCoroutine != null) return;
            _restartButtonCoroutine = RestartButtonRoutine();
            StartCoroutine(_restartButtonCoroutine);
        }

        private IEnumerator RestartButtonRoutine()
        {
            ButtonFeedback();
            if(settings.buttonSfx)
                yield return new WaitForSeconds(settings.buttonSfx.length);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        #endregion

        #region Score

        private void UpdateScore(int score, bool updateBestScore)
        {
            if(_scoreUpdateCoroutine != null)
                StopCoroutine(_scoreUpdateCoroutine);
            _endScore = score;
            _scoreUpdateCoroutine = UpdateScoreRoutine(settings.scoreUpdateAnimationDuration, updateBestScore, _bestScore);
            if (updateBestScore) _bestScore = _endScore;
            StartCoroutine(_scoreUpdateCoroutine);
        }

        private IEnumerator UpdateScoreRoutine(float duration, bool updateBestScore, int prevBestScore)
        {
            float elapsedTime = 0;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                _lastScore = Mathf.FloorToInt(Mathf.Lerp(_lastScore, _endScore, elapsedTime / duration));
                if (updateBestScore && prevBestScore <= _lastScore) bestScoreText.text = _lastScore.ToString();
                scoreText.text = _lastScore.ToString();
                yield return null;
            }
            _lastScore = _endScore;
            _scoreUpdateCoroutine = null;
        }

        #endregion
    
        #region Combo
    
        private void ShowCombo(int combo, int lastCombo)
        {
            if(_tutorialMode) return;
            
            _isCombo = true;
            //score animation during combo
            if (_comboAnimationCoroutine == null)
            {
                _comboAnimationCoroutine = ComboScoreAnimationRoutine();
                StartCoroutine(_comboAnimationCoroutine);
            }

            //showing combo text
            if(_comboCoroutine != null)
                StopCoroutine(_comboCoroutine);
            _lastCombo = lastCombo;
            _comboCoroutine = ShowComboRoutine(combo, settings.comboAnimationDuration);
            StartCoroutine(_comboCoroutine);
        }

        private void EndCombo() => _isCombo = false;

        private IEnumerator ShowComboRoutine(int combo, float duration)
        {
            //showing text
            comboText.gameObject.SetActive(true);
            string localizedCombo = comboText.GetComponent<LanguageYG>().lastTranslation;
            comboText.text = _lastCombo <= 1? localizedCombo : $"{localizedCombo} {_lastCombo}";
            yield return SizeChangeRoutine(comboText.rectTransform, Vector3.zero, new Vector3(1.2f, 1.2f, 1.2f), duration / 3f * .8f);
            yield return SizeChangeRoutine(comboText.rectTransform, new Vector3(1.2f, 1.2f, 1.2f), Vector3.one, duration / 3f * .2f);
        
            //combo++ animation
            int diff = combo - _lastCombo;
            float t = duration / 3 / diff;
            while (_lastCombo < combo)
            {
                _lastCombo++;
                comboText.text = _lastCombo <= 1? localizedCombo : $"{localizedCombo} {_lastCombo}";
                yield return new WaitForSeconds(t);
            }
        
            //hiding text
            yield return SizeChangeRoutine(comboText.rectTransform, Vector3.one, Vector3.zero, duration / 3f);
            comboText.gameObject.SetActive(false);
            _comboCoroutine = null;
        } 
    
        private IEnumerator ComboScoreAnimationRoutine()
        {
            while (_isCombo)
            {
                yield return SizeChangeRoutine(scoreText.rectTransform, Vector3.one, new Vector3(0.9f, 0.9f, 0.9f), settings.scoreHeartBeatFrequency);
                yield return SizeChangeRoutine(scoreText.rectTransform, new Vector3(0.9f, 0.9f, 0.9f), Vector3.one, settings.scoreHeartBeatFrequency);
            }
            _comboAnimationCoroutine = null;
        }
    
        #endregion

        #region All Clear

        private void ShowAllClear()
        {
            if(_tutorialMode) return;
            if(_allClearCoroutine != null) StopCoroutine(_allClearCoroutine);
            _allClearCoroutine = AllClearRoutine(settings.allClearTextAnimationDuration);
            StartCoroutine(_allClearCoroutine);
        }

        private IEnumerator AllClearRoutine(float duration)
        {
            allClearText.rectTransform.localScale = Vector3.zero;
            allClearText.gameObject.SetActive(true);
            yield return SizeChangeRoutine(allClearText.rectTransform, Vector3.zero, new Vector3(1.2f, 1.2f, 1.2f), duration/3f * .8f);
            yield return SizeChangeRoutine(allClearText.rectTransform,  new Vector3(1.2f, 1.2f, 1.2f), Vector3.one, duration / 3f * .2f);
            yield return new WaitForSeconds(duration / 3f);
            yield return SizeChangeRoutine(allClearText.rectTransform, Vector3.one, Vector3.zero, duration / 3f);
            allClearText.gameObject.SetActive(false);
            _allClearCoroutine = null;
        }

        #endregion

        #region Settings

        public void OpenSettings()
        {
            ButtonFeedback();
            settingsMenu.SetActive(true);
        }

        public void CloseSettings()
        {
            ButtonFeedback();
            settingsMenu.SetActive(false);
        }

        #endregion

        #region Themes

        public void ReceiveTheme(Theme theme)
        {
            var duration = settings.themeChangeDuration;
            foreach (var img in fieldColor)
                StartCoroutine(ThemeTools.SetImageColor(img, img.color, theme.fieldColor, duration));
            foreach (var img in backgroundColor)    
                StartCoroutine(ThemeTools.SetImageColor(img, img.color, theme.backgroundColor, duration));
            foreach (var img in emptyCellColor)
                StartCoroutine(ThemeTools.SetImageColor(img, img.color, theme.cellDefaultColor, duration));
            foreach (var text in primaryTextColorTexts)
                StartCoroutine(ThemeTools.SetTextColor(text, scoreText.color, theme.primaryTextColor, duration));
            foreach (var text in secondaryTextColorTexts)
                StartCoroutine(ThemeTools.SetTextColor(text, scoreText.color, theme.secondaryTextColor, duration));
            foreach (var text in tertiaryTextColorTexts)
                StartCoroutine(ThemeTools.SetTextColor(text, scoreText.color, theme.tertiaryTextColor, duration));
            foreach (var img in primaryTextColorImages)
                StartCoroutine(ThemeTools.SetImageColor(img, img.color, theme.primaryTextColor, duration));
        }

        public void ReceiveThemeOnGameStart(Theme theme)
        {
            foreach (var img in fieldColor)
                img.color = theme.fieldColor;
            foreach (var img in backgroundColor)
                img.color = theme.backgroundColor;
            foreach (var img in emptyCellColor)
                img.color = theme.cellDefaultColor;
            foreach (var text in primaryTextColorTexts) 
                text.color =  theme.primaryTextColor;
            foreach (var text in secondaryTextColorTexts) 
                text.color =  theme.secondaryTextColor;
            foreach (var text in tertiaryTextColorTexts) 
                text.color =  theme.tertiaryTextColor;
            foreach (var img in primaryTextColorImages)
                img.color =  theme.primaryTextColor;
        }

        #endregion

        #region Tutorial

        private void StartTutorial()
        {
            _tutorialMode = true;
            settingsButton.SetActive(false);
        }

        private void EndTutorial()
        {
            _tutorialMode = false;
            settingsButton.SetActive(true);
        }

        #endregion

        #region Coroutines

        private IEnumerator ColorAlphaBlinkRoutine(TextMeshProUGUI img, float minAlpha, float maxAlpha)
        {
            var asc = true;
            Color colorMinAlpha = img.color;
            colorMinAlpha.a = minAlpha;
            Color colorMaxAlpha = img.color;
            colorMaxAlpha.a = maxAlpha;
            while (true)
            {
                yield return ThemeTools.SetTextColor(img, asc? colorMinAlpha : colorMaxAlpha, 
                    asc? colorMaxAlpha : colorMinAlpha, settings.newBestAnimationDuration);
                asc = !asc;
            }
        }
    
        private static IEnumerator SizeChangeRoutine(RectTransform rectTransform, Vector3 startSize, Vector3 endSize, float duration)
        {
            var estimatedTime = 0f;
            while (estimatedTime < duration)
            {
                estimatedTime += Time.deltaTime;
                rectTransform.localScale = Vector3.Lerp(startSize, endSize, estimatedTime / duration);
                yield return null;
            }
            rectTransform.localScale = endSize;
        }

        #endregion
        
        #region Events
        
        private void OnEnable()
        {
            GameEvents.OnGameOver += EndGame;
            GameEvents.UpdateScore += UpdateScore;
            GameEvents.ShowCombo += ShowCombo;
            GameEvents.ShowAllClearBonus += ShowAllClear;
            GameEvents.OnComboEnded += EndCombo;
            GameEvents.StartTutorial += StartTutorial;
            GameEvents.FinishTutorial += EndTutorial;
        }

        private void OnDisable()
        {
            GameEvents.OnGameOver -= EndGame;
            GameEvents.UpdateScore -= UpdateScore;
            GameEvents.ShowCombo -= ShowCombo;
            GameEvents.ShowAllClearBonus -= ShowAllClear;
            GameEvents.OnComboEnded -= EndCombo;
            GameEvents.StartTutorial -= StartTutorial;
            GameEvents.FinishTutorial -= EndTutorial;
        }

        #endregion
    
        #region Saves

        public void Save(SaveData saveData)
        {
            if(_gameIsOver) return;
            saveData.LastCombo = _lastCombo;
            saveData.IsCombo = _isCombo;
        }

        public void Load(SaveData saveData)
        {
            _bestScore = saveData.BestScore;
            bestScoreText.text = _bestScore.ToString();
            if(saveData.GameIsOver) return;
            _lastScore = saveData.Score;
            _endScore = saveData.Score;
            _lastCombo = saveData.LastCombo;
            _isCombo = saveData.IsCombo;
            scoreText.text = _endScore.ToString();
            if (_isCombo)
            {
                _comboAnimationCoroutine = ComboScoreAnimationRoutine();
                StartCoroutine(_comboAnimationCoroutine);
            }
        }
    
        #endregion
    }
}
