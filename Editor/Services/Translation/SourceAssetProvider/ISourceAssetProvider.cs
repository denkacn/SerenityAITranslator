using System.Collections.Generic;
using SerenityAITranslator.Editor.Services.Translation.Models;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Translation.SourceAssetProvider
{
    public interface ISourceAssetProvider
    {
        void OnDraw();
        List<string> GetLanguages();
        List<TranslatedTermsData> GetTerms();
        Object GetAsset();
    }
}