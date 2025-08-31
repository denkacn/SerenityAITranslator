using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using SerenityAITranslator.Editor.Services.Translation.Models;
using SerenityAITranslator.Editor.Services.Translation.SourceAssetProvider;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SerenityAITranslator.Extension.Editor.Services.Translation.SourceAssetProvider
{
    [System.Serializable]
    public class I2SourceAssetProvider : ISourceAssetProvider
    {
        [SerializeField] public LanguageSourceAsset LanguageSourceAsset;
        
        private List<TranslatedTermsData> _translatedTermsData;
        private List<string> _languages;
        private Dictionary<string, List<TranslatedTermsData>> _translatedTermsByGroupsData;
        
        public Object GetAsset() => LanguageSourceAsset;

        public bool IsReady()
        {
            return LanguageSourceAsset != null;
        }

        public void OnDraw()
        {
            LanguageSourceAsset = (LanguageSourceAsset)EditorGUILayout.ObjectField(
                "Language Source", 
                LanguageSourceAsset, 
                typeof(LanguageSourceAsset), 
                false
            );
        }

        public List<string> GetLanguages()
        {
            if (_languages == null)
            {
                var languagesData = LanguageSourceAsset.SourceData.mLanguages;
                _languages = languagesData.Select(language => language.Name).ToList();
            }

            return _languages;
        }

        public List<TranslatedTermsData> GetTerms()
        {
            CreateCacheData();

            return _translatedTermsData;
        }

        public List<TranslatedTermsData> GetTerms(string group)
        {
            CreateCacheData();

            return _translatedTermsByGroupsData[group];
        }
        
        public List<string> GetGroups()
        {
            return LanguageSourceAsset.SourceData.GetCategories();
        }

        public void ApplyChanges(string destinationLanguage, Action<bool> onCompleted)
        {
            var terms = LanguageSourceAsset.SourceData.mTerms;
            var destinationLanguageIndex = _languages.IndexOf(destinationLanguage);
            
            foreach (var translationData in _translatedTermsData)
            {
                if (!translationData.IsUpdated) continue;
                
                terms.Find(t => t.Term == translationData.Term).Languages[destinationLanguageIndex] = translationData.Languages[destinationLanguageIndex];
                translationData.IsUpdated = false;
            }
            
            onCompleted?.Invoke(true);
        }

        private void CreateCacheData()
        {
            if (_translatedTermsData != null) return;
            
            _translatedTermsByGroupsData = new Dictionary<string, List<TranslatedTermsData>>();
            _translatedTermsData = new List<TranslatedTermsData>();
            
            var terms = LanguageSourceAsset.SourceData.mTerms;
            
            foreach (var termData in terms)
            {
                var category = LanguageSourceData.GetCategoryFromFullTerm(termData.Term);
                var translatedTermData = new TranslatedTermsData()
                {
                    Term = termData.Term,
                    Languages = termData.Languages,
                };
                
                _translatedTermsData.Add(translatedTermData);
                    
                if (_translatedTermsByGroupsData.ContainsKey(category))
                    _translatedTermsByGroupsData[category].Add(translatedTermData);
                else
                    _translatedTermsByGroupsData.Add(category, new List<TranslatedTermsData>() {translatedTermData});   
            }
        }
    }
}