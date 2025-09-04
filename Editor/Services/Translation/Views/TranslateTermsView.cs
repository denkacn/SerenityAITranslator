using SerenityAITranslator.Editor.Context;
using SerenityAITranslator.Editor.Services.Common.Views;
using SerenityAITranslator.Editor.Services.Translation.SourceAssetProvider;
using SerenityAITranslator.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Translation.Views
{
    public class TranslateTermsView : BaseView
    {
        private readonly ISourceAssetProvider _sourceAssetProvider;
        private Vector2 _scrollPosition;
        private int _editRowId = -1;
        
        public TranslateTermsView(EditorWindow owner, ISourceAssetProvider sourceAssetProvider, SerenityContext context) : base(owner, context)
        {
            _sourceAssetProvider = sourceAssetProvider;
        }
        
        public override void Draw()
        {
            var translationSessionData = _context.SessionData.TranslationSessionData;
            if (translationSessionData.TranslationsData == null) return;
            
            var rows = translationSessionData.TranslationsData;
            
            EditorGUILayout.BeginHorizontal(UiStyles.OddRowStyle);
            EditorGUILayout.LabelField("Translation Table", UiStyles.LabelStyleCenter);
            EditorGUILayout.EndHorizontal();
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal(UiStyles.DarkRowStyle);
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
                
                if (_context.TranslateManager.IsTranslateProviderAndTranslateSettingSetup)
                {
                    if (GUILayout.Button("Translate", GUILayout.Width(80)))
                    {
                        _context.TranslateManager.TranslateOne(row, Repaint);
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
                            _context.TranslateManager.ApplyChange(row, isOk =>
                            {
                                EditorUtility.SetDirty(_sourceAssetProvider.GetAsset());
                                AssetDatabase.SaveAssets();
                            });
                        }
                        
                        var rewertButtonContent = new GUIContent("R", "Rewert Change");
                        if (GUILayout.Button(rewertButtonContent, GUILayout.Width(20)))
                        {
                            GUI.FocusControl(null);
                            _context.TranslateManager.RewertChange(row);
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
            
            EditorGUI.indentLevel--;
            EditorGUILayout.EndScrollView();
        }
    }
}