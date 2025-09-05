using System.IO;
using UnityEditor;
using UnityEngine;

namespace SerenityAITranslator.Editor.Tools
{
    public static class AssetsUtility
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
        
        public static T LoadFromResources<T>(string assetPath) where T : ScriptableObject
        {
            return Resources.Load<T>(assetPath);
        }

        public static Texture2D LoadIcon(string iconName)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/SerenityAITranslator/Editor/Icons/" + iconName);
        }
    }
}