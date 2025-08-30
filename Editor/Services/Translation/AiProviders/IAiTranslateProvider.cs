using System.Threading.Tasks;
using SerenityAITranslator.Editor.Services.Common.PromtFactories;
using SerenityAITranslator.Editor.Services.Translation.AiProviders.Settings;
using SerenityAITranslator.Editor.Services.Translation.Models;

namespace SerenityAITranslator.Editor.Services.Translation.AiProviders
{
    public interface IAiTranslateProvider
    {
        Task<TranslatedData> GetTranslate(TranslatedPromtData promtData, BaseTranslateProviderSettings settings, PromtFactoryBase promtFactory);
    }
}