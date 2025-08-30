using I2.Loc;
using SerenityAITranslator.Editor.Services.Translation.Managers;
using SerenityAITranslator.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Translation.Views
{
    public class TranslateSettingsButtonView : BaseExtensionView
    {
        private readonly I2LocAiTranslateExtensionManager _translateExtensionManager;
        private readonly LanguageSourceAsset _languageSourceAsset;
        private string _filter;

        public TranslateSettingsButtonView(EditorWindow owner, I2LocAiTranslateExtensionManager translateExtensionManager, LanguageSourceAsset languageSourceAsset) : base(owner)
        {
            _translateExtensionManager = translateExtensionManager;
            _languageSourceAsset = languageSourceAsset;
        }

        public void Draw()
        {
            GUILayout.BeginVertical(UiStyles.OddRowStyle);
            EditorGUILayout.LabelField("Translation Controls", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            
            var optionsArray = _translateExtensionManager.GetAvailableLanguages().ToArray();

            if (optionsArray.Length > 0)
            {
                var originalLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = UiTools.GetLabelWidth("Source"); 
                var selectedSourceLanguageIndex = UiTools.GetIndexForValue(_translateExtensionManager.SourceLanguage, optionsArray);
                selectedSourceLanguageIndex = EditorGUILayout.Popup("Source", selectedSourceLanguageIndex, optionsArray,GUILayout.Width(300));
                _translateExtensionManager.SourceLanguage = optionsArray[selectedSourceLanguageIndex];
                
                GUILayout.Space(20);
            
                EditorGUIUtility.labelWidth = UiTools.GetLabelWidth("Destination"); 
                var selectedDestinationLanguageIndex = UiTools.GetIndexForValue(_translateExtensionManager.DestinationLanguage, optionsArray);
                selectedDestinationLanguageIndex = EditorGUILayout.Popup("Destination", selectedDestinationLanguageIndex, optionsArray, GUILayout.Width(300));
                _translateExtensionManager.DestinationLanguage = optionsArray[selectedDestinationLanguageIndex];
                
                EditorGUIUtility.labelWidth = originalLabelWidth;
                
                GUILayout.Space(20);
            
                if (GUILayout.Button("Display Terms"))
                {
                    _translateExtensionManager.GetTranslationTerms();
                }

                if (_translateExtensionManager.IsTranslateProviderAndTranslateSettingSetup)
                {
                    if (_translateExtensionManager.IsTranslateAllStarted)
                    {
                        if (GUILayout.Button("Stop Translate Process"))
                        {
                            _translateExtensionManager.StopTranslateProcess();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Translate All"))
                        {
                            var result = UiTools.DisplayTranslateAllDialog();

                            if (result) _translateExtensionManager.TranslateAll(Repaint);
                        }
                    }
                    
                    if (GUILayout.Button("Apply Changes"))
                    {
                        var result = UiTools.DisplayApplyChangesDialog();

                        if (result)
                        {
                            if (_translateExtensionManager.ApplyChanges())
                            {
                                EditorUtility.SetDirty(_languageSourceAsset);
                                AssetDatabase.SaveAssets();
                            }
                        }
                    }
                }
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            
            _filter = EditorGUILayout.TextField("Filter", _filter,GUILayout.Width(400));
            if (GUILayout.Button("Apply Filter"))
            {
                _translateExtensionManager.GetTranslationTerms(_filter);
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.EndVertical();
        }
    }
}