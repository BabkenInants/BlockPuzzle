using System;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace Themes
{
    public static class ThemeTools
    {
        public static IEnumerator SetSpriteRendererColor(SpriteRenderer renderer, Color oldColor, 
            Color newColor, float duration, int row = 0, int col = 0, Action<int, int> callback = null)
        {
            if (!renderer) yield break;
            float elapsedTime = 0;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                renderer.color = Color.Lerp(oldColor, newColor, elapsedTime / duration);
                yield return null;
            }
            renderer.color = newColor;
            callback?.Invoke(row, col);
        }

        public static IEnumerator SetImageColor(Image image, Color oldColor, Color newColor, float duration)
        {
            if(!image) yield break;
            float elapsedTime = 0;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                image.color = Color.Lerp(oldColor, newColor, elapsedTime / duration);
                yield return null;
            }
            image.color = newColor;
        }

        public static IEnumerator SetTextColor(TextMeshProUGUI text, Color oldColor, Color newColor, float duration)
        {
            if(!text) yield break;
            float elapsedTime = 0;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                text.color = Color.Lerp(oldColor, newColor, elapsedTime / duration);
                yield return null;
            }
            text.color = newColor;
        }
    }
}