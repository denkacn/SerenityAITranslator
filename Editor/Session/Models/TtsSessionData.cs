using SerenityAITranslator.Editor.Services.Tts.AiProviders;
using SerenityAITranslator.Editor.Services.Tts.Collections;

namespace SerenityAITranslator.Editor.Session.Models
{
    public class TtsSessionData
    {
        public string ProviderId;
        public string SelectedPromt;
        
        public TtsProvidersConfigurationItem TranslateSettings;
        public IAITtsProvider TranslateProvider;
    }
}