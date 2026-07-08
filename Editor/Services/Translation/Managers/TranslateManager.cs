using System;
using System.Threading;
using System.Threading.Tasks;
using SerenityAITranslator.Editor.Context;
using SerenityAITranslator.Editor.Services.Common.Collections;
using SerenityAITranslator.Editor.Services.Common.PromtFactories;
using SerenityAITranslator.Editor.Services.Settings.Models;
using SerenityAITranslator.Editor.Services.Translation.AiProviders;
using SerenityAITranslator.Editor.Services.Translation.Collections;
using SerenityAITranslator.Editor.Services.Translation.Models;
using SerenityAITranslator.Editor.Services.Translation.SourceAssetProvider;
using SerenityAITranslator.Editor.Session.Models;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Translation.Managers
{
    public class TranslateManager
    {
        private const string DefaultPromt =
            "Translate fully the following into {0} and return ONLY the translations in the same format, be case-sensitive, consider line breaks, inside curly braces, with nothing else in the output:\n{1}";
        
        private readonly SerenityContext _context;
        private CancellationTokenSource _translateAllCancellationTokenSource = new CancellationTokenSource();

        private bool _isTranslateAllStarted = false;
        
        public bool IsContextSetup => _context != null;
        public bool IsTranslateProviderAndTranslateSettingSetup => _context != null && _context.SessionData.TranslationSessionData.TranslateProvider != null && _context.SessionData.TranslationSessionData.TranslateSettings != null;
        public string SelectedTranslateProviderId => _context.SessionData.TranslationSessionData.TranslateSettings != null ? _context.SessionData.TranslationSessionData.TranslateSettings.Id : string.Empty;
        public bool IsTranslateAllStarted => _isTranslateAllStarted;

        public TranslateManager(SerenityContext context)
        {
            _context = context;
            
            _context.SessionData.TranslationSessionData.BaseLanguage = "English";
            _context.SessionData.TranslationSessionData.PromtFactory = new PromtFactorySimple();
        }

        public void Setup()
        {
            var translationSessionData = _context.SessionData.TranslationSessionData;
            if (translationSessionData.SourceAssetProvider == null)
            {
                Debug.LogWarning("[SerenityAI] Translation source asset provider is not selected.");
                return;
            }
            
            translationSessionData.AvailableLanguages = translationSessionData.SourceAssetProvider.GetLanguages();
            if (translationSessionData.AvailableLanguages == null || translationSessionData.AvailableLanguages.Count == 0)
            {
                Debug.LogWarning("[SerenityAI] Translation source asset provider has no languages.");
                return;
            }
            
            if (string.IsNullOrEmpty(translationSessionData.SourceLanguage))
                translationSessionData.SourceLanguage = translationSessionData.AvailableLanguages[0];
            
            if (string.IsNullOrEmpty(translationSessionData.DestinationLanguage))
                translationSessionData.DestinationLanguage = translationSessionData.AvailableLanguages[0];

            if (string.IsNullOrEmpty(translationSessionData.SelectedPromt))
                translationSessionData.SelectedPromt = DefaultPromt;

            if (!string.IsNullOrEmpty(translationSessionData.ProviderId))
            {
                var provider = _context.TranslateProvidersConfigurations?.Providers?.Find(a => a.Id == translationSessionData.ProviderId);
                if (provider != null)
                    SelectTranslateProviderSettings(provider);
            }
        }
        
        public void SetSourceAssetProvider(ISourceAssetProvider sourceAssetProvider)
        {
            _context.SessionData.TranslationSessionData.SourceAssetProvider = sourceAssetProvider;
        }

        #region Translate
        
        public void GetTranslationTerms(string filter = null)
        {
            if (!IsTranslationSourceReady()) return;
            
            var translationSessionData = _context.SessionData.TranslationSessionData;
            var translationsData = translationSessionData.TranslationsData;
            if (translationsData == null)
            {
                translationSessionData.TranslationsData = new System.Collections.Generic.List<TranslatedRowData>();
                translationsData = translationSessionData.TranslationsData;
            }
            
            translationsData.Clear();

            var terms = translationSessionData.SourceAssetProvider.GetTerms();
            if (terms == null) return;
            if (!TryGetLanguageIndex(translationSessionData.SourceLanguage, out var sourceLanguageIndex)) return;
            if (!TryGetLanguageIndex(translationSessionData.DestinationLanguage, out var destinationLanguageIndex)) return;
            
            var index = 0;
            
            foreach (var term in terms)
            {
                if (string.IsNullOrEmpty(term.Term)) continue;
                if (filter != null && !term.Term.Contains(filter)) continue;
                if (term.Languages == null || term.Languages.Length <= Mathf.Max(sourceLanguageIndex, destinationLanguageIndex)) continue;
                
                translationsData.Add(new TranslatedRowData(index, term.Term,
                    term.Languages[sourceLanguageIndex],
                    term.Languages[destinationLanguageIndex],
                    translationSessionData.AvailableLanguages.Count));

                index++;
            }

            SaveSession();
        }
        
        public void GetTranslationTermsByGroup(string group, string filter = null)
        {
            if (!IsTranslationSourceReady()) return;
            
            var translationSessionData = _context.SessionData.TranslationSessionData;
            var translationsData = translationSessionData.TranslationsData;
            if (translationsData == null)
            {
                translationSessionData.TranslationsData = new System.Collections.Generic.List<TranslatedRowData>();
                translationsData = translationSessionData.TranslationsData;
            }
            
            translationsData.Clear();

            var terms = translationSessionData.SourceAssetProvider.GetTerms(group);
            if (terms == null) return;
            if (!TryGetLanguageIndex(translationSessionData.SourceLanguage, out var sourceLanguageIndex)) return;
            if (!TryGetLanguageIndex(translationSessionData.DestinationLanguage, out var destinationLanguageIndex)) return;
            
            var index = 0;
            
            foreach (var term in terms)
            {
                if (string.IsNullOrEmpty(term.Term)) continue;
                if (filter != null && !term.Term.Contains(filter)) continue;
                if (term.Languages == null || term.Languages.Length <= Mathf.Max(sourceLanguageIndex, destinationLanguageIndex)) continue;

                translationsData.Add(new TranslatedRowData(index, term.Term,
                    term.Languages[sourceLanguageIndex],
                    term.Languages[destinationLanguageIndex],
                    translationSessionData.AvailableLanguages.Count));

                index++;
            }
        }
        
        public void TranslateAll(Action onCompleted)
        {
            StartTranslateProcess(token => TranslateAllAsync(onCompleted, token), onCompleted);
        }
        
        public void TranslateSelected(Action onCompleted)
        {
            StartTranslateProcess(token => TranslateSelectedAsync(onCompleted, token), onCompleted);
        }
        
        public void TranslateToAllLanguages(TranslatedRowData translatedData, Action onCompleted)
        {
            StartTranslateProcess(token => TranslateOneAsyncToAllLanguages(translatedData, onCompleted, token), onCompleted);
        }

        public void TranslateOne(TranslatedRowData translatedData, Action onCompleted)
        {
            StartTranslateProcess(token => TranslateOneAsync(translatedData, onCompleted, token), onCompleted);
        }

        public void StopTranslateProcess()
        {
            _translateAllCancellationTokenSource?.Cancel();
        }
        
        #endregion

        #region Async Operations

        private async Task TranslateOneAsync(TranslatedRowData translatedData, 
            Action onCompleted,
            CancellationToken cancellationToken = default)
        {
            var translationSessionData = _context.SessionData.TranslationSessionData;
            if (!CanTranslate(translationSessionData, translatedData)) return;
            
            var promtData = new TranslatedPromtData(
                translatedData.Term, 
                translationSessionData.DestinationLanguage,
                translatedData.SourceText,
                string.IsNullOrEmpty(translationSessionData.SelectedPromt) ? DefaultPromt : translationSessionData.SelectedPromt);

            var result = await translationSessionData.TranslateProvider
                .GetTranslate(promtData, translationSessionData.TranslateSettings, translationSessionData.PromtFactory);

            if (!cancellationToken.IsCancellationRequested && result.IsNoError)
            {
                if (!TryGetLanguageIndex(translationSessionData.DestinationLanguage, out var destinationLanguageIndex)) return;
                translatedData.TranslatedText[destinationLanguageIndex] = result.Translation;
                translatedData.IsShowTranslated = true;
                onCompleted?.Invoke();
            }
        }
        
        private async Task TranslateOneAsyncToAllLanguages(TranslatedRowData translatedData, 
            Action onCompleted,
            CancellationToken cancellationToken = default)
        {
            var translationSessionData = _context.SessionData.TranslationSessionData;
            if (!CanTranslate(translationSessionData, translatedData)) return;

            foreach (var destinationLanguage in translationSessionData.AvailableLanguages)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (destinationLanguage == translationSessionData.SourceLanguage) continue;
                
                var promtData = new TranslatedPromtData(
                    translatedData.Term, 
                    destinationLanguage,
                    translatedData.SourceText,
                    string.IsNullOrEmpty(translationSessionData.SelectedPromt) ? DefaultPromt : translationSessionData.SelectedPromt);
                
                var result = await translationSessionData.TranslateProvider
                    .GetTranslate(promtData, translationSessionData.TranslateSettings, translationSessionData.PromtFactory);
                
                if (!cancellationToken.IsCancellationRequested && result.IsNoError)
                {
                    if (!TryGetLanguageIndex(destinationLanguage, out var destinationLanguageIndex)) continue;
                    translatedData.TranslatedText[destinationLanguageIndex] = result.Translation;
                }
            }
            
            translatedData.IsShowTranslated = true;
            onCompleted?.Invoke();
        }
        
        private async Task TranslateAllAsync(Action onCompleted, CancellationToken cancellationToken = default)
        {
            var translationSessionData = _context.SessionData.TranslationSessionData;
            var translationsData = translationSessionData.TranslationsData;

            foreach (var translationData in translationsData)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                await TranslateOneAsync(translationData, onCompleted, cancellationToken);
            }
        }
        
        private async Task TranslateSelectedAsync(Action onCompleted, CancellationToken cancellationToken = default)
        {
            var translationSessionData = _context.SessionData.TranslationSessionData;
            var translationsData = translationSessionData.TranslationsData;

            foreach (var translationData in translationsData)
            {
                if (!translationData.IsSelected) continue;
                
                cancellationToken.ThrowIfCancellationRequested();
                
                await TranslateOneAsync(translationData, onCompleted, cancellationToken);
            }
        }
        
        /*private async Task TranslateSelectedToAllLanguagesAsync(Action onCompleted, CancellationToken cancellationToken = default)
        {
            var translationSessionData = _context.SessionData.TranslationSessionData;
            var translationsData = translationSessionData.TranslationsData;
            
            foreach (var translationData in translationsData)
            {
                if (!translationData.IsSelected) continue;
                
                cancellationToken.ThrowIfCancellationRequested();
                
                await TranslateOneAsyncToAllLanguages(translationData, onCompleted, cancellationToken);
            }
        }*/

        #endregion

        #region Edit
        
        public void ApplyChanges(Action<bool> onCompleted)
        {
            if (!IsTranslationSourceReady())
            {
                onCompleted?.Invoke(false);
                return;
            }
            
            var translationSessionData = _context.SessionData.TranslationSessionData;
            
            var translationsData = translationSessionData.TranslationsData;
            var terms = translationSessionData.SourceAssetProvider.GetTerms();
            var destinationLanguageIndex = translationSessionData.AvailableLanguages.IndexOf(translationSessionData.DestinationLanguage);
            if (translationsData == null || terms == null || destinationLanguageIndex < 0)
            {
                onCompleted?.Invoke(false);
                return;
            }
            
            foreach (var translationData in translationsData)
            {
                if (translationData.TranslatedText == null || translationData.TranslatedText.Length <= destinationLanguageIndex) continue;
                if (string.IsNullOrEmpty(translationData.TranslatedText[destinationLanguageIndex])) continue;
                var term = terms.Find(t => t.Term == translationData.Term);
                if (term?.Languages == null || term.Languages.Length <= destinationLanguageIndex) continue;
                
                if (term.Languages[destinationLanguageIndex] != translationData.TranslatedText[destinationLanguageIndex])
                {
                    term.Languages[destinationLanguageIndex] = translationData.TranslatedText[destinationLanguageIndex];
                    term.IsUpdated = true;
                }
            }
            
            translationSessionData.SourceAssetProvider.ApplyChanges(translationSessionData.DestinationLanguage, onCompleted);
        }

        public void ApplyChange(TranslatedRowData translatedData, Action<bool> onCompleted)
        {
            if (!IsTranslationSourceReady() || translatedData == null)
            {
                onCompleted?.Invoke(false);
                return;
            }
            
            var translationSessionData = _context.SessionData.TranslationSessionData;
            var destinationLanguageIndex = translationSessionData.AvailableLanguages.IndexOf(translationSessionData.DestinationLanguage);
            
            if (destinationLanguageIndex < 0 || translatedData.TranslatedText == null ||
                translatedData.TranslatedText.Length <= destinationLanguageIndex ||
                string.IsNullOrEmpty(translatedData.TranslatedText[destinationLanguageIndex]))
            {
                onCompleted?.Invoke(false);
                return;
            }
            
            var terms = translationSessionData.SourceAssetProvider.GetTerms();
            var term = terms?.Find(t => t.Term == translatedData.Term);
            if (term?.Languages == null || term.Languages.Length <= destinationLanguageIndex)
            {
                onCompleted?.Invoke(false);
                return;
            }
            
            term.Languages[destinationLanguageIndex] = translatedData.TranslatedText[destinationLanguageIndex];
            term.IsUpdated = true;
            
            translatedData.OriginalText = translatedData.TranslatedText[destinationLanguageIndex];
            
            translationSessionData.SourceAssetProvider.ApplyChanges(translationSessionData.DestinationLanguage, onCompleted);
        }

        public void ApplyChangeToAllLanguages(TranslatedRowData translatedData, Action<bool> onCompleted)
        {
            if (!IsTranslationSourceReady() || translatedData == null)
            {
                onCompleted?.Invoke(false);
                return;
            }
            
            var translationSessionData = _context.SessionData.TranslationSessionData;
            
            foreach (var destinationLanguage in translationSessionData.AvailableLanguages)
            {
                if (destinationLanguage == translationSessionData.SourceLanguage) continue;
                
                var destinationLanguageIndex = translationSessionData.AvailableLanguages.IndexOf(destinationLanguage);
                
                if (destinationLanguageIndex < 0 || translatedData.TranslatedText == null ||
                    translatedData.TranslatedText.Length <= destinationLanguageIndex ||
                    string.IsNullOrEmpty(translatedData.TranslatedText[destinationLanguageIndex]))
                {
                    onCompleted?.Invoke(false);
                    return;
                }
                
                var terms = translationSessionData.SourceAssetProvider.GetTerms();
                var term = terms?.Find(t => t.Term == translatedData.Term);
                
                if (term?.Languages == null || term.Languages.Length <= destinationLanguageIndex) continue;
                
                if (term.Languages[destinationLanguageIndex] != translatedData.TranslatedText[destinationLanguageIndex])
                {
                    term.Languages[destinationLanguageIndex] = translatedData.TranslatedText[destinationLanguageIndex];
                    term.IsUpdated = true;
                }   
                
                translationSessionData.SourceAssetProvider.ApplyChanges(destinationLanguage, onCompleted);
            }
            
            SaveSession();
        }
        
        public void RewertChange(TranslatedRowData translatedData)
        {
            if (translatedData == null) return;
            
            var translationSessionData = _context.SessionData.TranslationSessionData;
            var destinationLanguageIndex = translationSessionData.AvailableLanguages.IndexOf(translationSessionData.DestinationLanguage);
            if (destinationLanguageIndex < 0 || translatedData.TranslatedText == null ||
                translatedData.TranslatedText.Length <= destinationLanguageIndex) return;
            
            translatedData.TranslatedText[destinationLanguageIndex] = translatedData.OriginalText;
        }
        
        #endregion

        #region Translate Provider Settings
        
        public void SelectTranslateProviderSettings(TranslateProviderConfigurationItem provider)
        {
            if (provider == null) return;
            
            var translationSessionData = _context.SessionData.TranslationSessionData;
            translationSessionData.ProviderId = provider.Id;
            translationSessionData.TranslateSettings = provider;
            
            switch (provider.ProviderType)
            {
                case TextProviderType.None:
                    break;
                case TextProviderType.LmStudio:
                    translationSessionData.TranslateProvider = new LmStudioTranslateProvider();
                    break;
                case TextProviderType.Ollama:
                    translationSessionData.TranslateProvider = new OllamaTranslateProvider();
                    break;
                case TextProviderType.OpenAi:
                    translationSessionData.TranslateProvider = new OpenAiTranslateProvider();
                    break;
                case TextProviderType.DeepSeek:
                    translationSessionData.TranslateProvider = new DeepSeekTranslateProvider();
                    break;
                case TextProviderType.Grok:
                    translationSessionData.TranslateProvider = new GrokTranslateProvider();
                    break;
                case TextProviderType.GoogleAi:
                    translationSessionData.TranslateProvider = new GoogleAiTranslateProvider();
                    break;
                case TextProviderType.GoogleTranslate:
                    translationSessionData.TranslateProvider = new GoogleTranslateProvider();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            SaveSession();
        }
        
        #endregion

        #region Promt Settings
        
        public void SelectPromt(PromtSettingsItem promtData)
        {
            if (promtData == null) return;
            
            var translationSessionData = _context.SessionData.TranslationSessionData;
            translationSessionData.SelectedPromt = promtData.Promt;

            SaveSession();
        }
        
        #endregion
        
        public string GetInfo()
        {
            if (_context == null) return string.Empty;
            var translationSessionData = _context.SessionData.TranslationSessionData;
            
            return
                $"BaseLanguage: <color=#ffc83d>{_context.SessionData.TranslationSessionData.BaseLanguage}</color>\n" +
                $"TranslateProvider: <color=#ffc83d>{_context.SessionData.TranslationSessionData.TranslateProvider?.GetType().Name}</color>\n" +
                $"PromtFactory: <color=#ffc83d>{_context.SessionData.TranslationSessionData.PromtFactory?.GetType().Name}</color>\n" +
                $"Host: <color=#ffc83d>{string.Concat(_context.SessionData.TranslationSessionData.TranslateSettings?.Host, _context.SessionData.TranslationSessionData.TranslateSettings?.Endpoint)}</color>\n" +
                $"Model: <color=#ffc83d>{_context.SessionData.TranslationSessionData.TranslateSettings?.Model}</color>\n" +
                $"Promt: <color=#ffc83d>{(string.IsNullOrEmpty(translationSessionData.SelectedPromt) ? DefaultPromt : translationSessionData.SelectedPromt)}</color>";
        }
        
        private void SaveSession()
        {
            _context.Save();
        }

        public void SelectAllTerms()
        {
            var translationsData = _context.SessionData.TranslationSessionData.TranslationsData;
            if (translationsData == null) return;
            
            foreach (var translationData in translationsData)
            {
                translationData.IsSelected = true;
            }
            SaveSession();
        }
        
        public void DeselectAllTerms()
        {
            var translationsData = _context.SessionData.TranslationSessionData.TranslationsData;
            if (translationsData == null) return;
            
            foreach (var translationData in translationsData)
            {
                translationData.IsSelected = false;
            }
            SaveSession();  
        }

        private void StartTranslateProcess(Func<CancellationToken, Task> operation, Action onCompleted)
        {
            if (_isTranslateAllStarted) return;
            
            _isTranslateAllStarted = true;
            _translateAllCancellationTokenSource?.Dispose();
            _translateAllCancellationTokenSource = new CancellationTokenSource();
            
            _ = RunTranslateProcessAsync(operation, onCompleted, _translateAllCancellationTokenSource.Token);
        }
        
        private async Task RunTranslateProcessAsync(Func<CancellationToken, Task> operation, Action onCompleted, CancellationToken cancellationToken)
        {
            try
            {
                await operation(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[SerenityAI] Translation process canceled.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SerenityAI] Translation process failed: {ex}");
            }
            finally
            {
                _isTranslateAllStarted = false;
                onCompleted?.Invoke();
            }
        }
        
        private bool IsTranslationSourceReady()
        {
            var translationSessionData = _context?.SessionData?.TranslationSessionData;
            if (translationSessionData?.SourceAssetProvider == null)
            {
                Debug.LogWarning("[SerenityAI] Translation source asset provider is not selected.");
                return false;
            }
            
            if (translationSessionData.AvailableLanguages == null || translationSessionData.AvailableLanguages.Count == 0)
            {
                translationSessionData.AvailableLanguages = translationSessionData.SourceAssetProvider.GetLanguages();
            }
            
            if (translationSessionData.AvailableLanguages == null || translationSessionData.AvailableLanguages.Count == 0)
            {
                Debug.LogWarning("[SerenityAI] Translation source asset provider has no languages.");
                return false;
            }
            
            return true;
        }
        
        private bool TryGetLanguageIndex(string language, out int index)
        {
            var languages = _context?.SessionData?.TranslationSessionData?.AvailableLanguages;
            index = languages?.IndexOf(language) ?? -1;
            if (index >= 0) return true;
            
            Debug.LogWarning($"[SerenityAI] Language is not available: {language}");
            return false;
        }
        
        private bool CanTranslate(TranslationSessionData translationSessionData, TranslatedRowData translatedData)
        {
            if (translationSessionData == null || translatedData == null) return false;
            if (translationSessionData.TranslateProvider == null || translationSessionData.TranslateSettings == null)
            {
                Debug.LogWarning("[SerenityAI] Translation provider is not configured.");
                return false;
            }
            
            if (translationSessionData.PromtFactory == null)
                translationSessionData.PromtFactory = new PromtFactorySimple();
            
            return IsTranslationSourceReady();
        }
        
    }
}
