#if !CrazyGamesPlatform_yg && UNITY_EDITOR
using System;
using UnityEngine;

namespace YG
{
    public partial class InfoYG
    {
        public partial class TemplatesSettings
        {
#if RU_YG2
            [HeaderYG("Текст при загрузке игры")]
#else
            [HeaderYG("Text when loading game")]
#endif

            public bool enableLoadText = true;

            [NestedYG(nameof(enableLoadText)), Min(0)]
#if RU_YG2
            [Tooltip("Отступ сверху")]
#else
            [Tooltip("Top margin")]
#endif
            public int textMarginTop = 15;

            [NestedYG(nameof(enableLoadText)), Min(0)]
#if RU_YG2
            [Tooltip("Размер шрифта")]
#else
            [Tooltip("Font Size")]
#endif
            public float textScale = 20;

            [NestedYG(nameof(enableLoadText))]
#if RU_YG2
            [Tooltip("Цвет шрифта")]
#else
            [Tooltip("Font Color")]
#endif
            public Color textColor = Color.white;

            [NestedYG(nameof(enableLoadText)), Range(100, 900)]
#if RU_YG2
            [Tooltip("Толщина шрифта")]
#else
            [Tooltip("Font thickness")]
#endif
            public int textWeight = 500;

            public enum FontPreset { Custom, System, Roboto, Arial }

            [NestedYG(nameof(enableLoadText))]
#if RU_YG2
            [Tooltip("Пресет семейства шрифтов")]
#else
            [Tooltip("Font Family Preset")]
#endif
            public FontPreset fontPreset = FontPreset.System;

            [NestedYG(nameof(enableLoadText), "!fontPreset")]
#if RU_YG2
            [Tooltip("Кастомный CSS-стек для font-family, если выбран Custom")]
#else
            [Tooltip("Custom CSS stack for font-family, if Custom is selected")]
#endif
            public string customFontStack = "'Inter', 'Segoe UI', Arial, sans-serif";
#if RU_YG2
            [Tooltip("Текст на разных языках. Каждый элемент массива - это вариант текста с переводом на язык пользователя. Первый элемент массива будет использоваться как стандартный, если в списке не будет варианта для языка пользователя.")]
#else
            [Tooltip("The text is in different languages. Each element of the array is a text variant with a translation into the user's language. The first element of the array will be used as a standard one if there is no option for the user's language in the list.")]
#endif
            public Translate[] translates =
            {
                new Translate
                {
                    language = "en",
                    text = "Please wait for the download."
                },
                new Translate
                {
                    language = "ru",
                    text = "Пожалуйста, дождитесь загрузки..."
                }
            };

            [Serializable]
            public struct Translate
            {
#if RU_YG2
                [Tooltip("Язык. Например: ru, en, tr")]
#else
                [Tooltip("Language. For example: ru, en, tr")]
#endif
                public string language;
#if RU_YG2
                [Tooltip("Текст, который будет показываться при загрузке")]
#else
                [Tooltip("The text that will be displayed when loading")]
#endif
                public string text;
            }
        }
    }
}
#endif