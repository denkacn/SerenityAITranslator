using System;
using System.Collections.Generic;
using SerenityAITranslator.Editor.Services.Translation.Models;
using Object = UnityEngine.Object;

namespace SerenityAITranslator.Editor.Services.Translation.SourceAssetProvider
{
    public interface ISourceAssetProvider
    {
        bool IsReady();
        void OnDraw();
        List<string> GetLanguages();
        List<TranslatedTermsData> GetTerms();
        List<TranslatedTermsData> GetTerms(string group);
        Object GetAsset();
        List<string> GetGroups();
        void ApplyChanges(string destinationLanguage, Action<bool> onCompleted);
    }
}