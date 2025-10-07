using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SerenityAITranslator.Editor.Services.Common.PromtFactories;
using SerenityAITranslator.Editor.Services.Translation.Collections;
using SerenityAITranslator.Editor.Services.Translation.Models;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Translation.AiProviders
{
    public class GoogleTranslateProvider : BaseTranslateProvider
    {
        public override async Task<TranslatedResultData> GetTranslate(TranslatedPromtData promtData, TranslateProviderConfigurationItem settings, PromtFactoryBase promtFactory)
        {
            
            var url = $"{string.Concat(settings.Host, settings.Endpoint)}?key={settings.Token}";
            
            Debug.Log(url);
            var httpClient = new HttpClient();
            
            var requestBody = new
            {
                q = promtData.From,
                target = promtData.Language,
                source = "auto",
                format = "text"
            };
            
            var json = JsonConvert.SerializeObject(requestBody);
            Debug.Log(json);
            
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await httpClient.PostAsync(url, content);
            //response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync();
            
            Debug.Log(responseContent); 
            
            var result = JsonConvert.DeserializeObject<TranslationResponse>(responseContent);
            
            if (result.Data != null && result.Data.Translations != null && result.Data.Translations.Length > 0)
            {
                return new TranslatedResultData(promtData.Term, result.Data.Translations[0].TranslatedText);
            }
            else
            {
                return new TranslatedResultData(promtData.Term, string.Empty).Failure();
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