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
    public class OpenAiTranslateProvider : BaseTranslateProvider
    {
        public override async Task<TranslatedResultData> GetTranslate(TranslatedPromtData promtData, TranslateProviderConfigurationItem settings, PromtFactoryBase promtFactory)
        {
            var request = new Request()
            {
                model = settings.Model,
                input = promtFactory.GetPromt(promtData),
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
                Debug.LogError($"[OpenAiTranslateProvider] Request failed: {requestResult.ErrorMessage}");
                return new TranslatedResultData(promtData.Term, string.Empty).Failure(requestResult.ErrorMessage);
            }
            
            Response response = null;
                    
            try
            {
                response = JsonConvert.DeserializeObject<Response>(requestResult.Text);
            }
            catch (Exception exp)
            {
                Debug.LogError(exp);
                return new TranslatedResultData(promtData.Term, string.Empty).Failure(exp.Message);
            }
                    
            var output = response?.output.Find(o=>o.type == "message");
            if (output != null && output.content != null && output.content.Length > 0)
            {
                var content = output.content[0].text;
                        
                if (!string.IsNullOrEmpty(content) && content.Length >= 2)
                {
                    var result = content;
                            
                    if (content.StartsWith("{") && content.EndsWith("}"))
                    {
                        result = content.Substring(1, content.Length - 2);
                    }
                            
                    return new TranslatedResultData(promtData.Term, result);
                }
            }

            return new TranslatedResultData(promtData.Term, string.Empty).Failure("The answer does not contain the expected data.");
        }
        
        private struct Request
        {
            public string model { get; set; }
            public string input { get; set; }
        }
        
        private class Response
        {
            public List<Output> output { get; set; }
        }

        private class Output
        {
            public string type { get; set; }
            public string message { get; set; }
            public Content[] content { get; set; }
            public string[] summary { get; set; }
        }
        
        private class Content
        {
            public string type { get; set; }
            public string text { get; set; }
        }
    }
}
