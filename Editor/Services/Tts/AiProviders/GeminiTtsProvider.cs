using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SerenityAITranslator.Editor.Services.Common.Ai;
using SerenityAITranslator.Editor.Services.Tts.Collections;
using SerenityAITranslator.Editor.Services.Tts.Converters;
using SerenityAITranslator.Editor.Services.Tts.Models;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Tts.AiProviders
{
    public class GeminiTtsProvider : BaseTtsProvider
    {
        private const string Extension = ".wav";
        private const string Prefix = "gemini";
        
        public override async Task<TtsResultData> GetTranslate(TtsPromtData promtData, TtsProvidersConfigurationItem settings, string promt)
        {
            var apiKey = await GetToken(settings);
         
            var request = new GeminiTTSRequest
            {
                Contents = new[]
                {
                    new Content
                    {
                        Parts = new[]
                        {
                            new Part { Text = promtData.Text }
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
                                VoiceName = settings.VoiceName ?? "Kore"
                            }
                        }
                    }
                }
            };

            try
            {
                var apiUrl = string.Concat(settings.Host, settings.Endpoint, settings.Model);
                apiUrl = $"{apiUrl}:generateContent?key={apiKey}";
                
                var json = JsonConvert.SerializeObject(request);
                var requestResult = await AiRequestService.PostJsonAsync(apiUrl, json);
                
                if (requestResult.IsSuccess)
                {
                    var ttsResponse = JsonConvert.DeserializeObject<GeminiTTSResponse>(requestResult.Text);

                    if (ttsResponse?.Candidates == null || ttsResponse.Candidates.Length == 0)
                        throw new Exception("No audio data received");

                    var audioData = ttsResponse.Candidates[0].Content.Parts[0].InlineData.Data;
                    var pcmData = Convert.FromBase64String(audioData);
                    
                    AudioConverter.ConvertAndSavePcmToWav(pcmData, $"{promtData.Path}_{Prefix}{Extension}", sampleRate: 24000, channels: 1, bitsPerSample: 16);
                    
                    Debug.Log($"Audio saved as: {promtData.Path}_{Prefix}{Extension}");
                    
                    return new TtsResultData(Prefix, Extension);
                }
                else
                {
                    Debug.LogError($"[GeminiTtsProvider] Error: {requestResult.ErrorMessage}");
                    Debug.LogError($"[GeminiTtsProvider] Message: {requestResult.Text}");
                    
                    return new TtsResultData(Prefix, Extension).Failure(requestResult.ErrorMessage);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return new TtsResultData(Prefix, Extension).Failure(e.Message);
            }
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
