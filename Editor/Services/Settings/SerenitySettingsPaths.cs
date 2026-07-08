namespace SerenityAITranslator.Editor.Services.Settings
{
    public static class SerenitySettingsPaths
    {
        public const string ProjectDataRoot = "Assets/SerenityAIData";
        public const string ProjectEditorSettingsRoot = ProjectDataRoot + "/Editor/SerenityAi";
        public const string PackageSettingsRoot = "Assets/SerenityAITranslator/Settings";
        public const string LegacyEditorSettingsRoot = "Assets/Editor/SerenityAi";

        public const string Session = ProjectEditorSettingsRoot + "/Session.asset";
        public const string TranslatePromtSettings = ProjectEditorSettingsRoot + "/PromtSettings.asset";
        public const string TranslateProvidersConfigurations = ProjectEditorSettingsRoot + "/TranslateProvidersConfigurations.asset";
        public const string TtsProvidersConfiguration = ProjectEditorSettingsRoot + "/TtsProvidersConfiguration.asset";
        public const string TtsPromtSettings = ProjectEditorSettingsRoot + "/TtsPromtSettings.asset";
        public const string LanguageConverterData = ProjectEditorSettingsRoot + "/LanguageConverterData.asset";
        public const string VoicesLibrary = ProjectDataRoot + "/VoicesLibrary/VoicesLibrary.asset";

        public const string TranslateProvidersSetting = PackageSettingsRoot + "/TranslateProvidersSetting.asset";

        public const string LegacySession = LegacyEditorSettingsRoot + "/Session.asset";
        public const string LegacyTranslatePromtSettings = LegacyEditorSettingsRoot + "/PromtSettings.asset";
        public const string LegacyTranslateProvidersConfigurations = LegacyEditorSettingsRoot + "/TranslateProvidersConfigurations.asset";
        public const string LegacyTtsProvidersConfiguration = LegacyEditorSettingsRoot + "/TtsProvidersConfiguration.asset";
        public const string LegacyTtsPromtSettings = LegacyEditorSettingsRoot + "/TtsPromtSettings.asset";
        public const string LegacyLanguageConverterData = LegacyEditorSettingsRoot + "/LanguageConverterData.asset";
    }
}
