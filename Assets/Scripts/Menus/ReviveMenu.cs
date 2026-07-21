using System.Collections;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menus
{
    public class ReviveMenu : MonoBehaviour
    {
        [SerializeField] private Settings settings;
        [SerializeField] private GameObject menu;
        [SerializeField] private Image timer;
        [SerializeField] private Image timerBackground;
        [SerializeField] private TextMeshProUGUI timerText;
        private bool _acceptedRevive;
        private IEnumerator _timerCoroutine;

        private void OnEnable()
        {
            GameEvents.OnReviveSuggestion += EnableMenu;
        }

        private void OnDisable()
        {
            GameEvents.OnReviveSuggestion -= EnableMenu;
        }

        private void EnableMenu()
        {
            menu.SetActive(true);
            if(_timerCoroutine != null) StopCoroutine(_timerCoroutine);
            _timerCoroutine = StartTimer();
            StartCoroutine(_timerCoroutine);
        }

        public void AcceptReviveSuggestion()
        {
            Debug.Log("Accepted revive suggestion");
            _acceptedRevive = true;
            if(_timerCoroutine != null) StopCoroutine(_timerCoroutine);
            menu.SetActive(false);
            GameEvents.RaiseShowRewardedAd("Revive", GameEvents.RaiseSpawnNewBlocksForRevival);
        }

        public void DenyReviveSuggestion()
        {
            Debug.Log("Denied revive suggestion");
            if(_timerCoroutine != null) StopCoroutine(_timerCoroutine);
            menu.SetActive(false);
            GameEvents.RaiseGameOver();
            GameEvents.RaiseSaveGame();
        }

        private IEnumerator StartTimer()
        {
            _acceptedRevive = false;
            int timerTextValue = settings.reviveSuggestionDuration;
            float timeLeft = settings.reviveSuggestionDuration;
            timer.fillAmount = 1f;
            timerText.text = timerTextValue.ToString();
            while (timeLeft > 0)
            {
                timer.fillAmount = timeLeft / settings.reviveSuggestionDuration;
                timeLeft -= Time.deltaTime;
                yield return null;
                if (timeLeft > 0 && timeLeft < timerTextValue - 1)
                    timerText.text = (--timerTextValue).ToString();
            }
            if(!_acceptedRevive) DenyReviveSuggestion();
            _timerCoroutine = null;
        }
    }
}