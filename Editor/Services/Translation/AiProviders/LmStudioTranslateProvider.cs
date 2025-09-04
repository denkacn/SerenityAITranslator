using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SerenityAITranslator.Editor.Services.Common.PromtFactories;
using SerenityAITranslator.Editor.Services.Translation.Collections;
using SerenityAITranslator.Editor.Services.Translation.Models;
using UnityEngine;
using UnityEngine.Networking;

namespace SerenityAITranslator.Editor.Services.Translation.AiProviders
{
    public class LmStudioTranslateProvider : BaseTranslateProvider
    {
        public override async Task<TranslatedData> GetTranslate(TranslatedPromtData promtData, TranslateProviderConfigurationItem settings, PromtFactoryBase promtFactory)
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
            var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            var apiUrl = string.Concat(settings.Host, settings.Endpoint);

            using (var www = new UnityWebRequest(apiUrl, "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                if (settings.IsTokenExist)
                {
                    var token = await GetToken(settings);
                    www.SetRequestHeader("Authorization", $"Bearer {token}");
                }

                www.timeout = 300;

                await www.SendWebRequest();
                
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("[LmStudioTranslateProvider] Web Request error: " + www.error);
                }
                else
                {
                    Response? response = null;
                    
                    try
                    {
                        response = JsonConvert.DeserializeObject<Response>(www.downloadHandler.text);
                    }
                    catch (Exception exp)
                    {
                        Debug.LogError(exp);
                        return new TranslatedData(promtData.Term, string.Empty).Failure();
                    }
                    
                    if (response?.choices?.Length > 0 && response?.choices[0].message.content != null)
                    {
                        var content = response?.choices[0].message.content;
                        Debug.Log($"[LmStudioTranslateProvider] The text of the model:  {content}");

                        if (content.Length >= 2)
                        {
                            var result = content;
                            
                            if (content.StartsWith("{") && content.EndsWith("}"))
                            {
                                result = content.Substring(1, content.Length - 2);
                            }
                            
                            return new TranslatedData(promtData.Term, result);
                        }
                    }
                    else
                    {
                        Debug.LogError("[LmStudioTranslateProvider] The answer does not contain the expected data.");
                    }

                    return new TranslatedData(promtData.Term, string.Empty).Failure();
                }

            }

            return new TranslatedData(promtData.Term, string.Empty).Failure();
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