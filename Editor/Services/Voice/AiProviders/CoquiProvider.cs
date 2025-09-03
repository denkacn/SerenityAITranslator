using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SerenityAITranslator.Editor.Services.Voice.AiProviders
{
    public class CoquiProvider
    {
        private readonly HttpClient _httpClient;
        
        public CoquiProvider()
        {
            _httpClient = new HttpClient();
        }
        
        public async Task TextToSpeechAsync(string text, string voiceName = "Marcos Rudaski", string language = "en")
        {
            var apiUrl = "http://192.168.0.110:5002/api/tts";
            
            var formData = new MultipartFormDataContent
            {
                { new StringContent(text), "text" },
                { new StringContent(voiceName), "speaker_id" },
                { new StringContent(language), "language_id" }
            };

            try
            {
                var response = await _httpClient.PostAsync(apiUrl, formData);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsByteArrayAsync();
                
                if (result != null)
                {
                    await File.WriteAllBytesAsync("D://output.wav", result);
                    
                    Console.WriteLine("Аудио сохранено как output.wav");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
        
        public class RequestData
        {
            public string text;
            public string speaker_id;
            public string language_id;

            public RequestData(){}
            public RequestData(string text, string speakerID, string languageID)
            {
                this.text = text;
                speaker_id = speakerID;
                language_id = languageID;
            }
        }
    }
}