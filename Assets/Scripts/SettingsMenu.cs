using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsMenu: MonoBehaviour, ISavable
{
    private bool _sfxIsOn = true;
    private bool _hapticsIsOn = true;
    [SerializeField] private HapticManager hapticManager;
    [SerializeField] private int mainMenuBuildIndex = 0;
    [SerializeField] private Sprite toggleEnabled;
    [SerializeField] private Sprite toggleDisabled;
    [SerializeField] private Image sfxToggle;
    [SerializeField] private Image hapticsToggle;
    [SerializeField] private Button hapticsButton;

    public void Start()
    {
#if !UNITY_IOS || UNITY_EDITOR
        _hapticsIsOn = false;
        hapticsToggle.sprite = toggleDisabled;
        GameEvents.RaiseSetHapticsState(_hapticsIsOn);
        hapticsButton.interactable = false;
#endif
    }

    private void ButtonFeedback()
    {
        hapticManager.Light();
    }
    
    public void Save(SaveData saveData)
    {
        ButtonFeedback();
        saveData.SfxIsOn = _sfxIsOn;
        saveData.HapticsIsOn = _hapticsIsOn;
    }

    public void Load(SaveData saveData)
    {
        ButtonFeedback();
        _sfxIsOn = saveData.SfxIsOn;
        _hapticsIsOn = saveData.HapticsIsOn;
        sfxToggle.sprite = _sfxIsOn ? toggleEnabled : toggleDisabled;
        hapticsToggle.sprite = _hapticsIsOn ? toggleEnabled : toggleDisabled;
    }

    public void SfxButton()
    {
        ButtonFeedback();
        _sfxIsOn = !_sfxIsOn;
        sfxToggle.sprite = _sfxIsOn ? toggleEnabled : toggleDisabled;
        GameEvents.RaiseSetSfxState(_sfxIsOn);
    }

    public void HapticsButton()
    {
        ButtonFeedback();
        _hapticsIsOn = !_hapticsIsOn;
        hapticsToggle.sprite = _hapticsIsOn ? toggleEnabled : toggleDisabled;
        GameEvents.RaiseSetHapticsState(_hapticsIsOn);
    }

    public void MainMenuButton()
    {
        ButtonFeedback();
        SceneManager.LoadScene(mainMenuBuildIndex);
    }

    public void Replay()
    {
        ButtonFeedback();
        GameEvents.RaiseGameOver();
        GameEvents.RaiseSaveGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}