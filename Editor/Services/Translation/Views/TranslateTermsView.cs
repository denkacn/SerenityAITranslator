using SerenityAITranslator.Editor.Context;
using SerenityAITranslator.Editor.Services.Common.Views;
using SerenityAITranslator.Editor.Services.Translation.SourceAssetProvider;
using SerenityAITranslator.Editor.Services.Translation.Windows;
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
            EditorGUILayout.LabelField("", UiStyles.LabelHeaderStyle, GUILayout.Width(30));
            EditorGUILayout.LabelField("Terms", UiStyles.LabelHeaderStyle, GUILayout.Width(215));
            EditorGUILayout.LabelField("Base Text", UiStyles.LabelHeaderStyle, GUILayout.Width(415));
            EditorGUILayout.LabelField("Translation", UiStyles.LabelHeaderStyle, GUILayout.Width(400));
            EditorGUILayout.LabelField("Controls", UiStyles.LabelHeaderStyle, GUILayout.Width(100));
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            for (var i = 0; i < rows.Count; i++)
            {
                var rowStyle = (i % 2 == 0) ? UiStyles.EvenRowStyle : UiStyles.OddRowStyle;
                var row = rows[i];
                var destinationLanguageIndex = translationSessionData.AvailableLanguages.IndexOf(translationSessionData.DestinationLanguage);
                var translatedText = row.IsShowTranslated ? row.TranslatedText[destinationLanguageIndex] : row.OriginalText;

                EditorGUILayout.BeginHorizontal(rowStyle);
                
                //EditorGUILayout.LabelField(row.Id.ToString(), GUILayout.Width(40));
                var sourceHeight = UiStyles.TableCellStyle.CalcHeight(new GUIContent(row.SourceText), 400);
                var translationHeight = UiStyles.TableCellStyle.CalcHeight(new GUIContent(translatedText), 400);
                var rowHeight = Mathf.Clamp(Mathf.Max(30, sourceHeight, translationHeight), 30, 800);
                
                var selectRect = GUILayoutUtility.GetRect(30, rowHeight, GUILayout.Width(30), GUILayout.Height(rowHeight));
                var toggleRect = new Rect(selectRect.x + 7, selectRect.y + (rowHeight - 16) * 0.5f, 16, 16);
                row.IsSelected = EditorGUI.Toggle(toggleRect, row.IsSelected);
                EditorGUILayout.LabelField(row.Term, UiStyles.TableCellStyle, GUILayout.Width(200), GUILayout.Height(rowHeight));
                EditorGUILayout.LabelField(row.SourceText, UiStyles.TableCellStyle, GUILayout.Width(400), GUILayout.Height(rowHeight));
                
                if (row.IsShowTranslated)
                {
                    if (_editRowId == row.Id)
                    {
                        row.TranslatedText[destinationLanguageIndex] = EditorGUILayout.TextArea(row.TranslatedText[destinationLanguageIndex], GUILayout.Width(415), GUILayout.Height(rowHeight));
                        //GUI.FocusControl(null);
                        //Repaint();
                    }
                    else
                    {
                        EditorGUILayout.LabelField(translatedText, UiStyles.TableCellStyleGreen, GUILayout.Width(400), GUILayout.Height(rowHeight));
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(translatedText, UiStyles.TableCellStyle, GUILayout.Width(400), GUILayout.Height(rowHeight));
                }
                
                if (_context.TranslateManager.IsTranslateProviderAndTranslateSettingSetup)
                {
                    var translateButtonContent = new GUIContent("T", "Translate");
                    if (DrawCenteredButton(translateButtonContent, 30, rowHeight))
                    {
                        _context.TranslateManager.TranslateOne(row, Repaint);
                        GUI.FocusControl(null);
                    }
                    
                    var translateAllButtonContent = new GUIContent("TA", "Translate To All Languages");
                    if (DrawCenteredButton(translateAllButtonContent, 30, rowHeight))
                    {
                        //var result = UiTools.DisplayTranslateSelectedDialog();
                        //_context.TranslateManager.TranslateSelectedToAllLanguages(Repaint);
                        
                        var window = EditorWindow.GetWindow<TranslateToAllLanguagesWindow>("Translate To All Languages");
                        window.Show();
                        window.Init(row, _context);
                    }
                    
                    if (DrawCenteredButton(row.IsShowTranslated? "Translated" : "Original", 120, rowHeight, row.IsShowTranslated? UiStyles.ButtonStyleGreen : EditorStyles.miniButton))
                    {
                        row.IsShowTranslated = !row.IsShowTranslated;
                    }

                    if (row.IsShowTranslated)
                    {
                        var editButtonContent = new GUIContent("E", "Edit");
                        if (DrawCenteredButton(editButtonContent, 20, rowHeight))
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

                if (!string.IsNullOrEmpty(row.TranslatedText[destinationLanguageIndex]))
                {
                    if (row.IsShowTranslated && row.TranslatedText[destinationLanguageIndex] != row.OriginalText)
                    {
                        var addButtonContent = new GUIContent("A", "Apply Change");
                        if (DrawCenteredButton(addButtonContent, 20, rowHeight))
                        {
                            _context.TranslateManager.ApplyChange(row, isOk =>
                            {
                                EditorUtility.SetDirty(_sourceAssetProvider.GetAsset());
                                AssetDatabase.SaveAssets();
                            });
                        }
                        
                        var rewertButtonContent = new GUIContent("R", "Rewert Change");
                        if (DrawCenteredButton(rewertButtonContent, 20, rowHeight))
                        {
                            GUI.FocusControl(null);
                            _context.TranslateManager.RewertChange(row);
                        }
                    }
                    
                    EditorGUILayout.LabelField("●",
                        row.TranslatedText[destinationLanguageIndex] != row.OriginalText
                            ? UiStyles.TableCellStyleGreen
                            : UiStyles.TableCellStyleYellow, GUILayout.Width(15), GUILayout.Height(rowHeight));
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
