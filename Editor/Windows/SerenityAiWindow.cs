using System;
using SerenityAITranslator.Editor.Services.Common.Views;
using SerenityAITranslator.Editor.Services.Translation.Views;
using SerenityAITranslator.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace SerenityAITranslator.Editor.Windows
{
    [Serializable]
    public class SerenityAiWindow : EditorWindow
    {
        private MainView _selectedView;
        
        [MenuItem("Tools/Serenity AI Window")]
        public static void ShowWindow()
        {
            GetWindow<SerenityAiWindow>("Serenity AI Window");
        }
        
        private void OnEnable()
        {
            _selectedView = new TranslateMainView(this);
            _selectedView.Init();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Serenity AI v0.12.2", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            GUILayout.Label("Select Service:");
            GUILayout.BeginHorizontal(UiStyles.OddRowStyle);

            if (GUILayout.Button("Translate", GUILayout.Width(100), GUILayout.Height(30)))
            {
                if (_selectedView.GetType() != typeof(TranslateMainView))
                {
                    _selectedView = new TranslateMainView(this);
                    _selectedView.Init();
                }
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            _selectedView?.Draw();
        }
    }
}
