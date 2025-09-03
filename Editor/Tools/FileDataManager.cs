using System.Threading.Tasks;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace SerenityAITranslator.Editor.Tools
{
    public static class FileDataManager
    {
        private static string BasePath => Path.Combine(Application.dataPath, "Editor/SerenityAi");
        
        public static async Task<bool> SaveJsonAsync<T>(T data, string fileName)
        {
            try
            {
                if (!Directory.Exists(BasePath))
                {
                    Directory.CreateDirectory(BasePath);
                    AssetDatabase.Refresh();
                }

                var filePath = Path.Combine(BasePath, $"{fileName}");
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);

                await File.WriteAllTextAsync(filePath, json);
                AssetDatabase.ImportAsset(filePath);

                Debug.Log($"[JSON Manager] The file is successfully saved: {fileName}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JSON Manager] Error while saving a file {fileName}: {ex.Message}");
                return false;
            }
        }
        
        public static async Task<T> LoadJsonAsync<T>(string fileName)
        {
            try
            {
                var filePath = Path.Combine(BasePath, $"{fileName}");
                
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"[JSON Manager] File not found: {fileName}");
                    return default(T);
                }

                var json = await File.ReadAllTextAsync(filePath);
                T data = JsonConvert.DeserializeObject<T>(json);

                Debug.Log($"[JSON Manager] The file is successfully uploaded: {fileName}");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JSON Manager] File loading error {fileName}: {ex.Message}");
                return default(T);
            }
        }
        
        public static bool FileExists(string fileName)
        {
            var filePath = Path.Combine(BasePath, $"{fileName}");
            return File.Exists(filePath);
        }
        
        public static bool DeleteFile(string fileName)
        {
            try
            {
                var filePath = Path.Combine(BasePath, $"{fileName}");

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    AssetDatabase.Refresh();
                    Debug.Log($"[JSON Manager] The file is deleted: {fileName}");
                    return true;
                }

                Debug.LogWarning($"[JSON Manager] The file is not found for deleting: {fileName}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JSON Manager] File deletion error {fileName}: {ex.Message}");
                return false;
            }
        }
        
        public static List<string> GetAllJsonFiles()
        {
            var files = new List<string>();

            try
            {
                if (Directory.Exists(BasePath))
                {
                    var filePaths = Directory.GetFiles(BasePath, "*.json");
                    foreach (var filePath in filePaths)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(filePath);
                        files.Add(fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JSON Manager] Error when receiving a list of files: {ex.Message}");
            }

            return files;
        }
    }
}