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
            if (string.IsNullOrEmpty(voiceItem.Term)) return;

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
                pack.VoiceItems.Add(voiceItem);
            }
        }

        public VoiceItem Get(string term, string language)
        {
            var pack = Voices.Find(p => p.Term == term);
            return pack?.VoiceItems.Find(v => v.Language == language);
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