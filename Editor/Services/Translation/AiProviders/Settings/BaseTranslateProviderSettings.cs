using System;

namespace SerenityAITranslator.Editor.Services.Translation.AiProviders.Settings
{
    [Serializable]
    public class BaseTranslateProviderSettings
    {
        public string Id { get; set; }
        public string Host { get; set; }
        public string Endpoint { get; set; }
        public string  Token { get; set; }
        public string  TokenFilePath { get; set; }
        public bool  IsTokenFromFile { get; set; }
        public string  Model { get; set; }
        public TextProviderType ProviderType { get; set; }

        public bool IsTokenExist => !string.IsNullOrEmpty(Token) || !string.IsNullOrEmpty(TokenFilePath);
    }
}