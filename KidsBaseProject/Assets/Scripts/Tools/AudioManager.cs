using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tools.Singletons;
using Tools.Utility;
using Tools.ObjectPooling;
using Tools.Attributes;
using System;

namespace Tools.AudioManager
{
    public class AudioManager : SingletonUnityEternal<AudioManager>
    {
        private const string playerPrefsVolumeKey = "volume";

        [UneditableField] public List<AudioClip> audioClips = new List<AudioClip>();
        private Dictionary<AudioVolumeType, float> volumes = new Dictionary<AudioVolumeType, float>();
        private Dictionary<AudioVolumeType, float> pitchs = new Dictionary<AudioVolumeType, float>();

        public event Action<AudioVolumeType> VolumeChangedEvent;
        public event Action<AudioVolumeType> PitchChangedEvent;

        private ObjectPool obp_bgm;
        private ObjectPool obp_sfx;
        private GameObject audioObject;

        public float GetPitch(AudioVolumeType type)
        {
            if (pitchs.ContainsKey(type))
                return pitchs[type];
            else
                return 1;
        }
        public void SetPitch(AudioVolumeType type, float value)
        {
            pitchs[type] = value;
            PitchChangedEvent(type);
        }

        public float GetVolume(AudioVolumeType type)
        {
            return volumes[type];
        }

        public void SetVolume(AudioVolumeType type, float value)
        {
            volumes[type] = value;
            VolumeChangedEvent(type);
        }

        public bool IsBGMPlaying(AudioClipKey clipKey)
        {
            if (obp_bgm.ContainsActivedObject(clipKey.ToString()))
            {
                return obp_bgm.GetObject(clipKey.ToString()).GetComponent<AudioSource>().isPlaying;
            }
            else
            {
                return false;
            }
        }

        public AudioSource PlaySFX(AudioClipKey clipKey, AudioVolumeType type = AudioVolumeType.sfx)
        {
            return obp_sfx.GetObject().GetComponent<AudioObject>().PlayAudio(audioClips[(int)clipKey], type);
        }

        public AudioSource PlaySFX(string id, AudioClip audioClip, AudioVolumeType type = AudioVolumeType.sfx)
        {
            return obp_sfx.GetObject(id).GetComponent<AudioObject>().PlayAudio(audioClip, type);
        }

        public AudioSource StopSFX(string id)
        {
            return obp_sfx.GetObject(id).GetComponent<AudioObject>().StopAudio();
        }

        public void PlayBGM(AudioClipKey clipKey, float fade = 0.0f, bool loop = true, AudioVolumeType type = AudioVolumeType.bgm)
        {
            obp_bgm.GetObject(clipKey.ToString()).GetComponent<AudioObject>().PlayAudio(audioClips[(int)clipKey], type, loop, fade);
        }

        public void StopBGM(AudioClipKey clipKey, float fade = 0.0f)
        {
            obp_bgm.GetObject(clipKey.ToString()).GetComponent<AudioObject>().StopAudio(fade);
        }

        public void PauseBGM(AudioClipKey clipKey)
        {
            if (obp_bgm.ContainsActivedObject(clipKey.ToString()))
            {
                obp_bgm.GetObject(clipKey.ToString()).GetComponent<AudioObject>().PauseAudio();
            }
        }

        public void ResumeBGM(AudioClipKey clipKey)
        {
            if (obp_bgm.ContainsActivedObject(clipKey.ToString()))
            {
                obp_bgm.GetObject(clipKey.ToString()).GetComponent<AudioObject>().ResumeAudio();
            }
        }

        private ObjectPool InitializeObjectPool(string name)
        {
            ObjectPool obp = new GameObject(name).AddComponent<ObjectPool>();
            obp.Initialize(audioObject);
            obp.transform.SetParent(transform);
            return obp;
        }

        protected override void Awake()
        {
            base.Awake();
            audioObject = new GameObject("AudioObject").AddComponent<AudioObject>().gameObject;
            audioObject.transform.SetParent(transform);
            // Load audio volume.
            foreach (AudioVolumeType type in Enum.GetValues(typeof(AudioVolumeType)))
            {
                string key = $"{playerPrefsVolumeKey}_{type}";
                if (PlayerPrefs.HasKey(key))
                {
                    volumes.Add(type, PlayerPrefs.GetFloat(key));
                }
                else
                {
                    volumes.Add(type, 1.0f);
                }
            }
            // Create ObjectPools for AudioSource.
            obp_bgm = InitializeObjectPool("Obp_BGM");
            obp_sfx = InitializeObjectPool("Obp_SFX");
        }
    }
}