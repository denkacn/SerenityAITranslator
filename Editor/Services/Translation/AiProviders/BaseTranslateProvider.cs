using System;
using System.IO;
using System.Threading.Tasks;
using SerenityAITranslator.Editor.Services.Common.PromtFactories;
using SerenityAITranslator.Editor.Services.Translation.AiProviders.Settings;
using SerenityAITranslator.Editor.Services.Translation.Models;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Translation.AiProviders
{
    public abstract class BaseTranslateProvider : IAiTranslateProvider
    {
        public abstract Task<TranslatedData> GetTranslate(TranslatedPromtData promtData,
            BaseTranslateProviderSettings settings, PromtFactoryBase promtFactory);

        protected async Task<string> GetToken(BaseTranslateProviderSettings settings)
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