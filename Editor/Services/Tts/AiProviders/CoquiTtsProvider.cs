using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using SerenityAITranslator.Editor.Services.Tts.Collections;
using SerenityAITranslator.Editor.Services.Tts.Models;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Tts.AiProviders
{
    public class CoquiTtsProvider : BaseTtsProvider
    {
        private const string Extension = ".wav";
        private const string Prefix = "coqui";
        
        private HttpClient _httpClient;
        
        public override async Task<TtsResultData> GetTranslate(TtsPromtData promtData, TtsProvidersConfigurationItem settings, string promt)
        {
            _httpClient = new HttpClient();
            
            var apiUrl = string.Concat(settings.Host, settings.Endpoint);
            
            var formData = new MultipartFormDataContent
            {
                { new StringContent(promtData.Text), "text" },
                { new StringContent(settings.VoiceName), "speaker_id" },
                { new StringContent(promtData.Language), "language_id" }
            };
            
            try
            {
                var response = await _httpClient.PostAsync(apiUrl, formData);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsByteArrayAsync();
                
                if (result != null)
                {
                    Debug.Log($"Audio saved as: {promtData.Path}_{Prefix}{Extension}");
                    
                    await File.WriteAllBytesAsync($"{promtData.Path}_{Prefix}{Extension}", result);
                    
                    return new TtsResultData(Prefix, Extension);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            
            return new TtsResultData(Prefix, Extension).Failure();
        }
    }
}