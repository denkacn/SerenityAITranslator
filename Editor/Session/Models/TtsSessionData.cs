using System;
using SerenityAITranslator.Editor.Services.Tts.AiProviders;
using SerenityAITranslator.Editor.Services.Tts.Collections;

namespace SerenityAITranslator.Editor.Session.Models
{
    [Serializable]
    public class TtsSessionData
    {
        public string ProviderId;
        public string SelectedPromt;
        
        public TtsProvidersConfigurationItem TtsSettings;
        public IAITtsProvider TranslateProvider;
    }
}