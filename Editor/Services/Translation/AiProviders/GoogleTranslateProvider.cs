using System.Threading.Tasks;
using Newtonsoft.Json;
using SerenityAITranslator.Editor.Services.Common.Ai;
using SerenityAITranslator.Editor.Services.Common.PromtFactories;
using SerenityAITranslator.Editor.Services.Translation.Collections;
using SerenityAITranslator.Editor.Services.Translation.Models;

namespace SerenityAITranslator.Editor.Services.Translation.AiProviders
{
    public class GoogleTranslateProvider : BaseTranslateProvider
    {
        public override async Task<TranslatedResultData> GetTranslate(TranslatedPromtData promtData, TranslateProviderConfigurationItem settings, PromtFactoryBase promtFactory)
        {
            var token = await GetToken(settings);
            var url = $"{string.Concat(settings.Host, settings.Endpoint)}?key={token}";

            var requestBody = new
            {
                q = promtData.From,
                target = promtData.Language,
                source = "auto",
                format = "text"
            };
            
            var json = JsonConvert.SerializeObject(requestBody);
            var requestResult = await AiRequestService.PostJsonAsync(url, json);
            if (!requestResult.IsSuccess)
            {
                return new TranslatedResultData(promtData.Term, string.Empty).Failure(requestResult.ErrorMessage);
            }
            
            var result = JsonConvert.DeserializeObject<TranslationResponse>(requestResult.Text);
            
            if (result.Data != null && result.Data.Translations != null && result.Data.Translations.Length > 0)
            {
                return new TranslatedResultData(promtData.Term, result.Data.Translations[0].TranslatedText);
            }
            else
            {
                return new TranslatedResultData(promtData.Term, string.Empty).Failure("The answer does not contain the expected data.");
            }
        }
        
        private class TranslationResponse
        {
            public Data Data { get; set; }
        }
    
        private class Data
        {
            public Translation[] Translations { get; set; }
        }
    
        private class Translation
        {
            public string TranslatedText { get; set; }
            public string DetectedSourceLanguage { get; set; }
        }
    }
}
