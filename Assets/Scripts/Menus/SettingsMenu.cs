using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Core;
using Managers;
using Saves;

namespace Menus
{
    public class SettingsMenu: MonoBehaviour, ISavable
    {
        private bool _sfxIsOn = true;
        private bool _hapticsIsOn = true;
        [SerializeField] private Settings settings;
        [SerializeField] private int mainMenuBuildIndex = 0;
        [SerializeField] private Sprite toggleEnabled;
        [SerializeField] private Sprite toggleDisabled;
        [SerializeField] private Image sfxToggle;
        [SerializeField] private Image hapticsToggle;
        [SerializeField] private Button hapticsButton; 
        private IEnumerator _mainMenuButtonCoroutine;
        private IEnumerator _restartButtonCoroutine;

#if !UNITY_IOS || UNITY_EDITOR
        public void Start()
        {
            _hapticsIsOn = false;
            hapticsToggle.sprite = toggleDisabled;
            GameEvents.RaiseSetHapticsState(_hapticsIsOn);
            hapticsButton.interactable = false;
        }
#endif

        #region Buttons

        private void ButtonFeedback()
        {
            GameEvents.RaisePlayHaptics(HapticManager.HapticType.Light);
            GameEvents.RaisePlaySfx(settings.buttonSfx);
        }
        
        public void DeleteSaveButton() =>
            GameEvents.RaiseDeleteSave();

        public void SfxButton()
        {
            ButtonFeedback();
            _sfxIsOn = !_sfxIsOn;
            sfxToggle.sprite = _sfxIsOn ? toggleEnabled : toggleDisabled;
            GameEvents.RaiseSetSfxState(_sfxIsOn);
            GameEvents.RaiseSaveGame();
        }

        public void HapticsButton()
        {
            ButtonFeedback();
            _hapticsIsOn = !_hapticsIsOn;
            hapticsToggle.sprite = _hapticsIsOn ? toggleEnabled : toggleDisabled;
            GameEvents.RaiseSetHapticsState(_hapticsIsOn);
            GameEvents.RaiseSaveGame();
        }

        public void MainMenuButton()
        {
            if (_mainMenuButtonCoroutine != null) return;
            _mainMenuButtonCoroutine = MainMenuButtonRoutine();
            StartCoroutine(_mainMenuButtonCoroutine);
        }

        private IEnumerator MainMenuButtonRoutine()
        {
            ButtonFeedback();
            GameEvents.RaiseSaveGame();
            if(settings.buttonSfx)
                yield return new WaitForSeconds(settings.buttonSfx.length);
            SceneManager.LoadScene(mainMenuBuildIndex);
        }
    
        public void Restart()
        {
            if(_restartButtonCoroutine != null) return;
            _restartButtonCoroutine = RestartButtonRoutine();
            StartCoroutine(_restartButtonCoroutine);
        }
    
        private IEnumerator RestartButtonRoutine()
        {
            ButtonFeedback();
            GameEvents.RaiseSaveGameForRestart();
            if(settings.buttonSfx)
                yield return new WaitForSeconds(settings.buttonSfx.length);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        #endregion
        
        #region Saves

        public void Save(SaveData saveData)
        {
            saveData.SfxIsOn = _sfxIsOn;
            saveData.HapticsIsOn = _hapticsIsOn;
        }

        public void Load(SaveData saveData)
        {
            _sfxIsOn = saveData.SfxIsOn;
            _hapticsIsOn = saveData.HapticsIsOn;
            sfxToggle.sprite = _sfxIsOn ? toggleEnabled : toggleDisabled;
            hapticsToggle.sprite = _hapticsIsOn ? toggleEnabled : toggleDisabled;
        }

        #endregion
    }
}