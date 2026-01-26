using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private int endlessModeBuildIndex = 1;
    [SerializeField] private Settings settings;
    private IEnumerator _playButtonCoroutine;
    
    private void ButtonFeedback()
    {
        GameEvents.RaisePlayHaptics(HapticManager.HapticType.Light);
        GameEvents.RaisePlaySfx(settings.buttonSfx);
    }
    
    public void Play()
    {
        if(_playButtonCoroutine != null) return;
        _playButtonCoroutine = PlayButtonRoutine();
        StartCoroutine(_playButtonCoroutine);
    }

    private IEnumerator PlayButtonRoutine()
    {
        ButtonFeedback();
        yield return new WaitForSeconds(settings.buttonSfx.length);
        SceneManager.LoadScene(endlessModeBuildIndex);
    }
}