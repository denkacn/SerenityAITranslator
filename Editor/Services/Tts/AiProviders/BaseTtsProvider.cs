using System;
using System.IO;
using System.Threading.Tasks;
using SerenityAITranslator.Editor.Services.Tts.Collections;
using SerenityAITranslator.Editor.Services.Tts.Models;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Tts.AiProviders
{
    public abstract class BaseTtsProvider : IAITtsProvider
    {
        public abstract Task<TtsResultData> GetTranslate(TtsPromtData promtData, TtsProvidersConfigurationItem settings,
            string promt);
        
        protected async Task<string> GetToken(TtsProvidersConfigurationItem settings)
        {
            if (!settings.IsTokenFromFile || string.IsNullOrEmpty(settings.TokenFilePath)) return settings.Token;
    
            try
            {
                var result = await File.ReadAllTextAsync(settings.TokenFilePath);
                return result?.Trim() ?? settings.Token;
            }
            catch (Exception ex)
            {
                Debug.LogError($"File read error: {ex.Message}");
                return settings.Token;
            }
        }
    }
}