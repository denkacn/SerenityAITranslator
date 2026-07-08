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
