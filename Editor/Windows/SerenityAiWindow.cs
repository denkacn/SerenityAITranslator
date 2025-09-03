using System;
using SerenityAITranslator.Editor.Services.Common.Enums;
using SerenityAITranslator.Editor.Services.Common.Views;
using SerenityAITranslator.Editor.Services.Managers;
using SerenityAITranslator.Editor.Services.Translation.Views;
using SerenityAITranslator.Editor.Services.Voice.Views;
using SerenityAITranslator.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace SerenityAITranslator.Editor.Windows
{
    [Serializable]
    public class SerenityAiWindow : EditorWindow
    {
        private MainView _selectedView;
        private ISerenityAIManager _manager;

        public void Init(ISerenityAIManager manager)
        {
            _manager = manager;

            SelectService(_manager.Session.ServiceType);
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Serenity AI v0.12.2", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            GUILayout.Label("Select Service:");
            GUILayout.BeginHorizontal(UiStyles.OddRowStyle);

            if (GUILayout.Button("Translate", GUILayout.Width(100), GUILayout.Height(30)))
            {
                SelectService(SerenityServiceType.Translation);
            }
            
            
            if (GUILayout.Button("Voice", GUILayout.Width(100), GUILayout.Height(30)))
            {
                SelectService(SerenityServiceType.Voice);
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            _selectedView?.Draw();
        }

        private void SelectService(SerenityServiceType selectServiceType)
        {
            if (_selectedView != null && _manager.Session.ServiceType == selectServiceType) return;
            
            _manager.Session.ServiceType = selectServiceType;
            _manager.SaveSession();
            
            
            switch (selectServiceType)
            {
                case SerenityServiceType.Translation:
                    _selectedView = new TranslateMainView(this, _manager);
                    _selectedView.Init();
                    
                    Debug.Log("_manager " + _manager);
                    
                    break;
                case SerenityServiceType.Voice:
                    _selectedView = new VoiceMainView(this, _manager);
                    _selectedView.Init();
                    break;
            }
        }
    }
}
