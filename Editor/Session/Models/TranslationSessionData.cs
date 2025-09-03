namespace SerenityAITranslator.Editor.Session.Models
{
    public class TranslationSessionData
    {
        public string SourceLanguage { get; set; }
        public string DestinationLanguage { get; set; }
        public string ProviderId { get; set; }
        public string SelectedPromt { get; set; }
        public bool IsShowInfoView { get; set; } = true;
        public bool IsShowSettingView { get; set; } = true;
    }
}