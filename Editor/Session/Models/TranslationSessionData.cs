using System;
using System.Collections.Generic;
using SerenityAITranslator.Editor.Services.Common.PromtFactories;
using SerenityAITranslator.Editor.Services.Translation.AiProviders;
using SerenityAITranslator.Editor.Services.Translation.Collections;
using SerenityAITranslator.Editor.Services.Translation.Models;
using SerenityAITranslator.Editor.Services.Translation.SourceAssetProvider;

namespace SerenityAITranslator.Editor.Session.Models
{
    [Serializable]
    public class TranslationSessionData
    {
        public string SourceLanguage;
        public string DestinationLanguage;
        public string ProviderId;
        public string SelectedPromt;
        public bool IsShowInfoView = true;
        public bool IsShowSettingView = true;
        
        public List<TranslatedRowData> TranslationsData = new List<TranslatedRowData>();
        public IAiTranslateProvider TranslateProvider;
        public TranslateProviderConfigurationItem TranslateSettings;
        public string BaseLanguage;
        public PromtFactoryBase PromtFactory;
        public List<string> AvailableLanguages;
        
        public ISourceAssetProvider SourceAssetProvider;
    }
}