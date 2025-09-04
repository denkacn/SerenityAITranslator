using SerenityAITranslator.Editor.Context;
using SerenityAITranslator.Editor.Services.Common.Views;
using SerenityAITranslator.Editor.Services.Translation.Views;
using SerenityAITranslator.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Settings.Views
{
    public class SettingsMainView : MainView
    {
        private TranslateProviderView _translateProviderView;
        private TranslatePromtView _translatePromtView;
        private TtsProviderView _ttsProviderView;
        
        public SettingsMainView(EditorWindow owner, SerenityContext context) : base(owner, context){}

        public override void Init()
        {
            _translateProviderView = new TranslateProviderView(_owner, _context);
            _translatePromtView = new TranslatePromtView(_owner, _context);
            _ttsProviderView = new TtsProviderView(_owner, _context);
        }

        public override void Draw()
        {
            if (_context.TranslateManager != null && _context.TranslateManager.IsContextSetup)
            {
                DrawTranslateProvider();
            }
        }
        
        private void DrawTranslateProvider()
        {
            GUILayout.Space(10);

            if (_context.TranslateProvidersSetting.Settings.Count == 0)
            {
                EditorGUILayout.HelpBox("Add at least one Translate Provider to start working with translation", MessageType.Info);
            }
                
            GUILayout.BeginHorizontal(UiStyles.OddRowStyle);
            GUILayout.Label("Settings", UiStyles.LabelStyleCenter);
            var translationSessionData = _context.SessionData.TranslationSessionData;
            if (GUILayout.Button(translationSessionData.IsShowSettingView? "X" : "▼", GUILayout.Width(20), GUILayout.Height(20)))
            {
                translationSessionData.IsShowSettingView = !translationSessionData.IsShowSettingView;
                _context.Save();
            }
            GUILayout.EndHorizontal();
            
            if (translationSessionData.IsShowSettingView)
            {
                GUILayout.BeginVertical();
                
                _translateProviderView?.Draw();
                _ttsProviderView.Draw();
                _translatePromtView?.Draw();
                
                
                GUILayout.Space(10);

                GUILayout.EndVertical();
            }
            
            GUILayout.Space(10);
        }
    }
}