using SerenityAITranslator.Editor.Context;
using SerenityAITranslator.Editor.Services.Translation.Models;
using SerenityAITranslator.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Translation.Windows
{
    public class TranslateToAllLanguagesWindow : EditorWindow
    {
        private SerenityContext _context;
        private TranslatedRowData _rowData;
 

        public void Init(TranslatedRowData rowData, SerenityContext context)
        {
            _context = context;        
            _rowData = rowData;
        }
        
        private void OnGUI()
        {
            if (_context == null) return;
            
            EditorGUILayout.BeginVertical("helpbox");
            
            GUILayout.Label("Translate To All Languages", UiStyles.LabelRowStyleYellowBold);
            var allLanguages = _context.SessionData.TranslationSessionData.AvailableLanguages;
            for (var i = 0; i < allLanguages.Count; i++)
            {
                var rowStyle = (i % 2 == 0) ? UiStyles.OddRowStyle : UiStyles.EvenRowStyle;
                var language = allLanguages[i];

                var isBase = language == _context.SessionData.TranslationSessionData.BaseLanguage;

                EditorGUILayout.BeginHorizontal(rowStyle);
                
                EditorGUILayout.LabelField(language, GUILayout.Width(100));
                if (!isBase)
                    _rowData.TranslatedText[i] =
                        EditorGUILayout.TextArea(_rowData.TranslatedText[i], GUILayout.Width(300));
                else
                {
                    var height = UiStyles.LabelRowStyle.CalcHeight(new GUIContent(_rowData.SourceText), 400);
                    EditorGUILayout.LabelField(_rowData.SourceText, GUILayout.Width(300), GUILayout.Height(height));
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Space(10);
            }
            
            EditorGUILayout.EndVertical();
            
            if (GUILayout.Button("Translate"))
            {
                _context.TranslateManager.TranslateToAllLanguages(_rowData, Repaint);
            }
            
            if (GUILayout.Button("Apply Changes"))
            {
                _context.TranslateManager.ApplyChangeToAllLanguages(_rowData, isOk => Repaint());
                Close();
            }
        }   
    }
}