using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private TextMeshProUGUI scoreText;
    private bool _gameIsOver;
    private IEnumerator scoreUpdateCoroutine = null;
    private int lastScore = 0;
    
    private void EndGame()
    {
        if (_gameIsOver) return;
        gameOverMenu.SetActive(true);
        _gameIsOver = true;
    }
    
    public void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    private void UpdateScore(int score)
    {
        if(scoreUpdateCoroutine != null)
            StopCoroutine(scoreUpdateCoroutine);
        scoreUpdateCoroutine = UpdateScoreRoutine(score, 2.5f);
        StartCoroutine(scoreUpdateCoroutine);
    }

    private IEnumerator UpdateScoreRoutine(int endScore, float duration)
    {
        float elapsedTime = 0;
        if (endScore - lastScore < 10)
            duration = .5f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            lastScore = Mathf.FloorToInt(Mathf.Lerp(lastScore, endScore, elapsedTime / duration));
            scoreText.text = lastScore.ToString();
            yield return null;
        }
        lastScore = endScore;
        scoreUpdateCoroutine = null;
    }

    private void OnEnable() => Subscribe();

    private void OnDisable() => Unsubscribe();

    private void Subscribe()
    {
        GameEvents.OnGameOver += EndGame;
        GameEvents.UpdateScore += UpdateScore;
    }

    private void Unsubscribe()
    {
        GameEvents.OnGameOver -= EndGame;
        GameEvents.UpdateScore -= UpdateScore;
    }
}
