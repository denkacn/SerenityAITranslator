using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SerenityAITranslator.Editor.Services.Common.Ai;
using SerenityAITranslator.Editor.Services.Common.PromtFactories;
using SerenityAITranslator.Editor.Services.Translation.Collections;
using SerenityAITranslator.Editor.Services.Translation.Models;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Translation.AiProviders
{
    public class LmStudioTranslateProvider : BaseTranslateProvider
    {
        public override async Task<TranslatedResultData> GetTranslate(TranslatedPromtData promtData, TranslateProviderConfigurationItem settings, PromtFactoryBase promtFactory)
        {
            var request = new ChatRequest()
            {
                model = settings.Model,
                messages = new Message[]
                {
                    new Message { role = "system", content = "You are a helpful assistant." },
                    new Message { role = "user", content = promtFactory.GetPromt(promtData) }
                },
                stream = false
            };
            
            var jsonBody = JsonConvert.SerializeObject(request);
            var apiUrl = string.Concat(settings.Host, settings.Endpoint);
            var headers = new Dictionary<string, string>();
            
            if (settings.IsTokenExist)
            {
                var token = await GetToken(settings);
                headers.Add("Authorization", $"Bearer {token}");
            }

            var requestResult = await AiRequestService.PostJsonAsync(apiUrl, jsonBody, headers);
            if (!requestResult.IsSuccess)
            {
                Debug.LogError($"[LmStudioTranslateProvider] Request failed: {requestResult.ErrorMessage}");
                return new TranslatedResultData(promtData.Term, string.Empty).Failure(requestResult.ErrorMessage);
            }
            
            Response? response = null;
                    
            try
            {
                response = JsonConvert.DeserializeObject<Response>(requestResult.Text);
            }
            catch (Exception exp)
            {
                Debug.LogError(exp);
                return new TranslatedResultData(promtData.Term, string.Empty).Failure(exp.Message);
            }
                    
            if (response?.choices?.Length > 0 && response?.choices[0].message.content != null)
            {
                var content = response?.choices[0].message.content;

                if (content.Length >= 2)
                {
                    var result = content;
                            
                    if (content.StartsWith("{") && content.EndsWith("}"))
                    {
                        result = content.Substring(1, content.Length - 2);
                    }
                            
                    return new TranslatedResultData(promtData.Term, result);
                }
            }
            
            const string errorMessage = "The answer does not contain the expected data.";
            Debug.LogError($"[LmStudioTranslateProvider] {errorMessage}");

            return new TranslatedResultData(promtData.Term, string.Empty).Failure(errorMessage);
        }
        
        private struct ChatRequest
        {
            public string model { get; set; }
            public Message[] messages { get; set; }
            public bool stream { get; set; }
        }
        private struct Message
        {
            public string role { get; set; }
            public string content { get; set; }
        }
        private struct Response
        {
            public Choice[] choices { get; set; }
        }
        private struct Choice
        {
            public Message message { get; set; }
        }
    }
}
