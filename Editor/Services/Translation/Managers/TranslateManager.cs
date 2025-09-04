using System;
using System.Threading;
using System.Threading.Tasks;
using SerenityAITranslator.Editor.Context;
using SerenityAITranslator.Editor.Services.Common.Collections;
using SerenityAITranslator.Editor.Services.Common.PromtFactories;
using SerenityAITranslator.Editor.Services.Translation.AiProviders;
using SerenityAITranslator.Editor.Services.Translation.Collections;
using SerenityAITranslator.Editor.Services.Translation.Models;
using SerenityAITranslator.Editor.Services.Translation.SourceAssetProvider;

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
            translationSessionData.AvailableLanguages = translationSessionData.SourceAssetProvider.GetLanguages();
            
            if (string.IsNullOrEmpty(translationSessionData.SourceLanguage))
                translationSessionData.SourceLanguage = translationSessionData.AvailableLanguages[0];
            
            if (string.IsNullOrEmpty(translationSessionData.DestinationLanguage))
                translationSessionData.DestinationLanguage = translationSessionData.AvailableLanguages[0];

            if (string.IsNullOrEmpty(translationSessionData.SelectedPromt))
                translationSessionData.SelectedPromt = DefaultPromt;

            if (!string.IsNullOrEmpty(translationSessionData.ProviderId))
            {
                var provider = _context.TranslateProvidersConfigurations.Providers.Find(a => a.Id == translationSessionData.ProviderId);
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
            var translationSessionData = _context.SessionData.TranslationSessionData;
            var translationsData = translationSessionData.TranslationsData;
            translationsData.Clear();

            var terms = translationSessionData.SourceAssetProvider.GetTerms();
            
            var index = 0;
            
            foreach (var term in terms)
            {
                if (string.IsNullOrEmpty(term.Term)) continue;
                if (filter != null && !term.Term.Contains(filter)) continue;
                
                translationsData.Add(new TranslatedRowData(index, term.Term,
                    term.Languages[translationSessionData.AvailableLanguages.IndexOf(translationSessionData.SourceLanguage)],
                    term.Languages[translationSessionData.AvailableLanguages.IndexOf(translationSessionData.DestinationLanguage)]));

                index++;
            }

            SaveSession();
        }
        
        public void GetTranslationTermsByGroup(string group, string filter = null)
        {
            var translationSessionData = _context.SessionData.TranslationSessionData;
            var translationsData = translationSessionData.TranslationsData;
            
            translationsData.Clear();

            var terms = translationSessionData.SourceAssetProvider.GetTerms(group);
            var index = 0;
            
            foreach (var term in terms)
            {
                if (string.IsNullOrEmpty(term.Term)) continue;
                if (filter != null && !term.Term.Contains(filter)) continue;

                translationsData.Add(new TranslatedRowData(index, term.Term,
                    term.Languages[translationSessionData.AvailableLanguages.IndexOf(translationSessionData.SourceLanguage)],
                    term.Languages[translationSessionData.AvailableLanguages.IndexOf(translationSessionData.DestinationLanguage)]));

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
        
        #endregion

        #region Async Operations

        private async Task TranslateOneAsync(TranslatedRowData translatedData, 
            Action onCompleted,
            CancellationToken cancellationToken = default)
        {
            var translationSessionData = _context.SessionData.TranslationSessionData;
            
            var promtData = new TranslatedPromtData(
                translatedData.Term, 
                translationSessionData.DestinationLanguage,
                translatedData.SourceText,
                string.IsNullOrEmpty(translationSessionData.SelectedPromt) ? DefaultPromt : translationSessionData.SelectedPromt);

            var result = await translationSessionData.TranslateProvider
                .GetTranslate(promtData, translationSessionData.TranslateSettings, translationSessionData.PromtFactory);

            if (!cancellationToken.IsCancellationRequested && result.IsNoError)
            {
                translatedData.TranslatedText = result.Translation;
                translatedData.IsShowTranslated = true;
                onCompleted?.Invoke();
            }
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
            
            _isTranslateAllStarted = false;
        }

        #endregion

        #region Edit
        
        public void ApplyChanges(Action<bool> onCompleted)
        {
            var translationSessionData = _context.SessionData.TranslationSessionData;
            
            var translationsData = translationSessionData.TranslationsData;
            var terms = translationSessionData.SourceAssetProvider.GetTerms();
            var destinationLanguageIndex = translationSessionData.AvailableLanguages.IndexOf(translationSessionData.DestinationLanguage);
            
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
            
            translationSessionData.SourceAssetProvider.ApplyChanges(translationSessionData.DestinationLanguage, onCompleted);
        }

        public void ApplyChange(TranslatedRowData translatedData, Action<bool> onCompleted)
        {
            if (string.IsNullOrEmpty(translatedData.TranslatedText))
            {
                onCompleted?.Invoke(false);
                return;
            }
            
            var translationSessionData = _context.SessionData.TranslationSessionData;
            var terms = translationSessionData.SourceAssetProvider.GetTerms();
            var term = terms.Find(t => t.Term == translatedData.Term);
            var destinationLanguageIndex = translationSessionData.AvailableLanguages.IndexOf(translationSessionData.DestinationLanguage);
            
            term.Languages[destinationLanguageIndex] = translatedData.TranslatedText;
            term.IsUpdated = true;
            
            translatedData.OriginalText = translatedData.TranslatedText;
            
            translationSessionData.SourceAssetProvider.ApplyChanges(translationSessionData.DestinationLanguage, onCompleted);
        }
        
        public void RewertChange(TranslatedRowData translatedData)
        {
            translatedData.TranslatedText = translatedData.OriginalText;
        }
        
        #endregion

        #region Translate Provider Settings
        
        public void SelectTranslateProviderSettings(TranslateProviderConfigurationItem provider)
        {
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
                    translationSessionData.TranslateProvider = new LmStudioTranslateProvider();
                    break;
                case TextProviderType.GoogleAi:
                    translationSessionData.TranslateProvider = new GoogleAiTranslateProvider();
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
                $"BaseLanguage: {_context.SessionData.TranslationSessionData.BaseLanguage}\n" +
                $"TranslateProvider: {_context.SessionData.TranslationSessionData.TranslateProvider?.GetType().Name}\n" +
                $"PromtFactory: {_context.SessionData.TranslationSessionData.PromtFactory.GetType().Name}\n" +
                $"Host: {string.Concat(_context.SessionData.TranslationSessionData.TranslateSettings?.Host, _context.SessionData.TranslationSessionData.TranslateSettings?.Endpoint)}\n" +
                $"Model: {_context.SessionData.TranslationSessionData.TranslateSettings?.Model}\n" +
                $"Promt: {(string.IsNullOrEmpty(translationSessionData.SelectedPromt) ? DefaultPromt : translationSessionData.SelectedPromt)}";
        }
        
        public void SaveSession()
        {
            _context.Save();
        }
    }
}