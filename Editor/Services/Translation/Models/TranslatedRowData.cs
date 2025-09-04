using System;

namespace SerenityAITranslator.Editor.Services.Translation.Models
{
    [Serializable]
    public class TranslatedRowData
    {
        public int Id;
        public string Term;
        public string SourceText;
        public string OriginalText;
        public string TranslatedText;
        public bool IsShowTranslated;
        
        public TranslatedRowData(int id, string term, string sourceText, string originalText)
        {
            Id = id;
            Term = term;
            SourceText = sourceText;
            OriginalText = originalText;
        }
    }
}