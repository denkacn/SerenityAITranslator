using System;
using SerenityAITranslator.Editor.Services.Managers;
using SerenityAITranslator.Editor.Windows;
using UnityEditor;
using UnityEngine;

namespace SerenityAITranslator.Editor.Services
{
    [Serializable]
    public class SerenityAIInitiator
    {
        private static SerenityAIManager _manager;
        
        [MenuItem("Tools/Serenity AI")]
        public static void OpenSerenityAiWindow()
        {
            _manager = new SerenityAIManager();
            _ = _manager.Init();
        }
        
        [InitializeOnLoadMethod]
        private static void Init()
        {
            Debug.Log("Init");

            if (EditorWindow.HasOpenInstances<SerenityAiWindow>())
            {
                _ = new SerenityAIManager().Init();
            }
            
            //EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        /*private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                Debug.Log("OnPlayModeStateChanged _manager: " + _manager);
            }
        }*/
    }
}