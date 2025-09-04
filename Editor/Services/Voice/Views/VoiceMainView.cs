using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using SerenityAITranslator.Editor.Context;
using SerenityAITranslator.Editor.Services.Common.Views;
using SerenityAITranslator.Editor.Services.Voice.AiProviders;
using SerenityAITranslator.Editor.Services.Voice.Converters;
using UnityEditor;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Voice.Views
{
    public class VoiceMainView : MainView
    {
        public VoiceMainView(EditorWindow owner, SerenityContext context) : base(owner, context)
        {
            
        }
        
        public override void Init()
        {

        }

        public override void Draw()
        {
            if (GUILayout.Button("TestGetVoice"))
            {
                //TestGetVoiceCoqui();
                //TestGetVoiceGemini();
                TestGetVoiceElevenLabs();
            }
        }

        private async Task TestGetVoiceElevenLabs()
        {
            var voiceId = "21m00Tcm4TlvDq8ikWAM"; // Пример: голос Rachel
        
            var textToSpeak = "Hello! This is a test of ElevenLabs TTS API using C#. It's working perfectly!";
            var outputFilePath = "D://output.mp3";

            var service = new ElevenLabsProvider();
            var success = await service.TextToSpeechAsync(voiceId, textToSpeak, outputFilePath);
        }

        private async Task TestGetVoiceGemini()
        {
            var apiKey = "AIzaSyBuoxBIyA1o06ErQFY_KN_GHYRG4P0KwRg";
            var ttsClient = new GeminiProvider(apiKey);
            
            var pcmPath = await ttsClient.GenerateSpeechAndSaveAsync(
                "Say cheerfully: Have a wonderful day!",
                "./",
                "Kore"
            );
            
            var wavPath = "D://output.wav";
            AudioConverter.ConvertPcmToWav(pcmPath, wavPath);
            
            Debug.Log($"WAV file saved: {wavPath}");
            
            File.Delete(pcmPath);
            Debug.Log("Temporary PCM file deleted");
        }
        
        private readonly HttpClient _httpClient = new HttpClient();
        
        private async Task TestGetVoiceCoqui()
        {
            var voiceClient = new CoquiProvider();
            await voiceClient.TextToSpeechAsync("Here's how you can send a request from C# to Coqui TTS - using a local server hosted with tts-server and making HTTP calls to it", "Marcos Rudaski", "en");
            
        }
        
        public async Task<bool> CheckServerStatusAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("http://192.168.0.110:5002");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}