using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SerenityAITranslator.Editor.Services.Voice.AiProviders
{
    public class GeminiProvider
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        private const string BaseUrl =
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-preview-tts:generateContent";

        public GeminiProvider(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
        }

        private async Task<byte[]> TextToSpeechAsync(string text, string voiceName = "Kore")
        {
            var request = new GeminiTTSRequest
            {
                Contents = new[]
                {
                    new Content
                    {
                        Parts = new[]
                        {
                            new Part { Text = text }
                        }
                    }
                },
                GenerationConfig = new GenerationConfig
                {
                    ResponseModalities = new[] { "AUDIO" },
                    SpeechConfig = new SpeechConfig
                    {
                        VoiceConfig = new VoiceConfig
                        {
                            PrebuiltVoiceConfig = new PrebuiltVoiceConfig
                            {
                                VoiceName = voiceName
                            }
                        }
                    }
                }
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{BaseUrl}?key={_apiKey}";
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var ttsResponse = JsonConvert.DeserializeObject<GeminiTTSResponse>(responseContent);

            if (ttsResponse?.Candidates == null || ttsResponse.Candidates.Length == 0)
            {
                throw new Exception("No audio data received");
            }

            var audioData = ttsResponse.Candidates[0].Content.Parts[0].InlineData.Data;
            return Convert.FromBase64String(audioData);
        }

        public async Task<string> GenerateSpeechAndSaveAsync(string text, string outputPath, string voiceName = "Kore")
        {
            var audioData = await TextToSpeechAsync(text, voiceName);
            var pcmPath = Path.Combine(outputPath, "output.pcm");
            
            await File.WriteAllBytesAsync(pcmPath, audioData);

            return pcmPath;
        }

        public class GeminiTTSRequest
        {
            [JsonProperty("contents")]
            public Content[] Contents { get; set; }
    
            [JsonProperty("generationConfig")]
            public GenerationConfig GenerationConfig { get; set; }
    
            [JsonProperty("model")]
            public string Model { get; set; } = "gemini-2.5-flash-preview-tts";
        }

        public class Content
        {
            [JsonProperty("parts")]
            public Part[] Parts { get; set; }
        }

        public class Part
        {
            [JsonProperty("text")]
            public string Text { get; set; }
        }

        public class GenerationConfig
        {
            [JsonProperty("responseModalities")]
            public string[] ResponseModalities { get; set; } = new[] { "AUDIO" };
    
            [JsonProperty("speechConfig")]
            public SpeechConfig SpeechConfig { get; set; }
        }

        public class SpeechConfig
        {
            [JsonProperty("voiceConfig")]
            public VoiceConfig VoiceConfig { get; set; }
        }

        public class VoiceConfig
        {
            [JsonProperty("prebuiltVoiceConfig")]
            public PrebuiltVoiceConfig PrebuiltVoiceConfig { get; set; }
        }

        public class PrebuiltVoiceConfig
        {
            [JsonProperty("voiceName")]
            public string VoiceName { get; set; }
        }

        public class GeminiTTSResponse
        {
            [JsonProperty("candidates")]
            public Candidate[] Candidates { get; set; }
        }

        public class Candidate
        {
            [JsonProperty("content")]
            public ResponseContent Content { get; set; }
        }

        public class ResponseContent
        {
            [JsonProperty("parts")]
            public ResponsePart[] Parts { get; set; }
        }

        public class ResponsePart
        {
            [JsonProperty("inlineData")]
            public InlineData InlineData { get; set; }
        }

        public class InlineData
        {
            [JsonProperty("data")]
            public string Data { get; set; }
    
            [JsonProperty("mimeType")]
            public string MimeType { get; set; }
        }
    }
}