using System;
using SerenityAITranslator.Editor.Context;
using SerenityAITranslator.Editor.Services.Common.Views;
using SerenityAITranslator.Editor.Services.Translation.AiProviders;
using SerenityAITranslator.Editor.Services.Translation.Collections;
using SerenityAITranslator.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Translation.Views
{
    public class TranslateProviderView : BaseView
    {
        private bool _isShowAddMenu = false;
        private TranslateProviderConfigurationItem _newTranslateProviderSettings;
        public TranslateProviderView(EditorWindow owner, SerenityContext context) : base(owner, context){}
        
        public override void Draw()
        {
            GUILayout.Label("Translate Provider Settings", EditorStyles.boldLabel);
            
            DrawTranslateProviderList();
            
            if (_isShowAddMenu)
            {
                DrawAddProvider();
            }

            if (!_isShowAddMenu)
            {
                if (GUILayout.Button("Add Provider"))
                {
                    _newTranslateProviderSettings = new TranslateProviderConfigurationItem();
                    _isShowAddMenu = true;
                }
            }
            else
            {
                if (GUILayout.Button("Close"))
                {
                    _isShowAddMenu = false;
                }
            }
            
            GUILayout.Space(10);
        }

        private void DrawAddProvider()
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            
            var originalLabelWidth = EditorGUIUtility.labelWidth;
            
            EditorGUIUtility.labelWidth = UiTools.GetLabelWidth("Provider Type"); 
            var providerType = (TextProviderType)EditorGUILayout.EnumPopup("Provider Type", _newTranslateProviderSettings.ProviderType,GUILayout.Width(200));

            if (_newTranslateProviderSettings.ProviderType != providerType)
            {
                _newTranslateProviderSettings.ProviderType = providerType;
                var setting = _context.TranslateProvidersSetting.Settings.Find(s => s.ProviderType == providerType);
                if (setting != null)
                {
                    _newTranslateProviderSettings.Host = setting.Host;
                    _newTranslateProviderSettings.Endpoint = setting.Endpoint;
                }
                else
                {
                    _newTranslateProviderSettings.Host = string.Empty;
                    _newTranslateProviderSettings.Endpoint = string.Empty;
                }
            }
            
            EditorGUIUtility.labelWidth = UiTools.GetLabelWidth("Host"); 
            _newTranslateProviderSettings.Host = EditorGUILayout.TextField("Host", _newTranslateProviderSettings.Host,GUILayout.Width(250));
            
            EditorGUIUtility.labelWidth = UiTools.GetLabelWidth("Endpoint"); 
            _newTranslateProviderSettings.Endpoint = EditorGUILayout.TextField("Endpoint", _newTranslateProviderSettings.Endpoint,GUILayout.Width(250));
            
            EditorGUIUtility.labelWidth = UiTools.GetLabelWidth("Token"); 
            _newTranslateProviderSettings.Token = EditorGUILayout.TextField("Token", _newTranslateProviderSettings.Token,GUILayout.Width(250));
            
            if (GUILayout.Button("File", _newTranslateProviderSettings.IsTokenFromFile? UiStyles.ButtonStyleGreen : EditorStyles.miniButton))
            {
                var path = EditorUtility.OpenFilePanel(
                    "Select File",
                    "",
                    "txt"
                );
        
                if (!string.IsNullOrEmpty(path))
                {
                    _newTranslateProviderSettings.TokenFilePath = path;
                    _newTranslateProviderSettings.IsTokenFromFile = true;
                }
            }
            
            EditorGUIUtility.labelWidth = UiTools.GetLabelWidth("Model"); 
            _newTranslateProviderSettings.Model = EditorGUILayout.TextField("Model", _newTranslateProviderSettings.Model,GUILayout.Width(250));

            EditorGUIUtility.labelWidth = originalLabelWidth;
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Save"))
            {
                if (_newTranslateProviderSettings.ProviderType == TextProviderType.None)
                {
                    UiTools.DisplayCreateProviderErrorMessage();
                }
                else
                {
                    _newTranslateProviderSettings.Id = Guid.NewGuid().ToString();
                    _context.TranslateProvidersConfigurations.Providers.Add(_newTranslateProviderSettings);
                    _context.Save();
                    _isShowAddMenu = false;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawTranslateProviderList()
        {
            var providers = _context.TranslateProvidersConfigurations.Providers;
            for (var i = 0; i < providers.Count; i++)
            {
                var rowStyle = (i % 2 == 0) ? UiStyles.EvenRowStyle : UiStyles.OddRowStyle;
                var provider = providers[i];

                EditorGUILayout.BeginHorizontal(rowStyle);
                
                EditorGUILayout.LabelField(provider.ProviderType.ToString(), GUILayout.Width(100));
                EditorGUILayout.LabelField(provider.Host, GUILayout.Width(200));
                EditorGUILayout.LabelField(provider.Endpoint, GUILayout.Width(200));
                EditorGUILayout.LabelField(provider.IsTokenFromFile ? "From File" : string.IsNullOrEmpty(provider.Token) ? "" : "*****", GUILayout.Width(200));
                EditorGUILayout.LabelField(provider.Model, GUILayout.Width(200));
                
                var isSelected = _context.TranslateManager.SelectedTranslateProviderId == provider.Id;

                if (GUILayout.Button("Select", isSelected? UiStyles.ButtonStyleGreen : EditorStyles.miniButton, GUILayout.Width(100)))
                {
                    _context.TranslateManager.SelectTranslateProviderSettings(provider);
                }
                
                if (GUILayout.Button("Remove", GUILayout.Width(100)))
                {
                    var result = UiTools.DisplayRemoveProviderDialog();
                    if (result)
                    {
                        _context.TranslateProvidersConfigurations.Providers.Remove(provider);
                        _context.Save();
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Space(10);
            }
        }
    }
}