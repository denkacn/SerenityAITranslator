using System;
using SerenityAITranslator.Editor.Context;
using SerenityAITranslator.Editor.Services.Common.Views;
using SerenityAITranslator.Editor.Services.Settings.Views;
using SerenityAITranslator.Editor.Services.Translation.SourceAssetProvider;
using SerenityAITranslator.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Translation.Views
{
    [Serializable]
    public class TranslateMainView : MainView
    {
        private Vector2 _scrollPosition;
        
        //views
        private TranslateSettingsButtonView _translateSettingsButtonView;
        private TranslateTermsView _translateTermsView;
        private TranslateProviderView _translateProviderView;
        private TranslatePromtView _translatePromtView;
        private SourceAssetProviderView _sourceAssetProviderView;

        [SerializeField] private bool _isShowAssetProviderMenu = true;
        
        public TranslateMainView(EditorWindow owner, SerenityContext context) : base(owner, context){}

        public override void Init()
        {
            _sourceAssetProviderView = new SourceAssetProviderView(_owner, _context);
            _sourceAssetProviderView.Init(provider => Setup(provider));
        }

        public override void Draw()
        {
            if (string.IsNullOrEmpty(_context.SessionData.TranslationSessionData.ProviderId))
            {
                EditorGUILayout.HelpBox("Create and select Translate Providers to continue editing.",
                    MessageType.Info);
                return;
            }
            
            _sourceAssetProviderView.Draw();
            
            GUILayout.Space(10);
            
            if(_context.TranslateManager == null) return;
            
            if (_context.TranslateManager != null && _context.TranslateManager.IsContextSetup)
            {
                DrawTranslateUI();
            }
        }
        
        private void DrawTranslateUI()
        {
            GUILayout.BeginHorizontal(UiStyles.OddRowStyle);
            GUILayout.Label("Info", UiStyles.LabelStyleCenter);
            var translationSessionData = _context.SessionData.TranslationSessionData;
            if (GUILayout.Button(translationSessionData.IsShowInfoView? "X" : "▼", GUILayout.Width(20), GUILayout.Height(20)))
            {
                translationSessionData.IsShowInfoView = !translationSessionData.IsShowInfoView;
                _context.Save();
            }
            GUILayout.EndHorizontal();
            
            if (translationSessionData.IsShowInfoView)
            {
                EditorGUILayout.BeginVertical("helpbox");
                GUILayout.Label(new GUIContent(_context.TranslateManager.GetInfo()), UiStyles.LabelStyleRich);
                EditorGUILayout.EndVertical();
            }
            
            GUILayout.Space(10);
            
            _translateSettingsButtonView?.Draw();
            
            _translateTermsView?.Draw();
        }
        
        private void Setup(ISourceAssetProvider sourceAssetProvider)
        {
            _context.TranslateManager.SetSourceAssetProvider(sourceAssetProvider);
            _context.TranslateManager.Setup();
            
            //views
            _translateSettingsButtonView = new TranslateSettingsButtonView(_owner, sourceAssetProvider, _context);
            _translateTermsView = new TranslateTermsView(_owner, sourceAssetProvider, _context);
            _translateProviderView = new TranslateProviderView(_owner, _context);
            _translatePromtView = new TranslatePromtView(_owner, _context);
        }
    }
}