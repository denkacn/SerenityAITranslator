using System.Threading.Tasks;
using SerenityAITranslator.Editor.Services.Tts.Collections;
using SerenityAITranslator.Editor.Services.Tts.Models;

namespace SerenityAITranslator.Editor.Services.Tts.AiProviders
{
    public interface IAITtsProvider
    {
        Task<TtsResultData> GetTranslate(TtsPromtData promtData, TtsProvidersConfigurationItem settings, string promt);
    }
}