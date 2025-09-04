using System;
using System.Collections.Generic;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Common.Collections
{
    public class PromtSettingsCollection : ScriptableObject
    {
        public List<PromtSettingsItem> Settings;
    }

    [Serializable]
    public class PromtSettingsItem
    {
        public string Id;
        public string Name;
        public string Promt;
    }
}