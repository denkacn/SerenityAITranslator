using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using I2.Loc;
using Newtonsoft.Json;
using SerenityAITranslator.Editor.Services.Common.Models;
using SerenityAITranslator.Editor.Services.Translation.AiProviders;
using SerenityAITranslator.Editor.Services.Translation.AiProviders.Settings;
using SerenityAITranslator.Editor.Services.Translation.Context;
using SerenityAITranslator.Editor.Services.Translation.Models;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Translation.Managers
{
    [Serializable]
    public class I2LocAiTranslateExtensionManager
    {
        private const string DefaultPromt =
            "Translate fully the following into {0} and return ONLY the translations in the same format, be case-sensitive, consider line breaks, inside curly braces, with nothing else in the output:\n{1}";
        
        private readonly LanguageSourceAsset _languageSourceAsset;
        private readonly I2LocAiExtensionContext _context;
        
        private List<string> _availableLanguages = new List<string>();
        private List<TranslationRowData> _translationsData = new List<TranslationRowData>();
        private List<BaseTranslateProviderSettings> _translateProviderSettingsList = new List<BaseTranslateProviderSettings>();
        private List<PromtSettingData> _promtSettingsData = new List<PromtSettingData>();
        private CancellationTokenSource _translateAllCancellationTokenSource = new CancellationTokenSource();

        private bool _isTranslateAllStarted = false;
        
        public List<string> GetAvailableLanguages() => _availableLanguages;
        public List<TranslationRowData> GetTranslationsData() => _translationsData;
        public List<BaseTranslateProviderSettings> GetTranslateProviderSettingsList() => _translateProviderSettingsList;
        public List<PromtSettingData> GetPromtSettingsData() => _promtSettingsData;
        
        public string SourceLanguage { get; set; }
        public string DestinationLanguage { get; set; }
        public string SelectedPromt { get; set; }
        public bool IsContextSetup => _context != null;
        public bool IsTranslateProviderAndTranslateSettingSetup => _context != null && _context.TranslateProvider != null && _context.TranslateSettings != null;
        public string SelectedTranslateProviderId => _context.TranslateSettings != null ? _context.TranslateSettings.Id : string.Empty;
        public bool IsTranslateAllStarted => _isTranslateAllStarted;

        public I2LocAiTranslateExtensionManager(LanguageSourceAsset languageSourceAsset, I2LocAiExtensionContext context)
        {
            _languageSourceAsset = languageSourceAsset;
            _context = context;

            foreach (var languageData in _languageSourceAsset.SourceData.mLanguages)
            {
                _availableLanguages.Add(languageData.Name);
            }
            
            SourceLanguage = _availableLanguages[0];
            DestinationLanguage = _availableLanguages[0];

            LoadTranslateProviderSettings();
            LoadPromtSettings();
        }
        
        public void GetTranslationTerms(string filter = null)
        {
            _translationsData.Clear();
            
            var terms = _languageSourceAsset.SourceData.mTerms;
            var index = 0;
            
            foreach (var term in terms)
            {
                if (string.IsNullOrEmpty(term.Term)) continue;
                if (filter != null && !term.Term.Contains(filter)) continue;

                _translationsData.Add(new TranslationRowData(index, term.Term,
                    term.Languages[_availableLanguages.IndexOf(SourceLanguage)],
                    term.Languages[_availableLanguages.IndexOf(DestinationLanguage)]));

                index++;
            }
        }
        
        public void TranslateAll(Action onCompleted)
        {
            if (_isTranslateAllStarted) return;
            
            _isTranslateAllStarted = true;
            _translateAllCancellationTokenSource = new CancellationTokenSource();
            TranslateAllAsync(onCompleted, _translateAllCancellationTokenSource.Token).ConfigureAwait(false);
        }

        public void TranslateOne(TranslationRowData translationData, Action onCompleted)
        {
            if (_isTranslateAllStarted) return;
            
            TranslateOneAsync(translationData, onCompleted).ConfigureAwait(false);
        }

        public void StopTranslateProcess()
        {
            _translateAllCancellationTokenSource.Cancel();
            _translateAllCancellationTokenSource.Dispose();
            _translateAllCancellationTokenSource = new CancellationTokenSource();

            _isTranslateAllStarted = false;
        }

        #region Async Operations

        public async Task TranslateOneAsync(TranslationRowData translationData, 
            Action onCompleted,
            CancellationToken cancellationToken = default)
        {
            var promtData = new TranslatedPromtData(
                translationData.Term, 
                DestinationLanguage,
                translationData.SourceText,
                string.IsNullOrEmpty(SelectedPromt) ? DefaultPromt : SelectedPromt);

            var result = await _context.TranslateProvider
                .GetTranslate(promtData, _context.TranslateSettings, _context.PromtFactory);

            if (!cancellationToken.IsCancellationRequested && result.IsNoError)
            {
                translationData.TranslatedText = result.Translation;
                translationData.IsShowTranslated = true;
                onCompleted?.Invoke();
            }
        }
        
        public async Task TranslateAllAsync(Action onCompleted, CancellationToken cancellationToken = default)
        {
            var translationsData = GetTranslationsData();

            foreach (var translationData in translationsData)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                await TranslateOneAsync(translationData, onCompleted, cancellationToken);
            }
            
            _isTranslateAllStarted = false;
        }

        #endregion
        
        public bool ApplyChanges()
        {
            var translationsData = GetTranslationsData();

            foreach (var translationData in translationsData)
            {
                if (string.IsNullOrEmpty(translationData.TranslatedText)) continue;
                
                _languageSourceAsset.SourceData.mTerms.Find(t => t.Term == translationData.Term)
                    .Languages[_availableLanguages.IndexOf(DestinationLanguage)] = translationData.TranslatedText;
            }

            return true;
        }

        public bool ApplyChange(TranslationRowData translationData)
        {
            if (string.IsNullOrEmpty(translationData.TranslatedText)) return false;
            
            _languageSourceAsset.SourceData.mTerms.Find(t => t.Term == translationData.Term)
                .Languages[_availableLanguages.IndexOf(DestinationLanguage)] = translationData.TranslatedText;
            
            translationData.OriginalText = translationData.TranslatedText;
            
            return true;
        }
        
        public void RewertChange(TranslationRowData translationData)
        {
            translationData.TranslatedText = translationData.OriginalText;
        }

        public void AddTranslateProviderSettings(BaseTranslateProviderSettings translateProviderSettings)
        {
            translateProviderSettings.Id = Guid.NewGuid().ToString();
            _translateProviderSettingsList.Add(translateProviderSettings);

            SaveTranslateProviderSettings();
        }
        
        public void RemoveTranslateProviderSettings(BaseTranslateProviderSettings translateProviderSettings)
        {
            _translateProviderSettingsList.RemoveAll(a => a.Id == translateProviderSettings.Id);
            SaveTranslateProviderSettings();
        }
        
        public void SelectTranslateProviderSettings(BaseTranslateProviderSettings provider)
        {
            _context.TranslateSettings = provider;
            switch (provider.TextProviderType)
            {
                case AiTextProviderType.None:
                    break;
                case AiTextProviderType.LmStudio:
                    _context.TranslateProvider = new LmStudioTranslateProvider();
                    break;
                case AiTextProviderType.Ollama:
                    _context.TranslateProvider = new OllamaTranslateProvider();
                    break;
                case AiTextProviderType.OpenAi:
                    _context.TranslateProvider = new OpenAiTranslateProvider();
                    break;
                case AiTextProviderType.DeepSeek:
                    _context.TranslateProvider = new LmStudioTranslateProvider();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public void AddPromt(PromtSettingData promtData)
        {
            promtData.Id = Guid.NewGuid().ToString();
            _promtSettingsData.Add(promtData);
            
            SavePromtSettings();
        }
        
        public void RemovePromt(PromtSettingData promtData)
        {
            _promtSettingsData.RemoveAll(a => a.Id == promtData.Id);
            SavePromtSettings();
        }
        
        public void SelectPromt(PromtSettingData promtData)
        {
            SelectedPromt = promtData.Promt;
        }

        public string GetInfo()
        {
            if (_context == null) return string.Empty;
            
            
            return
                $"BaseLanguage: {_context.BaseLanguage}\n" +
                $"TranslateProvider: {_context.TranslateProvider?.GetType().Name}\n" +
                $"PromtFactory: {_context.PromtFactory.GetType().Name}\n" +
                $"Host: {string.Concat(_context.TranslateSettings?.Host, _context.TranslateSettings?.Endpoint)}\n" +
                $"Model: {_context.TranslateSettings?.Model}\n" +
                $"Promt: {(string.IsNullOrEmpty(SelectedPromt) ? DefaultPromt : SelectedPromt)}";
        }

        #region Save Load

        private void SaveTranslateProviderSettings()
        {
            if (!Directory.Exists(Path.Combine(Application.dataPath, "Editor")))
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "Editor"));           
            }
            
            var path = Path.Combine(Application.dataPath, "Editor", "TranslateProviderSettings.json");
            var json = JsonConvert.SerializeObject(_translateProviderSettingsList, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        private void LoadTranslateProviderSettings()
        {
            var path = Path.Combine(Application.dataPath, "Editor", "TranslateProviderSettings.json");

            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                _translateProviderSettingsList = JsonConvert.DeserializeObject<List<BaseTranslateProviderSettings>>(json);
            }
            else
            {
                _translateProviderSettingsList.Clear();
            }
        }
        
        private void SavePromtSettings()
        {
            if (!Directory.Exists(Path.Combine(Application.dataPath, "Editor")))
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "Editor"));           
            }
            
            var path = Path.Combine(Application.dataPath, "Editor", "PromtSettings.json");
            var json = JsonConvert.SerializeObject(_promtSettingsData, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        private void LoadPromtSettings()
        {
            var path = Path.Combine(Application.dataPath, "Editor", "PromtSettings.json");

            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                _promtSettingsData = JsonConvert.DeserializeObject<List<PromtSettingData>>(json);
            }
            else
            {
                _promtSettingsData.Clear();
            }
        }

        #endregion
    }
}