using SerenityAITranslator.Editor.Services.Common.Enums;

namespace SerenityAITranslator.Editor.Session.Models
{
    public class SessionData
    {
        public SerenityServiceType ServiceType { get; set; } = SerenityServiceType.None;
        public TranslationSessionData TranslationSessionData { get; set; } = new TranslationSessionData();
    }
}