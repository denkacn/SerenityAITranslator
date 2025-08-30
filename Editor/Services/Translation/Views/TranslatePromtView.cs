using SerenityAITranslator.Editor.Services.Common.Models;
using SerenityAITranslator.Editor.Services.Common.Views;
using SerenityAITranslator.Editor.Services.Translation.Managers;
using SerenityAITranslator.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Translation.Views
{
    public class TranslatePromtView : BaseView
    {
        private readonly TranslateManager _translateManager;
        private PromtSettingData _promtSettingData;
        private bool _isShowAddMenu = false;
        
        public TranslatePromtView(EditorWindow owner, TranslateManager translateManager) : base(owner)
        {
            _translateManager = translateManager;
        }

        public override void Draw()
        {
            GUILayout.Label("Promt Settings", EditorStyles.boldLabel);

            DrawPromtList();
            if (_isShowAddMenu) DrawAddPromt();
            
            if (!_isShowAddMenu)
            {
                if (GUILayout.Button("Add Promt"))
                {
                    _promtSettingData = new PromtSettingData();
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
                _translateManager.AddPromt(_promtSettingData);
                _isShowAddMenu = false;
            }

            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawPromtList()
        {
            var promtSettings = _translateManager.GetPromtSettingsData();
            for (var i = 0; i < promtSettings.Count; i++)
            {
                var rowStyle = (i % 2 == 0) ? UiStyles.OddRowStyle : UiStyles.EvenRowStyle;
                var promtData = promtSettings[i];

                EditorGUILayout.BeginHorizontal(rowStyle);
                
                EditorGUILayout.LabelField(promtData.Name, UiStyles.LabelRowStyle, GUILayout.Width(200));
                EditorGUILayout.LabelField(promtData.Promt, UiStyles.LabelRowStyle, GUILayout.Width(800));

                var isSelected = _translateManager.SelectedPromt == promtData.Promt;
                
                if (GUILayout.Button("Select", isSelected? UiStyles.ButtonStyleGreen : EditorStyles.miniButton, GUILayout.Width(100)))
                {
                    _translateManager.SelectPromt(promtData);
                }
                
                if (GUILayout.Button("Remove", GUILayout.Width(100)))
                {
                    var result = UiTools.DisplayRemovePromtDialog();
                    if (result) _translateManager.RemovePromt(promtData);
                }
                
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Space(10);
            }
        }
    }
}