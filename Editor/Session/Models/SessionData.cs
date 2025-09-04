using System;
using SerenityAITranslator.Editor.Services.Common.Enums;
using UnityEngine;

namespace SerenityAITranslator.Editor.Session.Models
{
    [Serializable]
    public class SessionData : ScriptableObject
    {
        public SerenityServiceType ServiceType = SerenityServiceType.None;
        public TranslationSessionData TranslationSessionData = new TranslationSessionData();
    }
}