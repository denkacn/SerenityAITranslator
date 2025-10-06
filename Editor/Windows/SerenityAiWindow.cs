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
using SerenityAITranslator.Editor.Services.Tts.Managers;
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
            
            GUILayout.Label("Serenity AI v0.88.5", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            //GUILayout.Label("Select Service:");
            GUILayout.BeginHorizontal(UiStyles.OddRowStyle);
            
            var isSelected = _selectedView is SettingsMainView;
            if (GUILayout.Button(new GUIContent(" Settings", AssetsUtility.LoadIcon("icon-settings.png")),
                    isSelected ? UiStyles.ButtonStyleGreenText : GUI.skin.button, 
                    GUILayout.Width(100),
                    GUILayout.Height(25)))
            {
                SelectService(SerenityServiceType.Settings);
            }

            isSelected = _selectedView is TranslateMainView;
            if (GUILayout.Button(new GUIContent(" Translate", AssetsUtility.LoadIcon("icon-translate.png")),
                    isSelected ? UiStyles.ButtonStyleGreenText : GUI.skin.button, 
                    GUILayout.Width(100),
                    GUILayout.Height(25)))
            {
                SelectService(SerenityServiceType.Translation);
            }

            isSelected = _selectedView is TtsMainView;
            if (GUILayout.Button(new GUIContent(" Voice", AssetsUtility.LoadIcon("icon-speaker.png")),
                    isSelected ? UiStyles.ButtonStyleGreenText : GUI.skin.button, 
                    GUILayout.Width(100),
                    GUILayout.Height(25)))
            {
                SelectService(SerenityServiceType.Voice);
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
            var ttsPromtSettings = AssetsUtility.LoadOrCreate<PromtSettingsCollection>("Assets/Editor/SerenityAi/TtsPromtSettings.asset");
            var translateProvidersSettings = AssetsUtility.LoadOrCreate<TranslateProvidersSettingCollection>("Assets/SerenityAITranslator/Settings/TranslateProvidersSetting.asset");
            
            _context.Init(sessionData, promtSettings, translateProvidersConfigurations, translateProvidersSettings, ttsProvidersConfiguration, ttsPromtSettings);
            
            var translateManager = new TranslateManager(_context);
            _context.SetupTranslateManager(translateManager);

            var ttsManager = new TtsManager(_context);
            _context.SetupTtsManager(ttsManager);

            if (!string.IsNullOrEmpty(_context.SessionData.TtsSessionData.VoicesLibraryPath))
            {
                var voicesCollection =
                    AssetsUtility.LoadOrCreate<VoicesCollection>(_context.SessionData.TtsSessionData.VoicesLibraryPath);
                
                _context.SetupVoicesCollection(voicesCollection);
            }
            
            var languageConverterData = AssetsUtility.LoadOrCreate<LanguageConverterData>("Assets/Editor/SerenityAi/LanguageConverterData.asset");
            _context.SetupLanguageConverterData(languageConverterData);
        }
    }
}
