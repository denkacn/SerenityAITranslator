using System;
using System.Linq;
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
        [SerializeReference] private ISourceAssetProvider _sourceAssetProvider;
        private SerializedObject _serializedProvider;
        [SerializeField] private ScriptableObject _provider;
        
        private Vector2 _scrollPosition;
        
        private ISourceAssetProvider[] _providers;
        private string[] _providerNames;
        private int _selectedIndex = 0;
        
        //views
        private TranslateSettingsButtonView _translateSettingsButtonView;
        private TranslateTermsView _translateTermsView;
        private TranslateProviderView _translateProviderView;
        private TranslatePromtView _translatePromtView;

        [SerializeField] private bool _isShowAssetProviderMenu = true;
        
        public TranslateMainView(EditorWindow owner, SerenityContext context) : base(owner, context){}

        public override void Init()
        {
            LoadProviders();
            
            if(_sourceAssetProvider != null && _sourceAssetProvider.IsReady())
                Setup();
        }

        public override void Draw()
        {
            if (string.IsNullOrEmpty(_context.SessionData.TranslationSessionData.ProviderId))
            {
                EditorGUILayout.HelpBox("Create and select Translate Providers to continue editing.",
                    MessageType.Info);
                return;
            }
            
            DrawSourceAssetProvider();
            
            if(_context.TranslateManager == null) return;
            
            if (_context.TranslateManager != null && _context.TranslateManager.IsContextSetup)
            {
                //DrawTranslateProvider();
                
                DrawTranslateUI();
            }
        }

        private void DrawSourceAssetProvider()
        {
            GUILayout.BeginVertical();
            
            GUILayout.BeginHorizontal(UiStyles.OddRowStyle);
            GUILayout.Label("Source Asset Provider", UiStyles.LabelStyleCenter);
            if (GUILayout.Button(_isShowAssetProviderMenu? "X" : "▼", GUILayout.Width(20), GUILayout.Height(20)))
            {
                _isShowAssetProviderMenu = !_isShowAssetProviderMenu;
            }
            GUILayout.EndHorizontal();

            if (_isShowAssetProviderMenu)
            {
                DrawSourceAssetProviderField();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
            
                if (GUILayout.Button("Setup", GUILayout.Width(100)))
                {
                    if (_sourceAssetProvider != null && _sourceAssetProvider.IsReady())
                        Setup();
                }
            
                GUILayout.EndHorizontal();
            }
            
            GUILayout.EndVertical();
        }

        private void DrawTranslateProvider()
        {
            GUILayout.Space(10);
                
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
                _translatePromtView?.Draw();
                
                GUILayout.Space(10);

                GUILayout.EndVertical();
            }
            
            GUILayout.Space(10);
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
                GUILayout.Label(new GUIContent(_context.TranslateManager.GetInfo()));
            }
            
            GUILayout.Space(10);
            
            _translateSettingsButtonView?.Draw();
            
            _translateTermsView?.Draw();
        }

        private void DrawSourceAssetProviderField()
        {
            if (_providers == null || _providers.Length == 0)
            {
                if (GUILayout.Button("Reload Providers"))
                    LoadProviders();
                
                return;
            }

            _selectedIndex = EditorGUILayout.Popup("Active Provider", _selectedIndex, _providerNames);
            _sourceAssetProvider = _providers[_selectedIndex];

            if (_sourceAssetProvider != null) _sourceAssetProvider.OnDraw();

            var scriptableSourceAssetProvider = (ScriptableObject)_sourceAssetProvider;
            
            if (scriptableSourceAssetProvider != _provider)
            {
                _provider = scriptableSourceAssetProvider;
                _serializedProvider = _sourceAssetProvider != null ? new SerializedObject(scriptableSourceAssetProvider) : null;
                
                Setup();
            }
            
            if (_provider is ISourceAssetProvider && _serializedProvider != null)
            {
                _serializedProvider.Update();

                var iterator = _serializedProvider.GetIterator();
                var enterChildren = true;

                while (iterator.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (iterator.propertyPath == "m_Script") continue; 
                    EditorGUILayout.PropertyField(iterator, true);
                }

                _serializedProvider.ApplyModifiedProperties();
            }
            else if (_provider != null)
            {
                EditorGUILayout.HelpBox("Объект не реализует ISourceAssetProvider", MessageType.Error);
            }
            
            GUILayout.Space(10);
        }
        
        private void Setup()
        {
            _context.TranslateManager.SetSourceAssetProvider(_sourceAssetProvider);
            _context.TranslateManager.Setup();
            
            //views
            _translateSettingsButtonView = new TranslateSettingsButtonView(_owner, _sourceAssetProvider, _context);
            _translateTermsView = new TranslateTermsView(_owner, _sourceAssetProvider, _context);
            _translateProviderView = new TranslateProviderView(_owner, _context);
            _translatePromtView = new TranslatePromtView(_owner, _context);
        }
        
        private void LoadProviders()
        {
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");

            _providers = guids
                .Select(g => AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(g)))
                .OfType<ISourceAssetProvider>()
                .ToArray();

            _providerNames = _providers
                .Select(p => ((ScriptableObject)p).name)
                .ToArray();
        }
    }
}