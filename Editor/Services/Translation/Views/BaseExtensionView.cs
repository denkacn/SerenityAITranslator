using UnityEditor;

namespace SerenityAITranslator.Editor.Services.Translation.Views
{
    public class BaseExtensionView
    {
        private readonly EditorWindow _owner;

        protected BaseExtensionView(EditorWindow owner)
        {
            _owner = owner;
        }
        
        protected void Repaint()
        {
            _owner.Repaint();
        }
    }
}