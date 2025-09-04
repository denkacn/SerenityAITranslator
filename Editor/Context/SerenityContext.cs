using SerenityAITranslator.Editor.Services.Common.Collections;
using SerenityAITranslator.Editor.Services.Translation.Collections;
using SerenityAITranslator.Editor.Services.Translation.Managers;
using SerenityAITranslator.Editor.Session.Models;
using UnityEditor;

namespace SerenityAITranslator.Editor.Context
{
    public class SerenityContext
    {
        public SessionData SessionData { get; private set; }
        public PromtSettingsCollection PromtSettings { get; private set; }
        public TranslateProvidersConfigurationCollection TranslateProvidersConfigurations { get; private set; }
        public TranslateManager TranslateManager  { get; private set; }
        
        public void Init(SessionData sessionData, PromtSettingsCollection promtSettings, TranslateProvidersConfigurationCollection translateProvidersConfigurations)
        {
            SessionData = sessionData;
            PromtSettings = promtSettings;
            TranslateProvidersConfigurations = translateProvidersConfigurations;
        }
        
        public void SetupTranslateManager(TranslateManager translateManager)
        {
            TranslateManager = translateManager;
        }

        public void Save()
        {
            EditorUtility.SetDirty(SessionData);
            EditorUtility.SetDirty(PromtSettings);
            EditorUtility.SetDirty(TranslateProvidersConfigurations);
            AssetDatabase.SaveAssets();
        }
    }
}