using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SerenityAITranslator.Editor.Services.Tts.Collections;
using SerenityAITranslator.Editor.Services.Tts.Models;

namespace SerenityAITranslator.Editor.Services.Tts.AiProviders
{
    public class ElevenLabsTtsProvider : BaseTtsProvider
    {
        private HttpClient _httpClient;

        private const string Extension = ".mp3";
        private const string Prefix = "elevenlabs";
        
        public override async Task<TtsResultData> GetTranslate(TtsPromtData promtData, TtsProvidersConfigurationItem settings, string promt)
        {
            var token = await GetToken(settings);
            
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("xi-api-key", token);

            try
            {
                var apiUrl = string.Concat(settings.Host, settings.Endpoint, settings.VoiceName);
                var data = new
                {
                    text = promtData.Text,
                    model_id = settings.Model,
                    voice_settings = new
                    {
                        stability = 0.5,
                        similarity_boost = 0.5
                    }
                };
                
                var jsonData = JsonConvert.SerializeObject(data);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(apiUrl, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var audioBytes = await response.Content.ReadAsByteArrayAsync();
                    
                    await File.WriteAllBytesAsync($"{promtData.Path}_{Prefix}{Extension}", audioBytes);
                    Console.WriteLine($"Audio saved as: {promtData.Path}_{Prefix}{Extension}");
                    return new TtsResultData(Prefix, Extension);
                }
                else
                {
                    var errorText = await response.Content.ReadAsStringAsync();
                    
                    Console.WriteLine($"Error: {response.StatusCode}");
                    Console.WriteLine($"Message: {errorText}");
                    
                    return new TtsResultData(Prefix, Extension).Failure();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return new TtsResultData(Prefix, Extension).Failure();
            }
        }
    }
}