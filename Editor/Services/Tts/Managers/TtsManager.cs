using System;
using SerenityAITranslator.Editor.Context;
using SerenityAITranslator.Editor.Services.Common.Collections;
using SerenityAITranslator.Editor.Services.Settings.Models;
using SerenityAITranslator.Editor.Services.Tts.AiProviders;
using SerenityAITranslator.Editor.Services.Tts.Collections;

namespace SerenityAITranslator.Editor.Services.Tts.Managers
{
    public class TtsManager
    {
        private readonly SerenityContext _context;
        
        public string SelectedTtsProviderId => _context.SessionData.TtsSessionData.TtsSettings != null ? _context.SessionData.TtsSessionData.TtsSettings.Id : string.Empty;
        
        public TtsManager(SerenityContext context)
        {
            _context = context;
        }

        public void SelectPromt(PromtSettingsItem promtData)
        {
            var translationSessionData = _context.SessionData.TtsSessionData;
            translationSessionData.SelectedPromt = promtData.Promt;

            SaveSession();
        }
        
        private void SaveSession()
        {
            _context.Save();
        }

        public void SelectTranslateProviderSettings(TtsProvidersConfigurationItem provider)
        {
            var translationSessionData = _context.SessionData.TtsSessionData;
            translationSessionData.ProviderId = provider.Id;
            translationSessionData.TtsSettings = provider;
            
            switch (provider.ProviderType)
            {
                case TtsProviderType.None:
                    break;
                case TtsProviderType.Coqui:
                    translationSessionData.TranslateProvider = new CoquiTtsProvider();
                    break;
                case TtsProviderType.ElevenLabs:
                    translationSessionData.TranslateProvider = new ElevenLabsTtsProvider();
                    break;
                case TtsProviderType.Gemini:
                    translationSessionData.TranslateProvider = new GeminiTtsProvider();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            SaveSession();
        }
    }
}