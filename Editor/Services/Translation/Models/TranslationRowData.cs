namespace SerenityAITranslator.Editor.Services.Translation.Models
{
    public class TranslationRowData
    {
        public int Id { get; set; }
        public string Term { get; set; }
        public string SourceText { get; set; }
        public string OriginalText { get; set; }
        public string TranslatedText { get; set; }
        public bool IsShowTranslated { get; set; }
        
        public TranslationRowData(int id, string term, string sourceText, string originalText)
        {
            Id = id;
            Term = term;
            SourceText = sourceText;
            OriginalText = originalText;
        }
    }
}