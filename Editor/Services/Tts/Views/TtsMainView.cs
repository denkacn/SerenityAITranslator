using SerenityAITranslator.Editor.Context;
using SerenityAITranslator.Editor.Services.Common.Views;
using SerenityAITranslator.Editor.Services.Translation.SourceAssetProvider;
using UnityEditor;

namespace SerenityAITranslator.Editor.Services.Tts.Views
{
    public class TtsMainView : MainView
    {
        private SourceAssetProviderView _sourceAssetProviderView;
        private TtsSettingsButtonView _ttsSettingsButtonView;
        private TtsTermsView _termsView;
        
        public TtsMainView(EditorWindow owner, SerenityContext context) : base(owner, context){}

        public override void Init()
        {
            _sourceAssetProviderView = new SourceAssetProviderView(_owner, _context);
            _sourceAssetProviderView.Init(provider => Setup(provider));
        }
        
        public override void Draw()
        {
            _sourceAssetProviderView?.Draw();
            _ttsSettingsButtonView?.Draw();
            _termsView?.Draw();
        }

        private void Setup(ISourceAssetProvider sourceAssetProvider)
        {
            _ttsSettingsButtonView = new TtsSettingsButtonView(_owner, sourceAssetProvider, _context);
            _termsView = new TtsTermsView(_owner, sourceAssetProvider, _context);
        }
    }
}