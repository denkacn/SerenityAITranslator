using System.Reflection;
using UnityEditor.PackageManager;

namespace SerenityAITranslator.Editor.Services.Settings
{
    public static class SerenityPackagePaths
    {
        private const string AssetFallbackRoot = "Assets/SerenityAITranslator";

        public static readonly string Root = ResolveRoot();
        public static readonly string SettingsRoot = Root + "/Settings";
        public static readonly string IconsRoot = Root + "/Editor/Icons";

        private static string ResolveRoot()
        {
            var packageInfo = PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly());
            return string.IsNullOrEmpty(packageInfo?.assetPath) ? AssetFallbackRoot : packageInfo.assetPath;
        }
    }
}
