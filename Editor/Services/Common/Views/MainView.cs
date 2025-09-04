using SerenityAITranslator.Editor.Context;
using UnityEditor;

namespace SerenityAITranslator.Editor.Services.Common.Views
{
    public abstract class MainView : BaseView
    {
        protected MainView(EditorWindow owner, SerenityContext context) : base(owner, context){}

        public abstract void Init();
    }
}