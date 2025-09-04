using System;
using System.Collections.Generic;
using SerenityAITranslator.Editor.Services.Translation.AiProviders;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Translation.Collections
{
    [CreateAssetMenu(menuName = "SerenityAI/TranslateProvidersSetting")]
    public class TranslateProvidersSettingCollection : ScriptableObject
    {
        public List<TranslateProvidersSettingItem> Settings;
    }
    
    [Serializable]
    public class TranslateProvidersSettingItem
    {
        public string Host;
        public string Endpoint;
        public TextProviderType ProviderType;
    }
}