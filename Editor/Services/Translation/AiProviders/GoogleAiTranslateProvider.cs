using System.Threading.Tasks;
using Newtonsoft.Json;
using SerenityAITranslator.Editor.Services.Common.Ai;
using SerenityAITranslator.Editor.Services.Common.PromtFactories;
using SerenityAITranslator.Editor.Services.Translation.Collections;
using SerenityAITranslator.Editor.Services.Translation.Models;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Translation.AiProviders
{
    public class GoogleAiTranslateProvider : BaseTranslateProvider
    {
        public override async Task<TranslatedResultData> GetTranslate(TranslatedPromtData promtData, TranslateProviderConfigurationItem settings, PromtFactoryBase promtFactory)
        {
            var apiKey = await GetToken(settings);
            var url = $"{string.Concat(settings.Host, settings.Endpoint)}{settings.Model}:generateContent";
            
            var jsonBody = JsonConvert.SerializeObject(new RequestBody(promtFactory.GetPromt(promtData)));
            var requestResult = await AiRequestService.PostJsonAsync(
                url,
                jsonBody,
                new System.Collections.Generic.Dictionary<string, string>
                {
                    { "X-goog-api-key", apiKey }
                });
            
            if (!requestResult.IsSuccess)
            {
                Debug.LogError($"[GoogleAiTranslateProvider] Request failed: {requestResult.ErrorMessage}");
                return new TranslatedResultData(promtData.Term, string.Empty).Failure(requestResult.ErrorMessage);
            }
            
            var response = JsonConvert.DeserializeObject<ResponseData>(requestResult.Text);

            if (response != null && response.candidates != null && response.candidates.Length > 0)
            {
                var content = response.candidates[0].content.parts[0].text;
                        
                if (!string.IsNullOrEmpty(content) && content.Length >= 2)
                {
                    while (content.EndsWith("\r") || content.EndsWith("\n"))
                    {
                        content = content.Substring(0, content.Length - 1);
                    }
                            
                    var result = content;
                            
                    if (content.StartsWith("{") && content.EndsWith("}"))
                    {
                        result = content.Substring(1, content.Length - 2);
                    }
                            
                    return new TranslatedResultData(promtData.Term, result);
                }
            }
            
            const string errorMessage = "It was not possible to extract the text from the response.";
            Debug.LogWarning($"[GoogleAiTranslateProvider] {errorMessage}");
            return new TranslatedResultData(promtData.Term, string.Empty).Failure(errorMessage);
        }
        
        [System.Serializable]
        public class RequestBody
        {
            public Content[] contents;

            public RequestBody(string text)
            {
                contents = new Content[]
                {
                    new Content(text)
                };
            }
        }

        [System.Serializable]
        public class Content
        {
            public Part[] parts;
            public Content(string text)
            {
                parts = new Part[] { new Part(text) };
            }
        }

        [System.Serializable]
        public class Part
        {
            public string text;
            public Part(string text)
            {
                this.text = text;
            }
        }
        
        [System.Serializable]
        public class ResponseData
        {
            public Candidate[] candidates;
        }

        [System.Serializable]
        public class Candidate
        {
            public Content content;
        }
    }
}
