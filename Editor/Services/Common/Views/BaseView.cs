using UnityEditor;

namespace SerenityAITranslator.Editor.Services.Common.Views
{
    public class BaseView
    {
        protected readonly EditorWindow _owner;

        protected BaseView(EditorWindow owner)
        {
            _owner = owner;
        }
        
        public virtual void Draw(){}
        
        protected void Repaint()
        {
            _owner.Repaint();
        }
    }
}