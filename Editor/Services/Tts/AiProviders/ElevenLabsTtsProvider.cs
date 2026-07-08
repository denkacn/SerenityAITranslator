using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SerenityAITranslator.Editor.Services.Common.Ai;
using SerenityAITranslator.Editor.Services.Tts.Collections;
using SerenityAITranslator.Editor.Services.Tts.Models;

namespace SerenityAITranslator.Editor.Services.Tts.AiProviders
{
    public class ElevenLabsTtsProvider : BaseTtsProvider
    {
        private const string Extension = ".mp3";
        private const string Prefix = "elevenlabs";
        
        public override async Task<TtsResultData> GetTranslate(TtsPromtData promtData, TtsProvidersConfigurationItem settings, string promt)
        {
            var token = await GetToken(settings);

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
                var requestResult = await AiRequestService.PostJsonForBytesAsync(
                    apiUrl,
                    jsonData,
                    new Dictionary<string, string> { { "xi-api-key", token } });
                
                if (requestResult.IsSuccess)
                {
                    var audioBytes = requestResult.Bytes;
                    if (audioBytes == null || audioBytes.Length == 0)
                        return new TtsResultData(Prefix, Extension).Failure("Audio response is empty.");
                    
                    await File.WriteAllBytesAsync($"{promtData.Path}_{Prefix}{Extension}", audioBytes);
                    UnityEngine.Debug.Log($"Audio saved as: {promtData.Path}_{Prefix}{Extension}");
                    return new TtsResultData(Prefix, Extension);
                }
                else
                {
                    UnityEngine.Debug.LogError($"[ElevenLabsTtsProvider] Error: {requestResult.ErrorMessage}");
                    UnityEngine.Debug.LogError($"[ElevenLabsTtsProvider] Message: {requestResult.Text}");
                    
                    return new TtsResultData(Prefix, Extension).Failure(requestResult.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[ElevenLabsTtsProvider] Exception: {ex.Message}");
                return new TtsResultData(Prefix, Extension).Failure(ex.Message);
            }
        }
    }
}
