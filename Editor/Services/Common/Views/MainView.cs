using SerenityAITranslator.Editor.Services.Managers;
using UnityEditor;

namespace SerenityAITranslator.Editor.Services.Common.Views
{
    public abstract class MainView : BaseView
    {
        protected ISerenityAIManager _manager;

        protected MainView(EditorWindow owner, ISerenityAIManager manager) : base(owner)
        {
            _manager = manager;
        }

        public abstract void Init();
    }
}