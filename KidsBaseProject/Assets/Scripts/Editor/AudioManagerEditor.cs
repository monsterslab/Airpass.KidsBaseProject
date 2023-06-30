using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Tools.Utility;

namespace Tools.AudioManager
{
    [CustomEditor(typeof(AudioManager))]
    public class AudioManagerEditor : Editor
    {
        private const string defineEnumName = "AudioClipKey";
        [ContextMenu(@"Load AudioClips")]
        /// <summary>
        /// Load Audio Clip resources from 'Resources' folder. List name to 'AudioClipList' enum.
        /// </summary>
        public void LoadAudioClipsFromResources(AudioManager audioManager)
        {
            audioManager.audioClips.Clear();
            string scriptPath = Path.Combine(Application.dataPath, "Scripts", "Enums.cs");
            string script = File.ReadAllText(scriptPath);
            string enumDefination = $"public enum {defineEnumName}";
            int indexOfDefinationStart;
            if (script.Contains(enumDefination))
            {
                indexOfDefinationStart = script.IndexOf(enumDefination);
                int indexofDefinationEnd = script.IndexOf('}', indexOfDefinationStart);
                script = script.Remove(indexOfDefinationStart, indexofDefinationEnd - indexOfDefinationStart + 1);
            }
            else
            {
                script += "\n";
                indexOfDefinationStart = script.Length;
            }
            string newEnumDefination = $"public enum {defineEnumName}\n" + '{';
            AudioClip[] audioClips = Resources.LoadAll<AudioClip>("AudioClips");
            for (int i = 0; i < audioClips.Length; ++i)
            {
                if (audioClips[i].name.IsScripatableNaming())
                {
                    audioManager.audioClips.Add(audioClips[i]);
                    newEnumDefination += $"\n\t{audioClips[i].name},";
                }
                else
                {
                    Debug.LogError($"AudioClip name is illegal. Please remove special character or check is start with Alphabet or not.\nClip : {audioClips[i].name}");
                }
            }
            newEnumDefination += "\n}";
            script = script.Insert(indexOfDefinationStart, newEnumDefination);
            File.WriteAllText(scriptPath, script);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Reload AudioClips"))
            {
               LoadAudioClipsFromResources((AudioManager)target);
            }
        }
    }
}