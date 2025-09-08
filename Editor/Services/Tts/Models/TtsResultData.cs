using SerenityAITranslator.Editor.Services.Common.Models;

namespace SerenityAITranslator.Editor.Services.Tts.Models
{
    public class TtsResultData : BaseResultData
    {
        public TtsResultData()
        {
            IsNoError = true;       
        }
        
        public TtsResultData Failure()
        {
            IsNoError = false;
            return this;
        }
    }
}