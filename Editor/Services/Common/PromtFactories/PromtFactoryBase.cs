using SerenityAITranslator.Editor.Services.Translation.Models;

namespace SerenityAITranslator.Editor.Services.Common.PromtFactories
{
    public abstract class PromtFactoryBase
    {
        public abstract string GetPromt(TranslatedPromtData promtData);
    }
}