using SerenityAITranslator.Editor.Services.Common.Models;

namespace SerenityAITranslator.Editor.Services.Tts.Models
{
    public class TtsResultData : BaseResultData
    {
        public string Prefix;
        public string Extension;
        
        public TtsResultData(string prefix, string extension)
        {
            Prefix = prefix;
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