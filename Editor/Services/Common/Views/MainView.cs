using UnityEditor;

namespace SerenityAITranslator.Editor.Services.Common.Views
{
    public abstract class MainView : BaseView
    {
        protected MainView(EditorWindow owner) : base(owner){}

        public abstract void Init();
    }
}