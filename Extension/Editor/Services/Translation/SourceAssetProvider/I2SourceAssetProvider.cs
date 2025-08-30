using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using SerenityAITranslator.Editor.Services.Translation.Models;
using SerenityAITranslator.Editor.Services.Translation.SourceAssetProvider;
using UnityEditor;
using UnityEngine;

namespace SerenityAITranslator.Extension.Editor.Services.Translation.SourceAssetProvider
{
    [System.Serializable]
    public class I2SourceAssetProvider : ISourceAssetProvider
    {
        [SerializeField] public LanguageSourceAsset LanguageSourceAsset;
        
        private List<TranslatedTermsData> _translatedTermsData;
        private List<string> _languages;
        
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
            if (_translatedTermsData == null)
            {
                var terms = LanguageSourceAsset.SourceData.mTerms;
                var translatedTermsData = new List<TranslatedTermsData>();
                foreach (var termData in terms)
                {
                    var translatedTermData = new TranslatedTermsData()
                    {
                        Term = termData.Term,
                        Languages = termData.Languages,
                    };
                
                    translatedTermsData.Add(translatedTermData);
                }

                _translatedTermsData = translatedTermsData;
            }

            return _translatedTermsData;
        }
        
        public Object GetAsset() => LanguageSourceAsset;
    }
}