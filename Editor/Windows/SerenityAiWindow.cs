using System;
using SerenityAITranslator.Editor.Context;
using SerenityAITranslator.Editor.Services.Common.Collections;
using SerenityAITranslator.Editor.Services.Common.Enums;
using SerenityAITranslator.Editor.Services.Common.Views;
using SerenityAITranslator.Editor.Services.Translation.Collections;
using SerenityAITranslator.Editor.Services.Translation.Managers;
using SerenityAITranslator.Editor.Services.Translation.Views;
using SerenityAITranslator.Editor.Services.Voice.Views;
using SerenityAITranslator.Editor.Session.Models;
using SerenityAITranslator.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace SerenityAITranslator.Editor.Windows
{
    [Serializable]
    public class SerenityAiWindow : EditorWindow
    {
        private MainView _selectedView;
        private SerenityContext _context;

        [MenuItem("Tools/Serenity AI")]
        public static void OpenSerenityAiWindow()
        {
            var window = GetWindow<SerenityAiWindow>("Serenity AI Window");
            window.Init();
            window.Show();
        }

        public void Init()
        {
            PrepareData();
            SelectService(_context.SessionData.ServiceType);
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Serenity AI v0.15.1", EditorStyles.boldLabel);
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
            if (_context == null) PrepareData();
            if (_selectedView != null && _context.SessionData.ServiceType == selectServiceType) return;
            
            _context.SessionData.ServiceType = selectServiceType;
            
            switch (selectServiceType)
            {
                case SerenityServiceType.Translation:
                    _selectedView = new TranslateMainView(this, _context);
                    _selectedView.Init();
                    
                    break;
                case SerenityServiceType.Voice:
                    _selectedView = new VoiceMainView(this, _context);
                    _selectedView.Init();
                    break;
            }
        }

        private void PrepareData()
        {
            _context = new SerenityContext();
            
            var sessionData = ScriptableObjectUtility.LoadOrCreate<SessionData>("Assets/Editor/SerenityAi/Session.asset");
            var promtSettings = ScriptableObjectUtility.LoadOrCreate<PromtSettingsCollection>("Assets/Editor/SerenityAi/PromtSettings.asset");
            var translateProvidersConfigurations = ScriptableObjectUtility.LoadOrCreate<TranslateProvidersConfigurationCollection>("Assets/Editor/SerenityAi/TranslateProvidersConfigurations.asset");
            
            var translateProvidersSettings = ScriptableObjectUtility.LoadFromResources<TranslateProvidersSettingCollection>("TranslateProvidersSetting");
            
            _context.Init(sessionData, promtSettings, translateProvidersConfigurations, translateProvidersSettings);
            
            var translateManager = new TranslateManager(_context);
            _context.SetupTranslateManager(translateManager);
        }
    }
}
