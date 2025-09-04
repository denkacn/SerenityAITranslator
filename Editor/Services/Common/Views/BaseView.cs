using SerenityAITranslator.Editor.Context;
using UnityEditor;

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
    }
}