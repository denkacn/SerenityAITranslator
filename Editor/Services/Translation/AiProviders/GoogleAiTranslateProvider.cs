using System.Threading.Tasks;
using Newtonsoft.Json;
using SerenityAITranslator.Editor.Services.Common.PromtFactories;
using SerenityAITranslator.Editor.Services.Translation.AiProviders.Settings;
using SerenityAITranslator.Editor.Services.Translation.Models;
using UnityEngine;
using UnityEngine.Networking;

namespace SerenityAITranslator.Editor.Services.Translation.AiProviders
{
    public class GoogleAiTranslateProvider : BaseTranslateProvider
    {
        public override async Task<TranslatedData> GetTranslate(TranslatedPromtData promtData, BaseTranslateProviderSettings settings, PromtFactoryBase promtFactory)
        {
            var apiKey = await GetToken(settings);
            var url = $"{string.Concat(settings.Host, settings.Endpoint)}{settings.Model}:generateContent";
            Debug.Log(url);
            
            var jsonBody = JsonConvert.SerializeObject(new RequestBody(promtFactory.GetPromt(promtData)));
            var bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            
            using (var www = new UnityWebRequest(url, "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("X-goog-api-key", apiKey);
                www.timeout = 300;
                
                await www.SendWebRequest();
                
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("[GoogleAiTranslateProvider] Web Request error: " + www.error);
                }
                else
                {
                    var jsonResponse = www.downloadHandler.text;
                    var response = JsonConvert.DeserializeObject<ResponseData>(jsonResponse);

                    if (response != null && response.candidates != null && response.candidates.Length > 0)
                    {
                        var content = response.candidates[0].content.parts[0].text;
                        
                        if (content.Length >= 2)
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
                            
                            return new TranslatedData(promtData.Term, result);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[GoogleAiTranslateProvider] It was not possible to extract the text from the response!");
                    }
                }
            }
            
            return new TranslatedData(promtData.Term, string.Empty).Failure();
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