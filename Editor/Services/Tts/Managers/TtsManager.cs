using System;
using System.IO;
using System.Linq;
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
        private TranslatedRowData _translatedRowData;
        private bool _isTtsStarted;
        
        public string SelectedTtsProviderId => _context.SessionData.TtsSessionData.TtsSettings != null ? _context.SessionData.TtsSessionData.TtsSettings.Id : string.Empty;
        public bool IsTtsProviderAndTtsSettingSetup => _context != null && _context.SessionData.TtsSessionData.TranslateProvider != null && _context.SessionData.TtsSessionData.TtsSettings != null;
        public TranslatedRowData TranslatedRowData => _translatedRowData;
        
        public TtsManager(SerenityContext context)
        {
            _context = context;
            
            var ttsSessionData = _context.SessionData.TtsSessionData;
            if (!string.IsNullOrEmpty(ttsSessionData.ProviderId))
            {
                var provider = _context.TtsProvidersConfigurations?.Providers?.Find(a => a.Id == ttsSessionData.ProviderId);
                if (provider != null)
                    SelectTranslateProviderSettings(provider);
            }
        }

        public void SelectPromt(PromtSettingsItem promtData)
        {
            if (promtData == null) return;
            
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
            if (provider == null) return;
            
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
                case TtsProviderType.Resemble:
                    ttsSessionData.TranslateProvider = new ResembleTtsProvider();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            SaveSession();
        }

        public void TranslateOne(TranslatedRowData row, Action repaint)
        {
            if (_isTtsStarted) return;
            
            _isTtsStarted = true;
            _ttsCancellationTokenSource?.Dispose();
            _ttsCancellationTokenSource = new CancellationTokenSource();
            _ = RunTtsProcessAsync(row, repaint, _ttsCancellationTokenSource.Token);
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
            else if (_context.VoicesCollection == null)
            {
                _context.SetupVoicesCollection(AssetsUtility.LoadOrCreate<VoicesCollection>(voicesLibraryPath));
            }
        }

        public void Play(TranslatedRowData translatedData)
        {
            if (translatedData == null || _context.VoicesCollection == null) return;
            
            if (AudioClipTools.IsPlaying()) AudioClipTools.StopAll();
            
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
        
        public void Stop()
        {
            AudioClipTools.StopAll();
        }
        
        private async Task TranslateOneAsync(TranslatedRowData translatedData, Action onCompleted, CancellationToken cancellationToken = default)
        {
            if (translatedData == null)
            {
                Debug.LogWarning("[SerenityAI] TTS row is not selected.");
                return;
            }
            
            var ttsSessionData = _context.SessionData.TtsSessionData;
            if (ttsSessionData.TranslateProvider == null || ttsSessionData.TtsSettings == null)
            {
                Debug.LogWarning("[SerenityAI] TTS provider is not configured.");
                return;
            }
            
            if (string.IsNullOrEmpty(ttsSessionData.VoicesLibraryPath))
            {
                CreateVoiceLibrary();
            }
            
            if (_context.VoicesCollection == null || string.IsNullOrEmpty(ttsSessionData.VoicesLibraryPath))
            {
                Debug.LogWarning("[SerenityAI] Voice library is not configured.");
                return;
            }
            
            var correctLanguage = _context.LanguageConverterData?.ConvertLanguageName(_context.SessionData.TranslationSessionData.SourceLanguage);
            if (string.IsNullOrEmpty(correctLanguage))
            {
                Debug.LogWarning("[SerenityAI] Could not convert source language to TTS language code.");
                return;
            }
            
            var path = Path.Combine(PathUtils.GetFullDirectory(ttsSessionData.VoicesLibraryPath), $"{correctLanguage}_{FileNameSanitizer.Sanitize(translatedData.Term)}");
            var text = translatedData.SourceText?.TrimEnd();
            if (string.IsNullOrEmpty(text))
            {
                Debug.LogWarning("[SerenityAI] TTS source text is empty.");
                return;
            }
            
            var promtData = new TtsPromtData(text, correctLanguage, path);
            var result = await ttsSessionData.TranslateProvider
                .GetTranslate(promtData, ttsSessionData.TtsSettings, ttsSessionData.SelectedPromt);

            if (!cancellationToken.IsCancellationRequested && result.IsNoError)
            {
                UnityEditor.AssetDatabase.Refresh();

                var audioClipAssetPath = Path.Combine(PathUtils.GetDirectoryName(ttsSessionData.VoicesLibraryPath),
                    $"{correctLanguage}_{FileNameSanitizer.Sanitize(translatedData.Term)}_{result.Prefix}{result.Extension}");
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
                        TtsInfo = ttsSessionData.TranslateProvider.GetType().Name,
                        VoiceClip = audioClip
                    };
                    
                    _context.VoicesCollection.Add(voiceItem);
                    _context.Save();
                }
                
                onCompleted?.Invoke();
            }
        }

        public void SetForInfo(TranslatedRowData translatedRowData)
        {
            _translatedRowData = translatedRowData;
        }

        public void DeleteVoice(string term, Action repaint)
        {
            if (_context.VoicesCollection == null)
            {
                repaint?.Invoke();
                return;
            }
            
            var language = _context.SessionData.TranslationSessionData.SourceLanguage;
            
            _context.VoicesCollection.Remove(term, language);
            _context.Save();
            repaint?.Invoke();
        }
        
        private async Task RunTtsProcessAsync(TranslatedRowData row, Action repaint, CancellationToken cancellationToken)
        {
            try
            {
                await TranslateOneAsync(row, repaint, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[SerenityAI] TTS process canceled.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SerenityAI] TTS process failed: {ex}");
            }
            finally
            {
                _isTtsStarted = false;
                repaint?.Invoke();
            }
        }
    }
}
