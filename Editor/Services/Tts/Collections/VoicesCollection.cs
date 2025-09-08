using System;
using System.Collections.Generic;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Tts.Collections
{
    public class VoicesCollection : ScriptableObject
    {
        public Dictionary<string, List<VoiceItem>> Voices;
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