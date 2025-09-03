using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SerenityAITranslator.Editor.Services.Common.Models;
using SerenityAITranslator.Editor.Services.Translation.AiProviders;
using SerenityAITranslator.Editor.Services.Translation.AiProviders.Settings;
using SerenityAITranslator.Editor.Services.Translation.Context;
using SerenityAITranslator.Editor.Services.Translation.Models;
using SerenityAITranslator.Editor.Services.Translation.SourceAssetProvider;
using SerenityAITranslator.Editor.Tools;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Translation.Managers
{
    public class TranslateManager
    {
        private const string DefaultPromt =
            "Translate fully the following into {0} and return ONLY the translations in the same format, be case-sensitive, consider line breaks, inside curly braces, with nothing else in the output:\n{1}";
        private const string TranslateProviderSettingsFileName = "TranslateProviderSettings.json";
        private const string PromtSettingsFileName = "PromtSettings.json";
        
        private ISourceAssetProvider _sourceAssetProvider;
        private TranslatedContext _context;
        
        private List<string> _availableLanguages;
        private readonly List<TranslatedRowData> _translationsData = new List<TranslatedRowData>();
        private List<BaseTranslateProviderSettings> _translateProviderSettingsList = new List<BaseTranslateProviderSettings>();
        private List<PromtSettingData> _promtSettingsData = new List<PromtSettingData>();
        private CancellationTokenSource _translateAllCancellationTokenSource = new CancellationTokenSource();

        private bool _isTranslateAllStarted = false;
        
        public List<string> GetAvailableLanguages() => _availableLanguages;
        public List<TranslatedRowData> GetTranslationsData() => _translationsData;
        public List<BaseTranslateProviderSettings> GetTranslateProviderSettingsList() => _translateProviderSettingsList;
        public List<PromtSettingData> GetPromtSettingsData() => _promtSettingsData;
        
        public string SourceLanguage { get; set; }
        public string DestinationLanguage { get; set; }
        public string SelectedPromt { get; set; }
        public bool IsContextSetup => _context != null;
        public bool IsTranslateProviderAndTranslateSettingSetup => _context != null && _context.TranslateProvider != null && _context.TranslateSettings != null;
        public string SelectedTranslateProviderId => _context.TranslateSettings != null ? _context.TranslateSettings.Id : string.Empty;
        public bool IsTranslateAllStarted => _isTranslateAllStarted;

        public TranslateManager(){}
        
        public async Task SetContext(TranslatedContext context)
        {
            _context = context;
            
            await LoadTranslateProviderSettings();
            await LoadPromtSettings();
        }
        
        public void SetSourceAssetProvider(ISourceAssetProvider sourceAssetProvider)
        {
            _sourceAssetProvider = sourceAssetProvider;

            _availableLanguages = _sourceAssetProvider.GetLanguages();
            Debug.Log(string.Join(";", _availableLanguages));
            
            SourceLanguage = _availableLanguages[0];
            DestinationLanguage = _availableLanguages[0];
        }
        
        public void GetTranslationTerms(string filter = null)
        {
            _translationsData.Clear();

            var terms = _sourceAssetProvider.GetTerms();
            var index = 0;
            
            foreach (var term in terms)
            {
                if (string.IsNullOrEmpty(term.Term)) continue;
                if (filter != null && !term.Term.Contains(filter)) continue;

                _translationsData.Add(new TranslatedRowData(index, term.Term,
                    term.Languages[_availableLanguages.IndexOf(SourceLanguage)],
                    term.Languages[_availableLanguages.IndexOf(DestinationLanguage)]));

                index++;
            }
        }
        
        public void GetTranslationTermsByGroup(string group, string filter = null)
        {
            _translationsData.Clear();

            var terms = _sourceAssetProvider.GetTerms(group);
            var index = 0;
            
            foreach (var term in terms)
            {
                if (string.IsNullOrEmpty(term.Term)) continue;
                if (filter != null && !term.Term.Contains(filter)) continue;

                _translationsData.Add(new TranslatedRowData(index, term.Term,
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

        public void TranslateOne(TranslatedRowData translatedData, Action onCompleted)
        {
            if (_isTranslateAllStarted) return;
            
            TranslateOneAsync(translatedData, onCompleted).ConfigureAwait(false);
        }

        public void StopTranslateProcess()
        {
            _translateAllCancellationTokenSource.Cancel();
            _translateAllCancellationTokenSource.Dispose();
            _translateAllCancellationTokenSource = new CancellationTokenSource();

            _isTranslateAllStarted = false;
        }

        #region Async Operations

        private async Task TranslateOneAsync(TranslatedRowData translatedData, 
            Action onCompleted,
            CancellationToken cancellationToken = default)
        {
            var promtData = new TranslatedPromtData(
                translatedData.Term, 
                DestinationLanguage,
                translatedData.SourceText,
                string.IsNullOrEmpty(SelectedPromt) ? DefaultPromt : SelectedPromt);

            var result = await _context.TranslateProvider
                .GetTranslate(promtData, _context.TranslateSettings, _context.PromtFactory);

            if (!cancellationToken.IsCancellationRequested && result.IsNoError)
            {
                translatedData.TranslatedText = result.Translation;
                translatedData.IsShowTranslated = true;
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
        
        public void ApplyChanges(Action<bool> onCompleted)
        {
            var translationsData = GetTranslationsData();
            var terms = _sourceAssetProvider.GetTerms();
            var destinationLanguageIndex = _availableLanguages.IndexOf(DestinationLanguage);
            
            foreach (var translationData in translationsData)
            {
                if (string.IsNullOrEmpty(translationData.TranslatedText)) continue;
                var term = terms.Find(t => t.Term == translationData.Term);
                if (term == null) continue;
                
                if (term.Languages[destinationLanguageIndex] != translationData.TranslatedText)
                {
                    term.Languages[destinationLanguageIndex] = translationData.TranslatedText;
                    term.IsUpdated = true;
                }
            }
            
            _sourceAssetProvider.ApplyChanges(DestinationLanguage, onCompleted);
        }

        public void ApplyChange(TranslatedRowData translatedData, Action<bool> onCompleted)
        {
            if (string.IsNullOrEmpty(translatedData.TranslatedText))
            {
                onCompleted?.Invoke(false);
                return;
            }
            
            var terms = _sourceAssetProvider.GetTerms();
            var term = terms.Find(t => t.Term == translatedData.Term);
            var destinationLanguageIndex = _availableLanguages.IndexOf(DestinationLanguage);
            
            term.Languages[destinationLanguageIndex] = translatedData.TranslatedText;
            term.IsUpdated = true;
            
            translatedData.OriginalText = translatedData.TranslatedText;
            
            _sourceAssetProvider.ApplyChanges(DestinationLanguage, onCompleted);
        }
        
        public void RewertChange(TranslatedRowData translatedData)
        {
            translatedData.TranslatedText = translatedData.OriginalText;
        }

        public void AddTranslateProviderSettings(BaseTranslateProviderSettings translateProviderSettings)
        {
            translateProviderSettings.Id = Guid.NewGuid().ToString();
            _translateProviderSettingsList.Add(translateProviderSettings);

            _ = SaveTranslateProviderSettings();
        }
        
        public void RemoveTranslateProviderSettings(BaseTranslateProviderSettings translateProviderSettings)
        {
            _translateProviderSettingsList.RemoveAll(a => a.Id == translateProviderSettings.Id);
            _ = SaveTranslateProviderSettings();
        }
        
        public void SelectTranslateProviderSettings(BaseTranslateProviderSettings provider)
        {
            _context.TranslateSettings = provider;
            switch (provider.ProviderType)
            {
                case TextProviderType.None:
                    break;
                case TextProviderType.LmStudio:
                    _context.TranslateProvider = new LmStudioTranslateProvider();
                    break;
                case TextProviderType.Ollama:
                    _context.TranslateProvider = new OllamaTranslateProvider();
                    break;
                case TextProviderType.OpenAi:
                    _context.TranslateProvider = new OpenAiTranslateProvider();
                    break;
                case TextProviderType.DeepSeek:
                    _context.TranslateProvider = new LmStudioTranslateProvider();
                    break;
                case TextProviderType.GoogleAi:
                    _context.TranslateProvider = new GoogleAiTranslateProvider();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public void AddPromt(PromtSettingData promtData)
        {
            promtData.Id = Guid.NewGuid().ToString();
            _promtSettingsData.Add(promtData);
            
            _ = SavePromtSettings();
        }
        
        public void RemovePromt(PromtSettingData promtData)
        {
            _promtSettingsData.RemoveAll(a => a.Id == promtData.Id);
            _ = SavePromtSettings();
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

        private async Task SaveTranslateProviderSettings()
        {
            await FileDataManager.SaveJsonAsync(_translateProviderSettingsList, TranslateProviderSettingsFileName);
        }

        private async Task LoadTranslateProviderSettings()
        {
            if (FileDataManager.FileExists(TranslateProviderSettingsFileName))
                _translateProviderSettingsList = await FileDataManager.LoadJsonAsync<List<BaseTranslateProviderSettings>>(TranslateProviderSettingsFileName);
            else
                _translateProviderSettingsList.Clear();
        }
        
        private async Task SavePromtSettings()
        {
            await FileDataManager.SaveJsonAsync(_promtSettingsData, PromtSettingsFileName);
        }

        private async Task LoadPromtSettings()
        {
            if (FileDataManager.FileExists(PromtSettingsFileName))
                _promtSettingsData = await FileDataManager.LoadJsonAsync<List<PromtSettingData>>(PromtSettingsFileName);
            else
                _promtSettingsData.Clear();
        }

        #endregion
    }
}