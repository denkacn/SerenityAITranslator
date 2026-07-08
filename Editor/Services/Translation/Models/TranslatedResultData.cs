using SerenityAITranslator.Editor.Services.Common.Models;

namespace SerenityAITranslator.Editor.Services.Translation.Models
{
    public class TranslatedResultData : BaseResultData
    {
        public string Term;
        public string Translation;
        
        public TranslatedResultData(string term, string translation)
        {
            Term = term;
            Translation = translation;
            IsNoError = true;
        }

        public TranslatedResultData Failure(string errorMessage = null)
        {
            IsNoError = false;
            ErrorMessage = errorMessage;
            return this;
        }
    }
}
