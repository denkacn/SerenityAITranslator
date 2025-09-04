using System;
using System.Collections.Generic;
using SerenityAITranslator.Editor.Services.Settings.Models;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Translation.Collections
{
    public class TranslateProvidersConfigurationCollection : ScriptableObject
    {
        public List<TranslateProviderConfigurationItem> Providers;
    }
    
    [Serializable]
    public class TranslateProviderConfigurationItem
    {
        public string Id;
        public string Host;
        public string Endpoint;
        public string Token;
        public string TokenFilePath;
        public bool IsTokenFromFile;
        public string Model;
        public TextProviderType ProviderType;

        public bool IsTokenExist => !string.IsNullOrEmpty(Token) || !string.IsNullOrEmpty(TokenFilePath);
    }
}