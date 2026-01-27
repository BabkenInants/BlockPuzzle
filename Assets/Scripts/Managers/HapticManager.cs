using System.Collections;
using UnityEngine;
using Core;
using Saves;

namespace Managers
{
    public class HapticManager : MonoBehaviour, ISavable
    {
        private bool _hapticsIsOn;
    
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void _TriggerLightHaptic();
    
    [DllImport("__Internal")]
    private static extern void _TriggerMediumHaptic();
    
    [DllImport("__Internal")]
    private static extern void _TriggerHeavyHaptic();
#endif

        private void SetHapticsState(bool isOn) => _hapticsIsOn = isOn;

        public void OnEnable()
        {
            GameEvents.SetHapticsState += SetHapticsState;
            GameEvents.PlayHaptics += PlayHaptics;
            GameEvents.PlayHapticsInARow += PlayHapticsInARow;
        }

        public void OnDisable()
        {
            GameEvents.SetHapticsState -= SetHapticsState;
            GameEvents.PlayHaptics -= PlayHaptics;
            GameEvents.PlayHapticsInARow -= PlayHapticsInARow;
        }

        private void PlayHaptics(HapticType type)
        {
            switch (type)
            {
                case HapticType.Light:
                    Light();
                    break;
                case HapticType.Medium:
                    Medium();
                    break;
                case HapticType.Heavy:
                    Heavy();
                    break;
                default:
                    Debug.LogError("Undefined Haptic Type");
                    break;
            }
        }

        private void PlayHapticsInARow(HapticType type, int count) => 
            StartCoroutine(PlayHapticsInARowRoutine(type, count));

        public void Save(SaveData data){}

        public void Load(SaveData data) => SetHapticsState(data.HapticsIsOn);

        private void Light()
        {
            if(!_hapticsIsOn) return;
#if UNITY_IOS && !UNITY_EDITOR
        _TriggerLightHaptic();
#endif
        }
    
        private void Medium()
        {
            if(!_hapticsIsOn) return;
#if UNITY_IOS && !UNITY_EDITOR
        _TriggerMediumHaptic();
#endif
        }
    
        private void Heavy()
        {
            if(!_hapticsIsOn) return;
#if UNITY_IOS && !UNITY_EDITOR
        _TriggerHeavyHaptic();
#endif
        }

        public IEnumerator PlayHapticsInARowRoutine(HapticType type, int count)
        {
            if(!_hapticsIsOn) yield break;
#if UNITY_IOS && !UNITY_EDITOR
        for (var i = 0; i < count; i++)
        {
            switch (type)
            {
                case HapticType.Light:
                    Light();
                    yield return new WaitForSeconds(0.1f); // Задержка между haptics
                    break;
                case HapticType.Medium:
                    Medium();
                    yield return new WaitForSeconds(0.15f);
                    break;
                case HapticType.Heavy:
                    Heavy();
                    yield return new WaitForSeconds(0.2f);
                    break;
            }
        }
#endif
            yield break;
        }

        public enum HapticType
        {
            Light,
            Medium,
            Heavy
        }
    }
}