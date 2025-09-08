using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SerenityAITranslator.Editor.Context;
using SerenityAITranslator.Editor.Services.Common.Collections;
using SerenityAITranslator.Editor.Services.Settings.Models;
using SerenityAITranslator.Editor.Services.Translation.Models;
using SerenityAITranslator.Editor.Services.Tts.AiProviders;
using SerenityAITranslator.Editor.Services.Tts.Collections;
using SerenityAITranslator.Editor.Services.Tts.Models;
using SerenityAITranslator.Editor.Tools;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Tts.Managers
{
    public class TtsManager
    {
        private readonly SerenityContext _context;
        private CancellationTokenSource _ttsCancellationTokenSource = new CancellationTokenSource();
        
        public string SelectedTtsProviderId => _context.SessionData.TtsSessionData.TtsSettings != null ? _context.SessionData.TtsSessionData.TtsSettings.Id : string.Empty;
        public bool IsTtsProviderAndTtsSettingSetup => _context != null && _context.SessionData.TtsSessionData.TranslateProvider != null && _context.SessionData.TtsSessionData.TtsSettings != null;
        
        public TtsManager(SerenityContext context)
        {
            _context = context;
            
            var ttsSessionData = _context.SessionData.TtsSessionData;
            if (!string.IsNullOrEmpty(ttsSessionData.ProviderId))
            {
                var provider = _context.TtsProvidersConfigurations.Providers.Find(a => a.Id == ttsSessionData.ProviderId);
                if (provider != null)
                    SelectTranslateProviderSettings(provider);
            }
        }

        public void SelectPromt(PromtSettingsItem promtData)
        {
            var ttsSessionData = _context.SessionData.TtsSessionData;
            ttsSessionData.SelectedPromt = promtData.Promt;

            SaveSession();
        }
        
        private void SaveSession()
        {
            _context.Save();
        }

        public void SelectTranslateProviderSettings(TtsProvidersConfigurationItem provider)
        {
            var ttsSessionData = _context.SessionData.TtsSessionData;
            ttsSessionData.ProviderId = provider.Id;
            ttsSessionData.TtsSettings = provider;
            
            switch (provider.ProviderType)
            {
                case TtsProviderType.None:
                    break;
                case TtsProviderType.Coqui:
                    ttsSessionData.TranslateProvider = new CoquiTtsProvider();
                    break;
                case TtsProviderType.ElevenLabs:
                    ttsSessionData.TranslateProvider = new ElevenLabsTtsProvider();
                    break;
                case TtsProviderType.Gemini:
                    ttsSessionData.TranslateProvider = new GeminiTtsProvider();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            SaveSession();
        }

        public void TranslateOne(TranslatedRowData row, Action repaint)
        {
            //var ttsSessionData = _context.SessionData.TtsSessionData;
            _ttsCancellationTokenSource = new CancellationTokenSource();
            TranslateOneAsync(row, repaint, _ttsCancellationTokenSource.Token).ConfigureAwait(false);
        }

        public void CreateVoiceLibrary()
        {
            var ttsSessionData = _context.SessionData.TtsSessionData;
            var voicesLibraryPath = "Assets/SerenityAIResources/VoicesLibrary/VoicesLibrary.asset";
            if (!string.IsNullOrEmpty(ttsSessionData.VoicesLibraryPath))
            {
                voicesLibraryPath = ttsSessionData.VoicesLibraryPath;
            }

            if (!AssetsUtility.IsAssetExists(voicesLibraryPath))
            {
                ttsSessionData.VoicesLibraryPath = voicesLibraryPath;
                var voicesCollection = AssetsUtility.LoadOrCreate<VoicesCollection>(voicesLibraryPath);
                _context.SetupVoicesCollection(voicesCollection);
            }
        }

        public void Play(TranslatedRowData translatedData)
        {
            var language = _context.SessionData.TranslationSessionData.SourceLanguage;
            var voiceData = _context.VoicesCollection.Get(translatedData.Term, language);
            
            if (voiceData != null)
            {
                var audioClip = voiceData.VoiceClip;
                if (audioClip != null)
                {
                    AudioClipTools.Play(audioClip);
                }
            }
        }
        
        private async Task TranslateOneAsync(TranslatedRowData translatedData, Action onCompleted, CancellationToken cancellationToken = default)
        {
            var ttsSessionData = _context.SessionData.TtsSessionData;
            var correctLanguage = _context.LanguageConverterData.ConvertLanguageName(_context.SessionData.TranslationSessionData.SourceLanguage);
            var path = Path.Combine(PathUtils.GetFullDirectory(ttsSessionData.VoicesLibraryPath), $"{correctLanguage}_{FileNameSanitizer.Sanitize(translatedData.Term)}");
            var promtData = new TtsPromtData(translatedData.SourceText, correctLanguage, path);
            var result = await ttsSessionData.TranslateProvider
                .GetTranslate(promtData, ttsSessionData.TtsSettings, ttsSessionData.SelectedPromt);

            if (!cancellationToken.IsCancellationRequested && result.IsNoError)
            {
                UnityEditor.AssetDatabase.Refresh();

                var audioClipAssetPath = Path.Combine(PathUtils.GetDirectoryName(ttsSessionData.VoicesLibraryPath),
                    $"{correctLanguage}_{FileNameSanitizer.Sanitize(translatedData.Term)}{result.Extension}");
                var audioClip = AssetsUtility.Load<AudioClip>(audioClipAssetPath);
                
                Debug.Log("audioClipAssetPath: " + audioClipAssetPath);
                Debug.Log(audioClip);
                
                if (audioClip != null)
                {
                    var voiceItem = new VoiceItem()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Language = _context.SessionData.TranslationSessionData.SourceLanguage,
                        Term = translatedData.Term,
                        TtsInfo = ttsSessionData.TranslateProvider.GetType().ToString(),
                        VoiceClip = audioClip
                    };
                    
                    _context.VoicesCollection.Add(voiceItem);
                    _context.Save();
                }
                
                onCompleted?.Invoke();
            }
        }
    }
}