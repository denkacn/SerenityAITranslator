using UnityEditor;
using UnityEngine;

namespace SerenityAITranslator.Editor.Tools
{
    public static class UiStyles
    {
        public static readonly GUIStyle LabelHeaderStyle = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleLeft
        };
        
        public static readonly GUIStyle EvenRowStyle = new GUIStyle
        {
            normal =
            {
                background = UiTools.MakeColorTexture(UiTools.HexToColor("#383838"), 1, 1)
            }
        };

        public static readonly GUIStyle OddRowStyle = new GUIStyle
        {
            normal =
            {
                background = UiTools.MakeColorTexture(UiTools.HexToColor("#333333"), 1, 1)
            }
        };
        
        public static readonly GUIStyle LabelRowStyle = new GUIStyle(EditorStyles.label)
        {
            wordWrap = true,
            alignment = TextAnchor.UpperLeft
        };
            
        public static readonly GUIStyle LabelRowStyleGreen = new GUIStyle(EditorStyles.label)
        {
            wordWrap = true,
            alignment = TextAnchor.UpperLeft,
            normal = { textColor = UiTools.HexToColor("#3CFF00") }
        };
        
        public static readonly GUIStyle LabelRowStyleYellow = new GUIStyle(EditorStyles.label)
        {
            wordWrap = true,
            alignment = TextAnchor.UpperLeft,
            normal = { textColor = UiTools.HexToColor("#fff600") }
        };

        public static readonly GUIStyle ButtonStyleGreen = new GUIStyle(GUI.skin.button)
        {
            normal =
            {
                background = UiTools.MakeColorTexture(UiTools.HexToColor("#396D29"), 1, 1),
                scaledBackgrounds = null,
            },
            hover =
            {
                background = UiTools.MakeColorTexture(UiTools.HexToColor("#396D29"), 1, 1),
                scaledBackgrounds = null,
            },
            active =
            {
                background = UiTools.MakeColorTexture(UiTools.HexToColor("#396D29"), 1, 1),
                scaledBackgrounds = null,
            },
            focused =
            {
                background = UiTools.MakeColorTexture(UiTools.HexToColor("#396D29"), 1, 1),
                scaledBackgrounds = null,
            },
        };
    }
}