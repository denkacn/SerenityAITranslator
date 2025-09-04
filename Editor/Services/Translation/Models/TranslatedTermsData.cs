using System;

namespace SerenityAITranslator.Editor.Services.Translation.Models
{
    [Serializable]
    public class TranslatedTermsData
    {
        public string Term;
        public string[] Languages;
        public bool IsUpdated;
    }
}