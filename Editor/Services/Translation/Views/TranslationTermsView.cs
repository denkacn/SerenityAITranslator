using I2.Loc;
using SerenityAITranslator.Editor.Services.Translation.Managers;
using SerenityAITranslator.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Translation.Views
{
    public class TranslationTermsView : BaseExtensionView
    {
        private readonly I2LocAiTranslateExtensionManager _translateExtensionManager;
        private readonly LanguageSourceAsset _languageSourceAsset;
        private Vector2 _scrollPosition;
        private int _editRowId = -1;
        
        public TranslationTermsView(EditorWindow owner, I2LocAiTranslateExtensionManager translateExtensionManager, LanguageSourceAsset languageSourceAsset) : base(owner)
        {
            _translateExtensionManager = translateExtensionManager;
            _languageSourceAsset = languageSourceAsset;
        }
        
        public void Draw()
        {
            if (_translateExtensionManager.GetTranslationsData() == null) return;
            
            var rows = _translateExtensionManager.GetTranslationsData();
            
            EditorGUILayout.LabelField("Translation Table", EditorStyles.boldLabel);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal(UiStyles.OddRowStyle);
            //EditorGUILayout.LabelField("Id", UiStyles.LabelHeaderStyle, GUILayout.Width(40));
            EditorGUILayout.LabelField("Terms", UiStyles.LabelHeaderStyle, GUILayout.Width(215));
            EditorGUILayout.LabelField("Base Text", UiStyles.LabelHeaderStyle, GUILayout.Width(415));
            EditorGUILayout.LabelField("Translation", UiStyles.LabelHeaderStyle, GUILayout.Width(400));
            EditorGUILayout.LabelField("Translate", UiStyles.LabelHeaderStyle, GUILayout.Width(100));
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            for (var i = 0; i < rows.Count; i++)
            {
                var rowStyle = (i % 2 == 0) ? UiStyles.EvenRowStyle : UiStyles.OddRowStyle;
                var row = rows[i];
                
                var translatedText = row.IsShowTranslated ? row.TranslatedText : row.OriginalText;

                EditorGUILayout.BeginHorizontal(rowStyle);
                
                //EditorGUILayout.LabelField(row.Id.ToString(), GUILayout.Width(40));
                EditorGUILayout.LabelField(row.Term, UiStyles.LabelRowStyle,GUILayout.Width(200));
                EditorGUILayout.LabelField(row.SourceText, UiStyles.LabelRowStyle, GUILayout.Width(400), GUILayout.MinHeight(30), GUILayout.MaxHeight(800));

                var height = UiStyles.LabelRowStyle.CalcHeight(new GUIContent(translatedText), 400);
                
                if (row.IsShowTranslated)
                {
                    if (_editRowId == row.Id)
                    {
                        row.TranslatedText = EditorGUILayout.TextArea(row.TranslatedText, GUILayout.Width(415));
                        Repaint();
                    }
                    else
                    {
                        EditorGUILayout.LabelField(translatedText, UiStyles.LabelRowStyleGreen, GUILayout.Width(400), GUILayout.Height(height));
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(translatedText, UiStyles.LabelRowStyle, GUILayout.Width(400), GUILayout.Height(height));
                }
                
                if (_translateExtensionManager.IsTranslateProviderAndTranslateSettingSetup)
                {
                    if (GUILayout.Button("Translate", GUILayout.Width(80)))
                    {
                        _translateExtensionManager.TranslateOne(row, Repaint);
                        GUI.FocusControl(null);
                    }
                    
                    if (GUILayout.Button(row.IsShowTranslated? "Translated" : "Original", row.IsShowTranslated? UiStyles.ButtonStyleGreen : EditorStyles.miniButton, GUILayout.Width(120)))
                    {
                        row.IsShowTranslated = !row.IsShowTranslated;
                    }

                    if (row.IsShowTranslated)
                    {
                        var editButtonContent = new GUIContent("E", "Edit");
                        if (GUILayout.Button(editButtonContent, GUILayout.Width(20)))
                        {
                            if (_editRowId != row.Id)
                            {
                                _editRowId = row.Id;
                            }
                            else
                            {
                                _editRowId = -1;
                                GUI.FocusControl(null);
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(row.TranslatedText))
                {
                    if (row.IsShowTranslated && row.TranslatedText != row.OriginalText)
                    {
                        var addButtonContent = new GUIContent("A", "Apply Change");
                        if (GUILayout.Button(addButtonContent, GUILayout.Width(20)))
                        {
                            if (_translateExtensionManager.ApplyChange(row))
                            {
                                EditorUtility.SetDirty(_languageSourceAsset);
                                AssetDatabase.SaveAssets();
                            }
                        }
                        
                        var rewertButtonContent = new GUIContent("R", "Rewert Change");
                        if (GUILayout.Button(rewertButtonContent, GUILayout.Width(20)))
                        {
                            GUI.FocusControl(null);
                            _translateExtensionManager.RewertChange(row);
                        }
                    }
                    
                    EditorGUILayout.LabelField("●",
                        row.TranslatedText != row.OriginalText
                            ? UiStyles.LabelRowStyleGreen
                            : UiStyles.LabelRowStyleYellow, GUILayout.Width(15));
                }
                
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space();

            //down button no needed
            /*if (_translateExtensionManager.IsTranslateProviderAndTranslateSettingSetup)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                
                if (_translateExtensionManager.IsTranslateAllStarted)
                {
                    if (GUILayout.Button("Stop Translate Process", GUILayout.Width(100)))
                    {
                        _translateExtensionManager.StopTranslateProcess();
                    }
                }
                else
                {
                    if (GUILayout.Button("Translate All", GUILayout.Width(100)))
                    {
                        var result = UiTools.DisplayTranslateAllDialog();
                        
                        if (result) _translateExtensionManager.TranslateAll(Repaint);
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }*/
            
            EditorGUI.indentLevel--;
            EditorGUILayout.EndScrollView();
        }
    }
}