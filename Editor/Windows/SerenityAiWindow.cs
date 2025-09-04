using System;
using SerenityAITranslator.Editor.Context;
using SerenityAITranslator.Editor.Services.Common.Collections;
using SerenityAITranslator.Editor.Services.Common.Enums;
using SerenityAITranslator.Editor.Services.Common.Views;
using SerenityAITranslator.Editor.Services.Settings.Views;
using SerenityAITranslator.Editor.Services.Translation.Collections;
using SerenityAITranslator.Editor.Services.Translation.Managers;
using SerenityAITranslator.Editor.Services.Translation.Views;
using SerenityAITranslator.Editor.Services.Voice.Collections;
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
            window.Show();
        }

        private void OnEnable()
        {
            Init();
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
            
            //GUILayout.Label("Select Service:");
            GUILayout.BeginHorizontal(UiStyles.OddRowStyle);

            if (GUILayout.Button("Settings", GUILayout.Width(100), GUILayout.Height(20)))
            {
                SelectService(SerenityServiceType.Settings);
            }
            
            if (GUILayout.Button("Translate", GUILayout.Width(100), GUILayout.Height(20)))
            {
                SelectService(SerenityServiceType.Translation);
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
                case SerenityServiceType.Settings:
                    _selectedView = new SettingsMainView(this, _context);
                    _selectedView.Init();
                    break;
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
            var ttsProvidersConfiguration = ScriptableObjectUtility.LoadOrCreate<TtsProvidersConfigurationCollection>("Assets/Editor/SerenityAi/TtsProvidersConfiguration.asset");

            var translateProvidersSettings = ScriptableObjectUtility.LoadFromResources<TranslateProvidersSettingCollection>("TranslateProvidersSetting");
            
            _context.Init(sessionData, promtSettings, translateProvidersConfigurations, translateProvidersSettings, ttsProvidersConfiguration);
            
            var translateManager = new TranslateManager(_context);
            _context.SetupTranslateManager(translateManager);
        }
    }
}
