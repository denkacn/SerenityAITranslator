using SerenityAITranslator.Editor.Services.Settings.Models;

namespace SerenityAITranslator.Editor.Services.Tts.AiProviders
{
    public static class TtsProviderFactory
    {
        public static IAITtsProvider Create(TtsProviderType providerType)
        {
            switch (providerType)
            {
                case TtsProviderType.Coqui:
                    return new CoquiTtsProvider();
                case TtsProviderType.ElevenLabs:
                    return new ElevenLabsTtsProvider();
                case TtsProviderType.Gemini:
                    return new GeminiTtsProvider();
                case TtsProviderType.Resemble:
                    return new ResembleTtsProvider();
                case TtsProviderType.None:
                default:
                    return null;
            }
        }
    }
}
