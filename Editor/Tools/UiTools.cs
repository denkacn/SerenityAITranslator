using UnityEditor;
using UnityEngine;

namespace SerenityAITranslator.Editor.Tools
{
    public static class UiTools
    {
        public static Texture2D MakeColorTexture(Color color, int width, int height)
        {
            var pixels = new Color[width * height];
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = color;
        
            var texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
        
        public static Color HexToColor(string hex)
        {
            hex = hex.Replace("0x", "").Replace("#", "");
        
            if (hex.Length == 6)
            {
                var r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                var g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                var b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            
                return new Color(r / 255f, g / 255f, b / 255f);
            }
            else if (hex.Length == 8)
            {
                var r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                var g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                var b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                var a = int.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            
                return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
            }
        
            return Color.white;
        }
        
        public static int GetIndexForValue(string value, string[] options)
        {
            for (var i = 0; i < options.Length; i++)
            {
                if (options[i] == value)
                    return i;
            }
            return 0;
        }
        
        public static float GetLabelWidth(string text)
        {
            var style = new GUIStyle(EditorStyles.label);
            var content = new GUIContent(text);
            var size = style.CalcSize(content);
            return size.x + 5f;
        }
        
        public static bool DisplayTranslateAllDialog()
        {
            return DisplayDialog("Do you really want to translate all the terms?");
        }
        
        public static bool DisplayRemoveProviderDialog()
        {
            return DisplayDialog("Do you really want to delete provider?");
        }
        
        public static bool DisplayRemovePromtDialog()
        {
            return DisplayDialog("Do you really want to delete promt?");
        }
        
        public static bool DisplayApplyChangesDialog()
        {
            return DisplayDialog("Do you really want to apply changes in all terms?\nThis will overwrite non-empty terms for the selected language in LanguageSourceAsset");
        }
        
        public static bool DisplayCreateProviderErrorMessage()
        {
            return DisplayMessage("The type of provider is not selected!");
        }

        public static bool DisplayMessage(string message)
        {
            return EditorUtility.DisplayDialog("Warring", message, "Ok");
        }

        public static bool DisplayDialog(string question)
        {
            return EditorUtility.DisplayDialog(
                "Are you sure?",
                question,
                "Yes",
                "No"
            );
        }

        
    }
}