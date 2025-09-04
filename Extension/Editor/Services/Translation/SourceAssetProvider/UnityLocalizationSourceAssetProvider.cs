using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SerenityAITranslator.Editor.Services.Translation.Models;
using SerenityAITranslator.Editor.Services.Translation.SourceAssetProvider;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Settings;
using Object = UnityEngine.Object;

namespace SerenityAITranslator.Extension.Editor.Services.Translation.SourceAssetProvider
{
    [CreateAssetMenu(menuName = "SerenityAI/Providers/UnityLocalizationSourceAssetProvider")]
    public class UnityLocalizationSourceAssetProvider : ScriptableObject, ISourceAssetProvider
    {
        [SerializeField] public string TableName = "Test";
        [SerializeField] public List<TranslatedTermsData> TranslatedTermsData;
        [SerializeField] public List<string> Languages;
        
        private bool _isReady;
        
        public bool IsReady()
        {
            return _isReady;
        }

        public void OnDraw()
        {
            GUILayout.Label($"Is Provider Ready: {_isReady}");
            
            if (GUILayout.Button("Init Provider"))
            {
                _ = InitProviderAsync().ConfigureAwait(false);
            }
        }

        public List<string> GetLanguages()
        {
            var locales = LocalizationSettings.AvailableLocales.Locales;
            Languages = locales.ConvertAll(x => x.LocaleName);
            return Languages;
        }

        public List<TranslatedTermsData> GetTerms()
        {
            return TranslatedTermsData;
        }

        public List<TranslatedTermsData> GetTerms(string group)
        {
            return TranslatedTermsData;
        }

        public Object GetAsset()
        {
            return LocalizationSettings.Instance;
        }

        public List<string> GetGroups()
        {
            return new List<string>();
        }

        public void ApplyChanges(string destinationLanguage, Action<bool> onCompleted)
        {
            ApplyChangeAsync(destinationLanguage, onCompleted).ConfigureAwait(false);
        }
        
        private async Task InitProviderAsync()
        {
            var init = LocalizationSettings.InitializationOperation;
            
            await init.Task;
            await GetAllStringsForAllLocales(TableName);

            _isReady = true;
        }

        private async Task ApplyChangeAsync(string destinationLanguage, Action<bool> onCompleted)
        {
            var locale = LocalizationSettings.AvailableLocales.Locales.Find(l=>l.LocaleName == destinationLanguage);
            var tableOp = LocalizationSettings.StringDatabase.GetTableAsync(TableName, locale);
            var table = await tableOp.Task;
            var destinationLanguageIndex = Languages.IndexOf(destinationLanguage);
            
            foreach (var termsData in TranslatedTermsData)
            {
                if (!termsData.IsUpdated) continue;
                
                var entry = table.GetEntry(termsData.Term);
                var value = termsData.Languages[destinationLanguageIndex];
                
                if (entry == null)
                {
                    table.AddEntry(termsData.Term, value);
                }
                else
                {
                    entry.Value = value;
                }
                
                termsData.IsUpdated = false;
            }
            
            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();
            
            onCompleted?.Invoke(true);
        }

        private async Task GetAllStringsForAllLocales(string tableName)
        {
            TranslatedTermsData = new List<TranslatedTermsData>();
            
            var translationsMap = new Dictionary<string, List<string>>();
            var locales = LocalizationSettings.AvailableLocales.Locales;

            foreach (var locale in locales)
            {
                Debug.Log(locale.LocaleName);

                var tableLoading = LocalizationSettings.StringDatabase.GetTableAsync(tableName, locale);
                var stringTable = await tableLoading.Task;
                
                if (stringTable == null) continue;

                var localeIndex = locales.IndexOf(locale);

                foreach (var entry in stringTable.Values)
                {
                    if (!translationsMap.TryGetValue(entry.Key, out var translations))
                    {
                        translations = CreateEmptyTranslations(locales.Count);
                        translationsMap.Add(entry.Key, translations);
                    }
                    translations[localeIndex] = entry.LocalizedValue;
                }
            }

            foreach (var kvp in translationsMap)
            {
                TranslatedTermsData.Add(new TranslatedTermsData
                {
                    Term = kvp.Key,
                    Languages = kvp.Value.ToArray()
                });
            }
        }
        
        private List<string> CreateEmptyTranslations(int count)
        {
            var list = new List<string>(count);
            for (var i = 0; i < count; i++)
                list.Add(string.Empty);
            return list;
        }
    }
}
