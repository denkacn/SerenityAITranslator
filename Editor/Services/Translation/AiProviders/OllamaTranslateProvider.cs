using System;
using System.Text;
using System.Threading.Tasks;
using SerenityAITranslator.Editor.Services.Common.PromtFactories;
using SerenityAITranslator.Editor.Services.Translation.AiProviders.Settings;
using SerenityAITranslator.Editor.Services.Translation.Models;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace SerenityAITranslator.Editor.Services.Translation.AiProviders
{
    public class OllamaTranslateProvider : BaseTranslateProvider
    {
        public override async Task<TranslatedData> GetTranslate(TranslatedPromtData promtData, BaseTranslateProviderSettings settings, PromtFactoryBase promtFactory)
        {
            var requestData = new Request
            {
                model = settings.Model,
                prompt = promtFactory.GetPromt(promtData),
                stream = false
            };
            
            Debug.Log($"[LmStudioTranslateProvider] The text of the model:  {requestData.prompt}");
            
            var jsonBody = JsonConvert.SerializeObject(requestData);
            var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            var apiUrl = string.Concat(settings.Host, settings.Endpoint);

            using var www = new UnityWebRequest(apiUrl, "POST");
            
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.timeout = 300;
            www.SetRequestHeader("Content-Type", "application/json");

            if (settings.IsTokenExist)
            {
                var token = await GetToken(settings);
                www.SetRequestHeader("Authorization", $"Bearer {token}");
            }

            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Request error:" + www.error);
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
                
                var content = response?.response;
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

            return new TranslatedData(promtData.Term, string.Empty).Failure();
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