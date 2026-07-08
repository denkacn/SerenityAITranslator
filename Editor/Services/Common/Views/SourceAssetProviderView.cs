using System;
using System.Linq;
using SerenityAITranslator.Editor.Context;
using SerenityAITranslator.Editor.Services.Translation.SourceAssetProvider;
using SerenityAITranslator.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Common.Views
{
    [Serializable]
    public class SourceAssetProviderView : BaseView
    {
        [SerializeReference] private ISourceAssetProvider _sourceAssetProvider;
        private SerializedObject _serializedProvider;
        [SerializeReference] private ScriptableObject _provider;
        
        private ISourceAssetProvider[] _providers;
        private string[] _providerNames;
        private int _selectedIndex = 0;
        
        [SerializeReference] private bool _isShowAssetProviderMenu = true;

        private Action<ISourceAssetProvider> _onSetup;
        
        public SourceAssetProviderView(EditorWindow owner, SerenityContext context) : base(owner, context)
        {
            LoadProviders();
        }

        public void Init(Action<ISourceAssetProvider> onSetup)
        {
            _onSetup = onSetup;
            
            if(_sourceAssetProvider != null && _sourceAssetProvider.IsReady())
                _onSetup?.Invoke(_sourceAssetProvider);
        }
        
        public override void Draw()
        {
            DrawSourceAssetProvider();
        }
        
        private void DrawSourceAssetProvider()
        {
            GUILayout.BeginVertical();
            
            GUILayout.BeginHorizontal(UiStyles.OddRowStyle);
            GUILayout.Label("Source Asset Provider", UiStyles.LabelStyleCenter);
            if (GUILayout.Button("Reload", GUILayout.Width(60), GUILayout.Height(20)))
            {
                LoadProviders();
            }
            
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
                        _onSetup?.Invoke(_sourceAssetProvider);
                }
            
                GUILayout.EndHorizontal();
            }
            
            GUILayout.EndVertical();
        }
        
        private void DrawSourceAssetProviderField()
        {
            if (_providers == null || _providers.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "No source asset providers found. Import one extension package from SerenityAITranslator/Extension, then create a provider asset in the project.",
                    MessageType.Info);
                
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
                
                _onSetup?.Invoke(_sourceAssetProvider);
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
        
        private void LoadProviders()
        {
            var guids = AssetDatabase.FindAssets("t:ScriptableObject");

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
