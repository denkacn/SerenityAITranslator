using SerenityAITranslator.Editor.Services.Translation.Models;

namespace SerenityAITranslator.Editor.Services.Common.PromtFactories
{
    public class PromtFactorySimple : PromtFactoryBase
    {
        public override string GetPromt(TranslatedPromtData promtData)
        {
            return string.Format(promtData.Promt, promtData.Language, promtData.From);
        }
    }
}