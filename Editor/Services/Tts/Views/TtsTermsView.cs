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
                
                EditorGUILayout.LabelField(row.Term, UiStyles.LabelRowStyle,GUILayout.Width(200));
                EditorGUILayout.LabelField(row.SourceText, UiStyles.LabelRowStyle, GUILayout.Width(400), GUILayout.MinHeight(30), GUILayout.MaxHeight(800));
                
                if (_context.TtsManager.IsTtsProviderAndTtsSettingSetup)
                {
                    if (GUILayout.Button("Get Voice", GUILayout.Width(80)))
                    {
                        _context.TtsManager.TranslateOne(row, Repaint);
                        GUI.FocusControl(null);
                    }
                    EditorGUIUtility.SetIconSize(new Vector2(10, 10));
                    //Debug.Log(AudioClipTools.IsPlaying());
                    if (GUILayout.Button(new GUIContent(AssetsUtility.LoadIcon("icon-play.png")), GUILayout.Width(40), GUILayout.Height(20)))
                    {
                        _context.TtsManager.Play(row);
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