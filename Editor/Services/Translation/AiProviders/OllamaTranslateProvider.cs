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
    public class OllamaTranslateProvider : BaseTranslateProvider
    {
        public override async Task<TranslatedResultData> GetTranslate(TranslatedPromtData promtData, TranslateProviderConfigurationItem settings, PromtFactoryBase promtFactory)
        {
            var requestData = new Request
            {
                model = settings.Model,
                prompt = promtFactory.GetPromt(promtData),
                stream = false
            };
            
            var jsonBody = JsonConvert.SerializeObject(requestData);
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
                Debug.LogError($"[OllamaTranslateProvider] Request failed: {requestResult.ErrorMessage}");
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
                
            var content = response?.response;

            if (!string.IsNullOrEmpty(content) && content.Length >= 2)
            {
                var result = content;

                if (content.StartsWith("{") && content.EndsWith("}"))
                {
                    result = content.Substring(1, content.Length - 2);
                }

                return new TranslatedResultData(promtData.Term, result);
            }

            return new TranslatedResultData(promtData.Term, string.Empty).Failure("The answer does not contain the expected data.");
        }

        private struct Request
        {
            public string model { get; set; }
            public string prompt { get; set; }
            public bool stream { get; set; }
        }

        private struct Response
        {
            public string model { get; set; }
            public string response { get; set; }
            public bool done { get; set; }
        }
    }
}
