using System;
using System.Collections.Generic;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Tts.Collections
{
    public class VoicesCollection : ScriptableObject
    {
        public List<VoicePack> Voices = new List<VoicePack>();

        public void Add(VoiceItem voiceItem)
        {
            if (voiceItem == null || string.IsNullOrEmpty(voiceItem.Term)) return;
            if (Voices == null) Voices = new List<VoicePack>();

            var pack = Voices.Find(p => p.Term == voiceItem.Term);
            if (pack == null)
            {
                Voices.Add(new VoicePack()
                {
                    Term = voiceItem.Term,
                    VoiceItems = new List<VoiceItem>() { voiceItem }
                });
            }
            else
            {
                var voiceItemInPack = pack.VoiceItems.Find(v => v.Language == voiceItem.Language);
                if (voiceItemInPack != null)
                {
                    voiceItemInPack.VoiceClip = voiceItem.VoiceClip;
                    voiceItemInPack.TtsInfo = voiceItem.TtsInfo;
                }
                else
                {
                    pack.VoiceItems.Add(voiceItem);
                }
            }
        }

        public VoiceItem Get(string term, string language)
        {
            if (string.IsNullOrEmpty(term) || string.IsNullOrEmpty(language)) return null;
            if (Voices == null) return null;
            
            var pack = Voices.Find(p => p.Term == term);
            return pack?.VoiceItems.Find(v => v.Language == language);
        }

        public bool IsExist(string term, string language)
        {
            if (string.IsNullOrEmpty(term) || string.IsNullOrEmpty(language)) return false;
            if (Voices == null) return false;
            
            var pack = Voices.Find(p => p.Term == term);
            return pack?.VoiceItems.Find(v => v.Language == language) != null;
        }

        public List<string> GetLanguagesForTerms(string term, LanguageConverterData languageConverter)
        {
            if (Voices == null) return new List<string>();
            
            var pack = Voices.Find(p => p.Term == term);
            var languages = new List<string>();
            if (pack != null)
            {
                foreach (var voiceItem in pack.VoiceItems)
                {
                    if (languageConverter != null && voiceItem != null)
                        languages.Add(languageConverter.ConvertLanguageName(voiceItem.Language));
                }
            }

            return languages;
        }

        public void Remove(string term, string language)
        {
            if (string.IsNullOrEmpty(term) || string.IsNullOrEmpty(language)) return;
            if (Voices == null) return;
            
            var pack = Voices.Find(p => p.Term == term);
            if (pack == null) return;
            if (pack.VoiceItems == null) return;
            
            var voiceItem = pack.VoiceItems.Find(v => v.Language == language);
            if (voiceItem == null) return;
            
            if (voiceItem.VoiceClip != null)
                DestroyImmediate(voiceItem.VoiceClip, true);
            
            pack.VoiceItems.RemoveAll(l => l.Language == language);
        }
    }
    
    [Serializable]
    public class VoicePack
    {
        public string Term;
        public List<VoiceItem> VoiceItems = new List<VoiceItem>();
    }

    [Serializable]
    public class VoiceItem
    {
        public string Id;
        public string Term;
        public string Language;
        public string TtsInfo;
        public AudioClip VoiceClip;
    }
}
