using System;
using System.Collections;
using UnityEngine;
using Themes;
using System.Collections.Generic;
using Core;
using Saves;
using UnityEngine.UI;

namespace Managers
{
    public class ThemeManager : MonoBehaviour, ISavable
    {
        [SerializeField] private Theme[] themes;
        private Theme _currentTheme;
        private int _currentThemeIndex;
        private List<IThemeReceiver> _themeReceivers = new List<IThemeReceiver>();

        private void OnEnable() => GameEvents.SetNextTheme += SetNextTheme;
        
        private void OnDisable() => GameEvents.SetNextTheme -= SetNextTheme;

        private void GetAllReceivers()
        {
            foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (mb is IThemeReceiver receiver) 
                    _themeReceivers.Add(receiver);
        }

        private void SetNextTheme() => SetThemeWithIndex(_currentThemeIndex + 1 == themes.Length ? 0 : _currentThemeIndex + 1);

        private void SetThemeWithIndex(int index)
        {
            if (index < 0 || index >= themes.Length)
            {
                Debug.LogError($"Index {index} is out of range");
                return;
            }
            _currentThemeIndex = index;
            _currentTheme = themes[_currentThemeIndex];
            foreach (IThemeReceiver receiver in _themeReceivers)
                receiver.ReceiveTheme(_currentTheme);
        }

        public void Save(SaveData saveData)
        {
            if(saveData.GameIsOver) return;
            saveData.CurrentThemeIndex = _currentThemeIndex;
        }

        public void Load(SaveData saveData)
        {
            _currentThemeIndex = saveData.GameIsOver ? 0 : saveData.CurrentThemeIndex;
            _currentTheme = themes[_currentThemeIndex];
            GetAllReceivers();
            foreach (var receiver in _themeReceivers)
                receiver.ReceiveThemeOnGameStart(_currentTheme);
        }
    } 
}

public static class ThemeTools
{
    public static IEnumerator SetSpriteRendererColor(SpriteRenderer renderer, Color oldColor, Color newColor, float duration)
    {
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            renderer.color = Color.Lerp(oldColor, newColor, elapsedTime / duration);
            yield return null;
        }
        renderer.color = newColor;
    }

    public static IEnumerator SetImageColor(Image image, Color oldColor, Color newColor, float duration)
    {
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            image.color = Color.Lerp(oldColor, newColor, elapsedTime / duration);
            yield return null;
        }
        image.color = newColor;
    }
}