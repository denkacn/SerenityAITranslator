namespace SerenityAITranslator.Editor.Services.Tts.Models
{
    public class TtsPromtData
    {
        public string Text { get; set; }
        public string Language { get; set; }
        public string Path { get; set; }
        
        public TtsPromtData(){}
        
        public TtsPromtData(string text, string language, string path)
        {
            Text = text;
            Language = language;
            Path = path;
        }
    }
}