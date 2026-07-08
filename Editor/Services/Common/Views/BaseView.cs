using SerenityAITranslator.Editor.Context;
using SerenityAITranslator.Editor.Services.Common.Jobs;
using UnityEditor;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services.Common.Views
{
    public class BaseView
    {
        protected readonly EditorWindow _owner;
        protected readonly SerenityContext _context;

        protected BaseView(EditorWindow owner, SerenityContext context)
        {
            _owner = owner;
            _context = context;
        }
        
        public virtual void Draw(){}
        
        protected void Repaint()
        {
            _owner.Repaint();
        }
        
        protected static bool DrawCenteredButton(GUIContent content, float width, float rowHeight, GUIStyle style = null, float buttonHeight = 20)
        {
            var cellRect = GUILayoutUtility.GetRect(width, rowHeight, GUILayout.Width(width), GUILayout.Height(rowHeight));
            var buttonRect = new Rect(cellRect.x, cellRect.y + (rowHeight - buttonHeight) * 0.5f, width, buttonHeight);
            
            return style == null
                ? GUI.Button(buttonRect, content)
                : GUI.Button(buttonRect, content, style);
        }
        
        protected static bool DrawCenteredButton(string text, float width, float rowHeight, GUIStyle style = null, float buttonHeight = 20)
        {
            return DrawCenteredButton(new GUIContent(text), width, rowHeight, style, buttonHeight);
        }
        
        protected static bool DrawSelectionButton(bool isSelected, params GUILayoutOption[] options)
        {
            if (!isSelected)
            {
                return GUILayout.Button("Select", EditorStyles.miniButton, options);
            }
            
            var previousBackgroundColor = GUI.backgroundColor;
            var previousContentColor = GUI.contentColor;
            
            GUI.backgroundColor = new Color(0.35f, 0.75f, 0.32f);
            GUI.contentColor = Color.white;
            
            var clicked = GUILayout.Button("Selected", EditorStyles.miniButton, options);
            
            GUI.backgroundColor = previousBackgroundColor;
            GUI.contentColor = previousContentColor;
            
            return clicked;
        }
        
        protected void DrawJobStatus(SerenityJob job)
        {
            if (job == null || job.Status == SerenityJobStatus.Idle) return;
            
            GUILayout.BeginVertical("helpbox");
            EditorGUILayout.LabelField($"{job.Name}: {job.Status}");
            
            if (job.Total > 0)
            {
                var progressRect = GUILayoutUtility.GetRect(18, 18, "TextField");
                EditorGUI.ProgressBar(progressRect, job.Progress, $"{job.Current}/{job.Total} {job.CurrentItem}");
            }
            
            if (!string.IsNullOrEmpty(job.ErrorMessage))
            {
                EditorGUILayout.HelpBox(job.ErrorMessage, MessageType.Warning);
            }
            
            GUILayout.EndVertical();
        }
    }
}
