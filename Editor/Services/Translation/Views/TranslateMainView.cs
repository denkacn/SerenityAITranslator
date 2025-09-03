using System;
using System.Linq;
using System.Threading.Tasks;
using SerenityAITranslator.Editor.Services.Common.Views;
using SerenityAITranslator.Editor.Services.Managers;
using SerenityAITranslator.Editor.Services.Translation.Managers;
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
        
        private Type[] _providerTypes;
        
        private TranslateManager _translateManager;
        private Vector2 _scrollPosition;
        
        private TranslateSettingsButtonView _translateSettingsButtonView;
        private TranslateTermsView _translateTermsView;
        private TranslateProviderView _translateProviderView;
        private TranslatePromtView _translatePromtView;

        [SerializeField] private bool _isShowAssetProviderMenu = true;
        
        public TranslateMainView(EditorWindow owner, ISerenityAIManager manager) : base(owner, manager){}

        public override void Init()
        {
            GetProviderTypes();
        }

        public override void Draw()
        {
            DrawSourceAssetProvider();
            
            if(_translateManager == null) return;
            
            if (_translateManager != null && _translateManager.IsContextSetup)
            {
                DrawTranslateProvider();
                
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
            var translationSessionData = _translateManager.SessionRepository.SessionData.TranslationSessionData;
            if (GUILayout.Button(translationSessionData.IsShowSettingView? "X" : "▼", GUILayout.Width(20), GUILayout.Height(20)))
            {
                translationSessionData.IsShowSettingView = !translationSessionData.IsShowSettingView;
                _translateManager.SaveSession();
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
            var translationSessionData = _translateManager.SessionRepository.SessionData.TranslationSessionData;
            if (GUILayout.Button(translationSessionData.IsShowInfoView? "X" : "▼", GUILayout.Width(20), GUILayout.Height(20)))
            {
                translationSessionData.IsShowInfoView = !translationSessionData.IsShowInfoView;
                _translateManager.SaveSession();
            }
            GUILayout.EndHorizontal();

            if (translationSessionData.IsShowInfoView)
            {
                GUILayout.Label(new GUIContent(_translateManager.GetInfo()));
            }
            
            GUILayout.Space(10);
            
            _translateSettingsButtonView?.Draw();
            
            _translateTermsView?.Draw();
        }

        private void DrawSourceAssetProviderField()
        {
            if (_providerTypes.Length == 0)
            {
                EditorGUILayout.HelpBox("There are no available implementations ISourceAssetProvider", MessageType.Warning);
                return;
            }
            
            var currentIndex = Array.IndexOf(_providerTypes, _sourceAssetProvider?.GetType());
            if (currentIndex < 0) currentIndex = 0;

            var options = _providerTypes.Select(t => t.Name).ToArray();
            var newIndex = EditorGUILayout.Popup("Provider Type", currentIndex, options);
            
            if (newIndex != currentIndex || _sourceAssetProvider == null)
            {
                _sourceAssetProvider = (ISourceAssetProvider)Activator.CreateInstance(_providerTypes[newIndex]);
            }
            
            _sourceAssetProvider?.OnDraw();
            
            GUILayout.Space(10);
        }
        
        private async Task Setup()
        {
            _translateManager = _manager.TranslateManager; 
            _translateManager.SetSourceAssetProvider(_sourceAssetProvider);
            
            await _translateManager.Setup();
            
            //views
            _translateSettingsButtonView = new TranslateSettingsButtonView(_owner, _translateManager, _sourceAssetProvider);
            _translateTermsView = new TranslateTermsView(_owner, _translateManager, _sourceAssetProvider);
            _translateProviderView = new TranslateProviderView(_owner, _translateManager);
            _translatePromtView = new TranslatePromtView(_owner, _translateManager);
        }
        
        private void GetProviderTypes()
        {
            _providerTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(ISourceAssetProvider).IsAssignableFrom(t) 
                            && !t.IsInterface 
                            && !t.IsAbstract)
                .ToArray();
        }
    }
}