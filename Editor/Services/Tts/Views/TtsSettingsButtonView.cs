using SerenityAITranslator.Editor.Context;
using SerenityAITranslator.Editor.Services.Common.Views;
using SerenityAITranslator.Editor.Services.Translation.SourceAssetProvider;
using SerenityAITranslator.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Tts.Views
{
    public class TtsSettingsButtonView : BaseView
    {
        private readonly ISourceAssetProvider _sourceAssetProvider;
        
        private string _filter;
        private float _originalLabelWidth;

        public TtsSettingsButtonView(EditorWindow owner, ISourceAssetProvider sourceAssetProvider, SerenityContext context) : base(owner, context)
        {
            _sourceAssetProvider = sourceAssetProvider;
        }

        public override void Draw()
        {
            GUILayout.BeginHorizontal(UiStyles.OddRowStyle);
            EditorGUILayout.LabelField("Voice Controls", UiStyles.LabelStyleCenter);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginVertical();
            
            GUILayout.BeginHorizontal();
            
            var translationSessionData = _context.SessionData.TranslationSessionData;
            var optionsArray = translationSessionData.AvailableLanguages.ToArray();

            if (optionsArray.Length > 0)
            {
                _originalLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = UiTools.GetLabelWidth("Language");
                
                var selectedSourceLanguageIndex = UiTools.GetIndexForValue(translationSessionData.SourceLanguage, optionsArray);
                selectedSourceLanguageIndex = EditorGUILayout.Popup("Language", selectedSourceLanguageIndex, optionsArray,GUILayout.Width(300));
                translationSessionData.SourceLanguage = optionsArray[selectedSourceLanguageIndex];
                
                EditorGUIUtility.labelWidth = _originalLabelWidth;
                
                GUILayout.Space(20);
            
                if (GUILayout.Button("Display Terms"))
                {
                    _context.TranslateManager.GetTranslationTerms();
                }
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            
            EditorGUIUtility.labelWidth = UiTools.GetLabelWidth("Filter"); 
            
            _filter = EditorGUILayout.TextField("Filter", _filter,GUILayout.Width(400));
            
            EditorGUIUtility.labelWidth = _originalLabelWidth;
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Apply Filter"))
            {
                _context.TranslateManager.GetTranslationTerms(_filter);
            }
            
            GUILayout.Space(20);
            
            GUILayout.Label("Groups:");
            
            if (GUILayout.Button("All"))
            {
                _context.TranslateManager.GetTranslationTerms();
            }

            GUILayout.Space(5);
            
            var groups = _sourceAssetProvider.GetGroups();
            foreach (var group in groups)
            {
                if (GUILayout.Button(group))
                {
                    _context.TranslateManager.GetTranslationTermsByGroup(group);
                }
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            GUILayout.EndVertical();
        }
    }
}