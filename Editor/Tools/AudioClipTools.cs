using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SerenityAITranslator.Editor.Tools
{
    public static class AudioClipTools
    {
        private static readonly Type AudioUtilType;
        private static readonly MethodInfo _isPreviewClipPlayingInfo;
        private static readonly MethodInfo _playPreviewClip;
        private static readonly MethodInfo _stopAllPreviewClips;
        
        static AudioClipTools()
        {
            AudioUtilType = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
            
            _isPreviewClipPlayingInfo = AudioUtilType.GetMethod(
                "IsPreviewClipPlaying",
                BindingFlags.Static | BindingFlags.Public);
            
            _playPreviewClip = AudioUtilType.GetMethod(
                "PlayPreviewClip",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
                null
            );
            
            _stopAllPreviewClips = AudioUtilType.GetMethod(
                "StopAllPreviewClips",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new Type[] { },
                null
            );
        }

        public static void Play(AudioClip clip, int startSample = 0, bool loop = false)
        {
            if (clip == null) return;
            
            _playPreviewClip.Invoke(
                null,
                new object[] { clip, startSample, loop }
            );
        }

        public static void StopAll()
        {
            _stopAllPreviewClips.Invoke(
                null,
                new object[] { }
            );
        }

        public static bool IsPlaying() =>
            _isPreviewClipPlayingInfo != null && (bool)_isPreviewClipPlayingInfo.Invoke(null, null);
    }
}