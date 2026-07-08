using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using SerenityAITranslator.Editor.Services.Common.Ai;
using SerenityAITranslator.Editor.Services.Tts.Collections;
using SerenityAITranslator.Editor.Services.Tts.Models;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Tts.AiProviders
{
    public class CoquiTtsProvider : BaseTtsProvider
    {
        private const string Extension = ".wav";
        private const string Prefix = "coqui";
        
        public override async Task<TtsResultData> GetTranslate(TtsPromtData promtData, TtsProvidersConfigurationItem settings, string promt)
        {
            var apiUrl = string.Concat(settings.Host, settings.Endpoint);
            
            var formData = new MultipartFormDataContent
            {
                { new StringContent(promtData.Text), "text" },
                { new StringContent(settings.VoiceName), "speaker_id" },
                { new StringContent(promtData.Language), "language_id" }
            };
            
            try
            {
                var requestResult = await AiRequestService.PostForBytesAsync(apiUrl, formData);
                if (requestResult.IsSuccess && requestResult.Bytes != null)
                {
                    Debug.Log($"Audio saved as: {promtData.Path}_{Prefix}{Extension}");
                    
                    await File.WriteAllBytesAsync($"{promtData.Path}_{Prefix}{Extension}", requestResult.Bytes);
                    
                    return new TtsResultData(Prefix, Extension);
                }
                
                return new TtsResultData(Prefix, Extension).Failure(requestResult.ErrorMessage);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CoquiTtsProvider] Request failed: {ex.Message}");
                return new TtsResultData(Prefix, Extension).Failure(ex.Message);
            }
        }
    }
}
