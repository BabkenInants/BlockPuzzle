using System.Collections;
using UnityEngine;
using Themes;
using System.Collections.Generic;
using Saves;
using UnityEngine.UI;

namespace Managers
{
    public class ThemeManager : MonoBehaviour, ISavable
    {
        [SerializeField] private Theme[] themes;
        private Theme _currentTheme;
        private int _currentThemeIndex = -1;
        private List<IThemeReceiver> _themeReceivers;

        private void Start()
        {
            GetAllReceivers();
            SetNextTheme();
        }

        private void GetAllReceivers()
        {
            foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (mb is IThemeReceiver receiver) 
                    _themeReceivers.Add(receiver);
        }

        public void SetNextTheme()
        {
            _currentThemeIndex += _currentThemeIndex + 1 == themes.Length - 1 ? -_currentThemeIndex : 1;
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
            if(saveData.GameIsOver) return;
            _currentThemeIndex = saveData.CurrentThemeIndex;
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