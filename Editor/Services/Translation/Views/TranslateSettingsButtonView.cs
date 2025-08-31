using SerenityAITranslator.Editor.Services.Common.Views;
using SerenityAITranslator.Editor.Services.Translation.Managers;
using SerenityAITranslator.Editor.Services.Translation.SourceAssetProvider;
using SerenityAITranslator.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Translation.Views
{
    public class TranslateSettingsButtonView : BaseView
    {
        private readonly TranslateManager _translateManager;
        private readonly ISourceAssetProvider _sourceAssetProvider;
        private string _filter;
        private float _originalLabelWidth;

        public TranslateSettingsButtonView(EditorWindow owner, TranslateManager translateManager, ISourceAssetProvider sourceAssetProvider) : base(owner)
        {
            _translateManager = translateManager;
            _sourceAssetProvider = sourceAssetProvider;
        }

        public override void Draw()
        {
            GUILayout.BeginHorizontal(UiStyles.OddRowStyle);
            EditorGUILayout.LabelField("Translation Controls", UiStyles.LabelStyleCenter);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginVertical();
            
            GUILayout.BeginHorizontal();
            
            var optionsArray = _translateManager.GetAvailableLanguages().ToArray();

            if (optionsArray.Length > 0)
            {
                _originalLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = UiTools.GetLabelWidth("Source"); 
                var selectedSourceLanguageIndex = UiTools.GetIndexForValue(_translateManager.SourceLanguage, optionsArray);
                selectedSourceLanguageIndex = EditorGUILayout.Popup("Source", selectedSourceLanguageIndex, optionsArray,GUILayout.Width(300));
                _translateManager.SourceLanguage = optionsArray[selectedSourceLanguageIndex];
                
                GUILayout.Space(20);
            
                EditorGUIUtility.labelWidth = UiTools.GetLabelWidth("Destination"); 
                var selectedDestinationLanguageIndex = UiTools.GetIndexForValue(_translateManager.DestinationLanguage, optionsArray);
                selectedDestinationLanguageIndex = EditorGUILayout.Popup("Destination", selectedDestinationLanguageIndex, optionsArray, GUILayout.Width(300));
                _translateManager.DestinationLanguage = optionsArray[selectedDestinationLanguageIndex];
                
                EditorGUIUtility.labelWidth = _originalLabelWidth;
                
                GUILayout.Space(20);
            
                if (GUILayout.Button("Display Terms"))
                {
                    _translateManager.GetTranslationTerms();
                }

                if (_translateManager.IsTranslateProviderAndTranslateSettingSetup)
                {
                    if (_translateManager.IsTranslateAllStarted)
                    {
                        if (GUILayout.Button("Stop Translate Process"))
                        {
                            _translateManager.StopTranslateProcess();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Translate All"))
                        {
                            var result = UiTools.DisplayTranslateAllDialog();

                            if (result) _translateManager.TranslateAll(Repaint);
                        }
                    }
                    
                    if (GUILayout.Button("Apply Changes"))
                    {
                        var result = UiTools.DisplayApplyChangesDialog();

                        if (result)
                        {
                            _translateManager.ApplyChanges(isOk =>
                            {
                                EditorUtility.SetDirty(_sourceAssetProvider.GetAsset());
                                
                                AssetDatabase.SaveAssets();
                            });
                        }
                    }
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
                _translateManager.GetTranslationTerms(_filter);
            }
            
            GUILayout.Space(20);
            
            GUILayout.Label("Groups:");
            
            if (GUILayout.Button("All"))
            {
                _translateManager.GetTranslationTerms();
            }

            GUILayout.Space(5);
            
            var groups = _sourceAssetProvider.GetGroups();
            foreach (var group in groups)
            {
                if (GUILayout.Button(group))
                {
                    _translateManager.GetTranslationTermsByGroup(group);
                }
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            GUILayout.EndVertical();
        }
    }
}