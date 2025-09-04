using System;
using System.Collections.Generic;
using SerenityAITranslator.Editor.Services.Common.Models;
using SerenityAITranslator.Editor.Services.Settings.Models;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Voice.Collections
{
    public class TtsProvidersConfigurationCollection : ScriptableObject
    {
        public List<TtsProvidersConfigurationItem> Providers;
    }
    
    [Serializable]
    public class TtsProvidersConfigurationItem : BaseProvidersConfigurationItem
    {
        public TtsProviderType ProviderType;

        public bool IsTokenExist => !string.IsNullOrEmpty(Token) || !string.IsNullOrEmpty(TokenFilePath);
    }
}