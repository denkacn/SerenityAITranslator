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
        public string[] TranslatedText;
        public bool IsShowTranslated;
        public bool IsSelected;
        
        public TranslatedRowData(int id, string term, string sourceText, string originalText, int languageAmount)
        {
            Id = id;
            Term = term;
            SourceText = sourceText;
            OriginalText = originalText;
            
            TranslatedText = new string[languageAmount];
            for (var i = 0; i < languageAmount; i++)
            {
                TranslatedText[i] = string.Empty;
            }
        }
    }
}