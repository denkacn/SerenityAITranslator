using SerenityAITranslator.Editor.Services.Common.Models;

namespace SerenityAITranslator.Editor.Services.Tts.Models
{
    public class TtsResultData : BaseResultData
    {
        public string Extension;
        
        public TtsResultData(string extension)
        {
            Extension = extension;
            IsNoError = true;       
        }
        
        public TtsResultData Failure()
        {
            IsNoError = false;
            return this;
        }
    }
}