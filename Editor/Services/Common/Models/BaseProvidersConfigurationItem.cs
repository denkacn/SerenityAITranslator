using System;

namespace SerenityAITranslator.Editor.Services.Common.Models
{
    [Serializable]
    public class BaseProvidersConfigurationItem
    {
        public string Id;
        public string Host;
        public string Endpoint;
        public string Token;
        public string TokenFilePath;
        public bool IsTokenFromFile;
        public string Model;
    }
}