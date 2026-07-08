using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SerenityAITranslator.Editor.Services.Common.Ai;
using SerenityAITranslator.Editor.Services.Tts.Collections;
using SerenityAITranslator.Editor.Services.Tts.Models;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Tts.AiProviders
{
    public class ResembleTtsProvider : BaseTtsProvider
    {
        private const string Extension = ".mp3";
        private const string Prefix = "resemble";

        public override async Task<TtsResultData> GetTranslate(TtsPromtData promtData,
            TtsProvidersConfigurationItem settings, string promt)
        {
            var apiKey = await GetToken(settings);

            var request = new ResembleRequest()
            {
                VoiceUuid = settings.VoiceName,
                Data = promtData.Text,
                SampleRate = 48000,
                OutputFormat = "mp3"
            };

            var json = JsonConvert.SerializeObject(request);
  
            var apiUrl = string.Concat(settings.Host, settings.Endpoint);
            
            Debug.Log($"Resemble API URL: {apiUrl}");

            try
            {
                var requestResult = await AiRequestService.PostJsonAsync(
                    apiUrl,
                    json,
                    new Dictionary<string, string> { { "Authorization", $"Bearer {apiKey}" } });

                if (requestResult.IsSuccess)
                {
                    var ttsResponse = JsonConvert.DeserializeObject<ResembleResponse>(requestResult.Text);

                    if (ttsResponse?.AudioContent == null)
                        return new TtsResultData(Prefix, Extension).Failure("No audio data received.");

                    var audioBytes = Convert.FromBase64String(ttsResponse.AudioContent);

                    //AudioConverter.ConvertAndSavePcmToWav(audioBytes, $"{promtData.Path}_{Prefix}{Extension}", sampleRate: (int)ttsResponse.SampleRate, channels: 1, bitsPerSample: 16);
                    await File.WriteAllBytesAsync($"{promtData.Path}_{Prefix}{Extension}", audioBytes);

                    Debug.Log($"Audio saved as: {promtData.Path}_{Prefix}{Extension}");

                    return new TtsResultData(Prefix, Extension);
                }

                Debug.LogError($"[ResembleTtsProvider] Error: {requestResult.ErrorMessage}");
                Debug.LogError($"[ResembleTtsProvider] Message: {requestResult.Text}");

                return new TtsResultData(Prefix, Extension).Failure(requestResult.ErrorMessage);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ResembleTtsProvider] Exception: {ex.Message}");
                return new TtsResultData(Prefix, Extension).Failure(ex.Message);
            }
        }

        public class ResembleRequest
        {
            [JsonProperty("voice_uuid")] public string VoiceUuid { get; set; } = string.Empty;
            [JsonProperty("data")] public string Data { get; set; } = string.Empty;
            [JsonProperty("sample_rate")] public int SampleRate { get; set; }
            [JsonProperty("output_format")] public string OutputFormat { get; set; } = "wav";
        }

        public class ResembleResponse
        {
            [JsonProperty("audio_content")]
            public string AudioContent { get; set; } = string.Empty;
            [JsonProperty("audio_timestamps")]
            public AudioTimestamps AudioTimestamps { get; set; } = new AudioTimestamps();
            [JsonProperty("duration")]
            public float Duration { get; set; }
            [JsonProperty("issues")]
            public string[] Issues { get; set; } = Array.Empty<string>();
            [JsonProperty("output_format")]
            public string OutputFormat { get; set; } = string.Empty;
            [JsonProperty("sample_rate")]
            public float SampleRate { get; set; }
            [JsonProperty("success")]
            public bool Success { get; set; }
            [JsonProperty("synth_duration")]
            public float SynthDuration { get; set; }
            [JsonProperty("title")]
            public string Title { get; set; }
        }

        public class AudioTimestamps
        {
            [JsonProperty("graph_chars")]
            public string[] GraphChars { get; set; } = Array.Empty<string>();
            [JsonProperty("graph_times")]
            public float[][] GraphTimes { get; set; } = Array.Empty<float[]>();
            [JsonProperty("phon_chars")]
            public string[] PhonChars { get; set; } = Array.Empty<string>();
            [JsonProperty("phon_times")]
            public float[][] PhonTimes { get; set; } = Array.Empty<float[]>();
        }
    }
}
