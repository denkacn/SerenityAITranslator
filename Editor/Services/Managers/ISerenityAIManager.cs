using SerenityAITranslator.Editor.Services.Translation.Managers;
using SerenityAITranslator.Editor.Session.Models;

namespace SerenityAITranslator.Editor.Services.Managers
{
    public interface ISerenityAIManager
    {
        SessionData Session { get; }
        TranslateManager TranslateManager { get; }
        void SaveSession();
    }
}