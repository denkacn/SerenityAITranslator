using System;
using System.Collections.Generic;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Tts.Collections
{
    public class LanguageConverterData : ScriptableObject
    {
        [SerializeField] private List<LanguageMapping> LanguageMappings = new List<LanguageMapping>();
        
        private Dictionary<string, string> _mappingCache;
        
        private void OnEnable()
        {
            if (LanguageMappings == null || LanguageMappings.Count == 0)
            {
                InitializeDefaultMappings();
            }
        }

        public string ConvertLanguageName(string languageName)
        {
            if (string.IsNullOrWhiteSpace(languageName))
                return null;
            
            if (_mappingCache == null)
            {
                InitializeCache();
            }
            
            if (_mappingCache.TryGetValue(languageName.Trim(), out var code)) return code;
            
            foreach (var kvp in _mappingCache)
            {
                if (string.Equals(kvp.Key, languageName.Trim(), System.StringComparison.OrdinalIgnoreCase))
                    return kvp.Value;
            }

            return string.Empty;
        }
        
        private void InitializeCache()
        {
            _mappingCache = new Dictionary<string, string>();
            foreach (var mapping in LanguageMappings)
            {
                _mappingCache.Add(mapping.FullName, mapping.Code);
            }
        }
        
        private void InitializeDefaultMappings()
        {
            LanguageMappings = new List<LanguageMapping>
            {
                new LanguageMapping { FullName = "english", Code = "en" },
                new LanguageMapping { FullName = "spanish", Code = "es" },
                new LanguageMapping { FullName = "french", Code = "fr" },
                new LanguageMapping { FullName = "german", Code = "de" },
                new LanguageMapping { FullName = "italian", Code = "it" },
                new LanguageMapping { FullName = "portuguese", Code = "pt" },
                new LanguageMapping { FullName = "polish", Code = "pl" },
                new LanguageMapping { FullName = "turkish", Code = "tr" },
                new LanguageMapping { FullName = "russian", Code = "ru" },
                new LanguageMapping { FullName = "dutch", Code = "nl" },
                new LanguageMapping { FullName = "czech", Code = "cs" },
                new LanguageMapping { FullName = "arabic", Code = "ar" },
                new LanguageMapping { FullName = "chinese (simplified)", Code = "zh-cn" },
                new LanguageMapping { FullName = "hungarian", Code = "hu" },
                new LanguageMapping { FullName = "korean", Code = "ko" },
                new LanguageMapping { FullName = "japanese", Code = "ja" },
                new LanguageMapping { FullName = "hindi", Code = "hi" }
            };
        }
        
        [Serializable]
        public class LanguageMapping
        {
            public string FullName;
            public string Code;
        }
    }
}