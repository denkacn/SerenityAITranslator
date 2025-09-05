using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SerenityAITranslator.Editor.Services.Tts.AiProviders
{
    public class ElevenLabsTtsProvider
    {
        private readonly string _apiKey = "sk_caf85f82415adf253311e446a3215b30bf84622c21783551";
        private readonly string _apiUrl = "https://api.elevenlabs.io/v1/";
        private readonly HttpClient _httpClient;

        public ElevenLabsTtsProvider()
        {
            //_apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("xi-api-key", _apiKey);
        }

        public async Task<bool> TextToSpeechAsync(string voiceId, string text, string outputPath)
        {
            try
            {
                var url = $"{_apiUrl}text-to-speech/{voiceId}";
                
                var data = new
                {
                    text = text,
                    model_id = "eleven_multilingual_v2", 
                    voice_settings = new
                    {
                        stability = 0.5,
                        similarity_boost = 0.5
                    }
                };
                
                var jsonData = JsonConvert.SerializeObject(data);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(url, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var audioBytes = await response.Content.ReadAsByteArrayAsync();
                    
                    await File.WriteAllBytesAsync(outputPath, audioBytes);
                    Console.WriteLine($"Аудио успешно сохранено в: {outputPath}");
                    return true;
                }
                else
                {
                    var errorText = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ошибка: {response.StatusCode}");
                    Console.WriteLine($"Сообщение: {errorText}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Исключение: {ex.Message}");
                return false;
            }
        }
    }
}