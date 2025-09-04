using System;
using SerenityAITranslator.Editor.Context;
using SerenityAITranslator.Editor.Services.Common.Collections;
using SerenityAITranslator.Editor.Services.Common.Views;
using SerenityAITranslator.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Settings.Views
{
    public class TranslatePromtView : BaseView
    {
        private PromtSettingsItem _promtSettingData;
        private bool _isShowAddMenu = false;
        
        public TranslatePromtView(EditorWindow owner, SerenityContext context) : base(owner, context){}

        public override void Draw()
        {
            EditorGUILayout.BeginVertical("helpbox");
            
            GUILayout.Label("Promt Settings:", UiStyles.LabelRowStyleYellowBold);

            DrawPromtList();
            
            if (_isShowAddMenu) DrawAddPromt();
            
            if (!_isShowAddMenu)
            {
                if (GUILayout.Button("Add Promt"))
                {
                    _promtSettingData = new PromtSettingsItem();
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
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
        }
        
        private void DrawAddPromt()
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            
            var originalLabelWidth = EditorGUIUtility.labelWidth;
            
            EditorGUIUtility.labelWidth = UiTools.GetLabelWidth("Name"); 
            _promtSettingData.Name = EditorGUILayout.TextField("Name", _promtSettingData.Name,GUILayout.Width(250));
            
            EditorGUIUtility.labelWidth = UiTools.GetLabelWidth("Promt"); 
            _promtSettingData.Promt = EditorGUILayout.TextField("Promt", _promtSettingData.Promt,GUILayout.Width(800));

            EditorGUIUtility.labelWidth = originalLabelWidth;
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Save"))
            {
                _promtSettingData.Id = Guid.NewGuid().ToString();
                _context.PromtSettings.Settings.Add(_promtSettingData);
                _context.Save();
                _isShowAddMenu = false;
            }

            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawPromtList()
        {
            var promtSettings = _context.PromtSettings.Settings;
            for (var i = 0; i < promtSettings.Count; i++)
            {
                var rowStyle = (i % 2 == 0) ? UiStyles.OddRowStyle : UiStyles.EvenRowStyle;
                var promtData = promtSettings[i];

                EditorGUILayout.BeginHorizontal(rowStyle);
                
                EditorGUILayout.LabelField(promtData.Name, UiStyles.LabelRowStyle, GUILayout.Width(200));
                EditorGUILayout.LabelField(promtData.Promt, UiStyles.LabelRowStyle, GUILayout.Width(800));

                var isSelected = _context.SessionData.TranslationSessionData.SelectedPromt ==
                                 promtData.Promt;
                
                if (GUILayout.Button("Select", isSelected? UiStyles.ButtonStyleGreen : EditorStyles.miniButton, GUILayout.Width(100)))
                {
                    _context.TranslateManager.SelectPromt(promtData);
                }
                
                if (GUILayout.Button("Remove", GUILayout.Width(100)))
                {
                    var result = UiTools.DisplayRemovePromtDialog();
                    if (result)
                    {
                        _context.PromtSettings.Settings.Remove(promtData);
                        _context.Save();
                    }
                }
                
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Space(10);
            }
        }
    }
}