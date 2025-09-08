using System.IO;
using UnityEngine;

namespace SerenityAITranslator.Editor.Tools
{
    public static class PathUtils
    {
        public static string GetFullDirectory(string assetPath)
        {
            var dirRelative = Path.GetDirectoryName(assetPath);
            var fullDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", dirRelative));

            return fullDir;
        }

        public static string GetDirectoryName(string assetPath)
        {
            return Path.GetDirectoryName(assetPath);
        }
    }
}