using SerenityAITranslator.Editor.Services.Settings.Models;

namespace SerenityAITranslator.Editor.Services.Translation.AiProviders
{
    public static class TranslateProviderFactory
    {
        public static IAiTranslateProvider Create(TextProviderType providerType)
        {
            switch (providerType)
            {
                case TextProviderType.LmStudio:
                    return new LmStudioTranslateProvider();
                case TextProviderType.Ollama:
                    return new OllamaTranslateProvider();
                case TextProviderType.OpenAi:
                    return new OpenAiTranslateProvider();
                case TextProviderType.DeepSeek:
                    return new DeepSeekTranslateProvider();
                case TextProviderType.Grok:
                    return new GrokTranslateProvider();
                case TextProviderType.GoogleAi:
                    return new GoogleAiTranslateProvider();
                case TextProviderType.GoogleTranslate:
                    return new GoogleTranslateProvider();
                case TextProviderType.None:
                default:
                    return null;
            }
        }
    }
}
