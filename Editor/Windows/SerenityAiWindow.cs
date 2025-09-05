using System;
using SerenityAITranslator.Editor.Context;
using SerenityAITranslator.Editor.Services.Common.Collections;
using SerenityAITranslator.Editor.Services.Common.Enums;
using SerenityAITranslator.Editor.Services.Common.Views;
using SerenityAITranslator.Editor.Services.Settings.Views;
using SerenityAITranslator.Editor.Services.Translation.Collections;
using SerenityAITranslator.Editor.Services.Translation.Managers;
using SerenityAITranslator.Editor.Services.Translation.Views;
using SerenityAITranslator.Editor.Services.Tts.Collections;
using SerenityAITranslator.Editor.Services.Tts.Views;
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
            EditorGUIUtility.SetIconSize(new Vector2(16, 16));
            
            GUILayout.Label("Serenity AI v0.15.1", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            //GUILayout.Label("Select Service:");
            GUILayout.BeginHorizontal(UiStyles.OddRowStyle);
            
            if (GUILayout.Button(new GUIContent(" Settings", AssetsUtility.LoadIcon("icon-settings.png")), GUILayout.Width(100), GUILayout.Height(25)))
            {
                SelectService(SerenityServiceType.Settings);
            }
            
            if (GUILayout.Button(new GUIContent(" Translate", AssetsUtility.LoadIcon("icon-translate.png")), GUILayout.Width(100), GUILayout.Height(25)))
            {
                SelectService(SerenityServiceType.Translation);
            }
            
            if (GUILayout.Button(new GUIContent(" Voice", AssetsUtility.LoadIcon("icon-speaker.png")), GUILayout.Width(100), GUILayout.Height(25)))
            {
                SelectService(SerenityServiceType.Translation);
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            _selectedView?.Draw();
            
            EditorGUIUtility.SetIconSize(Vector2.zero);
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
                    _selectedView = new TtsMainView(this, _context);
                    _selectedView.Init();
                    break;
            }
        }

        private void PrepareData()
        {
            _context = new SerenityContext();
            
            var sessionData = AssetsUtility.LoadOrCreate<SessionData>("Assets/Editor/SerenityAi/Session.asset");
            var promtSettings = AssetsUtility.LoadOrCreate<PromtSettingsCollection>("Assets/Editor/SerenityAi/PromtSettings.asset");
            var translateProvidersConfigurations = AssetsUtility.LoadOrCreate<TranslateProvidersConfigurationCollection>("Assets/Editor/SerenityAi/TranslateProvidersConfigurations.asset");
            var ttsProvidersConfiguration = AssetsUtility.LoadOrCreate<TtsProvidersConfigurationCollection>("Assets/Editor/SerenityAi/TtsProvidersConfiguration.asset");
            var translateProvidersSettings = AssetsUtility.LoadOrCreate<TranslateProvidersSettingCollection>("Assets/SerenityAITranslator/Settings/TranslateProvidersSetting.asset");

            //var translateProvidersSettings = AssetsUtility.LoadFromResources<TranslateProvidersSettingCollection>("TranslateProvidersSetting");
            
            _context.Init(sessionData, promtSettings, translateProvidersConfigurations, translateProvidersSettings, ttsProvidersConfiguration);
            
            var translateManager = new TranslateManager(_context);
            _context.SetupTranslateManager(translateManager);
        }
    }
}
