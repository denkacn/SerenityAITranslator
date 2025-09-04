using System.IO;
using UnityEditor;
using UnityEngine;

namespace SerenityAITranslator.Editor.Tools
{
    public static class ScriptableObjectUtility
    {
        public static T LoadOrCreate<T>(string assetPath) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            if (asset == null)
            {
                var dir = Path.GetDirectoryName(assetPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                
                asset = ScriptableObject.CreateInstance<T>();
                
                AssetDatabase.CreateAsset(asset, assetPath);
                AssetDatabase.SaveAssets();
            }
            else
            {
                Debug.Log($"[ScriptableObjectUtility] Loaded existing {typeof(T).Name} from {assetPath}");
            }

            return asset;
        }
    }
}