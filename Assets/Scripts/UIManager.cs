using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Settings settings;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI comboText;
    private bool _gameIsOver;
    private IEnumerator _scoreUpdateCoroutine;
    private IEnumerator _comboCoroutine;
    private int _lastScore;
    private int _bestScore;
    private int _lastCombo;

    private void Start()
    {
        _bestScore = PlayerPrefs.GetInt("BestScore");
        bestScoreText.text = _bestScore.ToString();
    }
    
    private void EndGame()
    {
        if (_gameIsOver) return;
        StartCoroutine(EndGameRoutine());
    }

    private IEnumerator EndGameRoutine()
    {
        GameEvents.RaisePlaySfx(settings.gameOverSfx);
        yield return new WaitForSeconds(settings.waitBeforeGameOverMenuAppears);
        gameOverMenu.SetActive(true);
        _gameIsOver = true;
    }
    
    public void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    private void UpdateScore(int score)
    {
        if(_scoreUpdateCoroutine != null)
            StopCoroutine(_scoreUpdateCoroutine);
        _scoreUpdateCoroutine = UpdateScoreRoutine(score, settings.scoreUpdateAnimationDuration);
        StartCoroutine(_scoreUpdateCoroutine);
    }

    private IEnumerator UpdateScoreRoutine(int endScore, float duration)
    {
        float elapsedTime = 0;
        if (endScore > _bestScore)
            PlayerPrefs.SetInt("BestScore", endScore);
        if (endScore - _lastScore < 10)
            duration = .5f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            _lastScore = Mathf.FloorToInt(Mathf.Lerp(_lastScore, endScore, elapsedTime / duration));
            if(endScore > _bestScore) bestScoreText.text = _lastScore.ToString();
            scoreText.text = _lastScore.ToString();
            yield return null;
        }
        if(endScore > _bestScore) _bestScore = endScore;
        _lastScore = endScore;
        _scoreUpdateCoroutine = null;
    }

    private void ShowCombo(int combo, int lastCombo)
    {
        if(_comboCoroutine != null)
            StopCoroutine(_comboCoroutine);
        _lastCombo = lastCombo;
        _comboCoroutine = ShowComboRoutine(combo, settings.comboAnimationDuration);
        StartCoroutine(_comboCoroutine);
    }

    private IEnumerator ShowComboRoutine(int combo, float duration)
    {
        //showing text
        comboText.gameObject.SetActive(true);
        comboText.text = _lastCombo <= 1? "Combo" : $"Combo {_lastCombo}";
        yield return SizeChangeRoutine(comboText.rectTransform, Vector3.zero, new Vector3(1.2f, 1.2f, 1.2f), duration / 3f * .8f);
        yield return SizeChangeRoutine(comboText.rectTransform, new Vector3(1.2f, 1.2f, 1.2f), Vector3.one, duration / 3f * .2f);
        
        //combo++ animation
        int diff = combo - _lastCombo;
        float t = duration / 3 / diff;
        while (_lastCombo < combo)
        {
            _lastCombo++;
            comboText.text = _lastCombo <= 1? "Combo" : $"Combo {_lastCombo}";
            yield return new WaitForSeconds(t);
        }
        
        //hiding text
        yield return SizeChangeRoutine(comboText.rectTransform, Vector3.one, Vector3.zero, duration / 3f);
        comboText.gameObject.SetActive(false);
        _comboCoroutine = null;
    } 
    
    private IEnumerator SizeChangeRoutine(RectTransform rectTransform, Vector3 startSize, Vector3 endSize, float duration)
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

    private void OnEnable() => Subscribe();

    private void OnDisable() => Unsubscribe();

    private void Subscribe()
    {
        GameEvents.OnGameOver += EndGame;
        GameEvents.UpdateScore += UpdateScore;
        GameEvents.ShowCombo += ShowCombo;
    }

    private void Unsubscribe()
    {
        GameEvents.OnGameOver -= EndGame;
        GameEvents.UpdateScore -= UpdateScore;
        GameEvents.ShowCombo -= ShowCombo;
    }
}
