#if !CrazyGamesPlatform_yg && UNITY_EDITOR
using System.Globalization;
using UnityEngine;
using static YG.InfoYG.TemplatesSettings;

namespace YG.EditorScr.BuildModify
{
    public partial class ModifyBuild
    {
        public static void TextGameLoad()
        {
            if (!YG2.infoYG.Templates.enableLoadText ||
                    infoYG.Templates.translates.Length == 0)
                return;

            string searchAnchor = "<div class=\"spinner\"></div>";
            string searchCodeInit = "// Additional init modules";

            // Div
            if (!indexFile.Contains(searchAnchor) || !indexFile.Contains(searchCodeInit))
            {
                Debug.LogWarning("Search string not found in index.html");
                return;
            }

            int index = indexFile.IndexOf(searchAnchor);
            if (index != -1)
            {
                index += searchAnchor.Length;
                indexFile = indexFile.Insert(index, "<div id=\"loading-text\"></div>");
            }

            // Init
            string addInitStr =
#if YandexGamesPlatform_yg
                "const loadingTextLang = ysdk.environment.i18n.lang;\n";
#else
                "const loadingTextLang = (() => {\n" +
                "    const browserLang = (navigator.languages && navigator.languages.length ? navigator.languages[0] : navigator.language || navigator.userLanguage || \"en\");\n" +
                "    return browserLang.toLowerCase().split(/[-_]/)[0];\n" +
                "})();\n";
#endif

            addInitStr += "switch (loadingTextLang) {\n";

            for (int i = 0; i < infoYG.Templates.translates.Length; i++)
            {
                addInitStr += $"case \"{EscapeJavaScriptString(infoYG.Templates.translates[i].language)}\":\n";
                addInitStr += GenStr(infoYG.Templates.translates[i].text);
            }

            addInitStr += $"default:\n";
            addInitStr += GenStr(infoYG.Templates.translates[0].text);
            addInitStr += "}";

            string GenStr(string text)
            {
                return $"document.getElementById(\"loading-text\").textContent = \"{EscapeJavaScriptString(text)}\";\nbreak;\n";
            }

            string EscapeJavaScriptString(string value)
            {
                if (string.IsNullOrEmpty(value))
                    return string.Empty;

                return value
                    .Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\r", "\\r")
                    .Replace("\n", "\\n");
            }

            AddIndexCode(addInitStr, CodeType.Init2);

            // CSS
            string styleText = FileTextCopy("TextGameLoad_css.css");
            styleText = styleText.Replace("___TOP___", YG2.infoYG.Templates.textMarginTop.ToString(CultureInfo.InvariantCulture));
            styleText = styleText.Replace("___WEIGHT___", YG2.infoYG.Templates.textWeight.ToString(CultureInfo.InvariantCulture));
            styleText = styleText.Replace("___SIZE___", YG2.infoYG.Templates.textScale.ToString(CultureInfo.InvariantCulture));
            styleText = styleText.Replace("___COLOR___", ConvertToRGBA(YG2.infoYG.Templates.textColor));

            styleText = styleText.Replace("___FONT___", GetFontStack());
            string GetFontStack()
            {
                switch (YG2.infoYG.Templates.fontPreset)
                {
                    case FontPreset.Roboto:
                        return "'Roboto', -apple-system, 'Segoe UI', Arial, sans-serif";
                    case FontPreset.Arial:
                        return "Arial, 'Helvetica Neue', Helvetica, sans-serif";
                    case FontPreset.Custom:
                        return YG2.infoYG.Templates.customFontStack;
                    case FontPreset.System:
                    default:
                        return "-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Fira Sans', 'Droid Sans', 'Helvetica Neue', Arial, sans-serif";
                }
            }

            styleFile += styleText;
        }
    }
}
#endif
