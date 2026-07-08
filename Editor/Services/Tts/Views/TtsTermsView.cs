using SerenityAITranslator.Editor.Context;
using SerenityAITranslator.Editor.Services.Common.Views;
using SerenityAITranslator.Editor.Services.Translation.SourceAssetProvider;
using SerenityAITranslator.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Tts.Views
{
    public class TtsTermsView : BaseView
    {
        private readonly ISourceAssetProvider _sourceAssetProvider;
        private Vector2 _scrollPosition;
        private int _editRowId = -1;
        
        public TtsTermsView(EditorWindow owner, ISourceAssetProvider sourceAssetProvider, SerenityContext context) : base(owner, context)
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
            EditorGUILayout.LabelField("Terms", UiStyles.LabelHeaderStyle, GUILayout.Width(215));
            EditorGUILayout.LabelField("Text", UiStyles.LabelHeaderStyle, GUILayout.Width(415));
            EditorGUILayout.LabelField("Controls", UiStyles.LabelHeaderStyle, GUILayout.Width(100));
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            for (var i = 0; i < rows.Count; i++)
            {
                var rowStyle = (i % 2 == 0) ? UiStyles.EvenRowStyle : UiStyles.OddRowStyle;
                var row = rows[i];

                EditorGUILayout.BeginHorizontal(rowStyle);
                var rowHeight = Mathf.Clamp(Mathf.Max(30, UiStyles.TableCellStyle.CalcHeight(new GUIContent(row.SourceText), 400)), 30, 800);
                
                EditorGUILayout.LabelField(row.Term, UiStyles.TableCellStyle, GUILayout.Width(200), GUILayout.Height(rowHeight));
                //EditorGUILayout.LabelField(row.SourceText, UiStyles.LabelRowStyle, GUILayout.Width(400), GUILayout.MinHeight(30), GUILayout.MaxHeight(800));
                
                if (_editRowId == row.Id)
                {
                    row.SourceText = EditorGUILayout.TextArea(row.SourceText, UiStyles.WrapTextAreaStyle, GUILayout.Width(415), GUILayout.Height(rowHeight));
                    //GUI.FocusControl(null);
                    //Repaint();
                }
                else
                {
                    EditorGUILayout.LabelField(row.SourceText, UiStyles.TableCellStyle, GUILayout.Width(400), GUILayout.Height(rowHeight));
                }
                
                if (_context.TtsManager.IsTtsProviderAndTtsSettingSetup)
                {
                    var language = _context.SessionData.TranslationSessionData.SourceLanguage;
                    if (_context.VoicesCollection.IsExist(row.Term, language))
                    {
                        if (GUILayout.Button("Info", UiStyles.ButtonStyleYellowText, GUILayout.Width(80)))
                        {
                            _context.TtsManager.SetForInfo(row);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("To Voice", GUILayout.Width(80)))
                        {
                            _context.TtsManager.TranslateOne(row, Repaint);
                            GUI.FocusControl(null);
                        }
                    }
                    
                    EditorGUIUtility.SetIconSize(new Vector2(10, 10));

                    if (GUILayout.Button(new GUIContent(AssetsUtility.LoadIcon("icon-play.png"), "Play Clip"), GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        _context.TtsManager.SetForInfo(row);
                        _context.TtsManager.Play(row);
                    }
                    
                    if (GUILayout.Button(new GUIContent(AssetsUtility.LoadIcon("icon-switch.png"), "Text to Speech"), GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        _context.TtsManager.TranslateOne(row, Repaint);
                    }
                    
                    if (GUILayout.Button(new GUIContent(AssetsUtility.LoadIcon("icon-edit.png"), "Edit text"), GUILayout.Width(20), GUILayout.Height(20)))
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
                    
                    if (GUILayout.Button(new GUIContent(AssetsUtility.LoadIcon("icon-delete.png"), "Delete"), GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        var result = UiTools.DisplayDeleteVoiceDialog();

                        if (result) _context.TtsManager.DeleteVoice(row.Term, Repaint);
                    }
                    
                    EditorGUIUtility.SetIconSize(new Vector2(16, 16));
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
