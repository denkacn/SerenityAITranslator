namespace SerenityAITranslator.Editor.Services.Translation.Models
{
    public class TranslatedData
    {
        public string Term;
        public string Translation;
        public bool IsNoError;
        
        public TranslatedData(string term, string translation)
        {
            Term = term;
            Translation = translation;
            IsNoError = true;
        }

        public TranslatedData Failure()
        {
            IsNoError = false;
            return this;
        }
    }
}