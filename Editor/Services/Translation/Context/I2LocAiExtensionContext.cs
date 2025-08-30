using System;
using SerenityAITranslator.Editor.Services.Common.PromtFactories;
using SerenityAITranslator.Editor.Services.Translation.AiProviders;
using SerenityAITranslator.Editor.Services.Translation.AiProviders.Settings;

namespace SerenityAITranslator.Editor.Services.Translation.Context
{
    [Serializable]
    public class I2LocAiExtensionContext
    {
        public IAiTranslateProvider TranslateProvider { get; set; }
        public BaseTranslateProviderSettings TranslateSettings { get; set; }
        public string BaseLanguage { get; set; }
        public PromtFactoryBase PromtFactory { get; set; }
    }
}