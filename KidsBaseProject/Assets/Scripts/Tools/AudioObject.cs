using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tools.Utility;
using Tools.ObjectPooling;
using Tools.AudioManager;

[RequireComponent(typeof(AudioSource))]
public class AudioObject : PoolObject
{
    [HideInInspector]
    public AudioSource audioSource;

    public AudioVolumeType audioVolumeType;

    private AudioManager audioManager => AudioManager.Instance;
    private float Volume => audioManager.GetVolume(audioVolumeType) * audioManager.GetVolume(AudioVolumeType.master);
    private float Pitch => audioManager.GetPitch(audioVolumeType);

    public AudioSource PlayAudio(AudioClip audioClip, AudioVolumeType type, bool loop = false, float fadeTime = 0.0f)
    {
        fadeTime = Mathf.Abs(fadeTime);
        audioVolumeType = type;
        if (!audioSource)
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        audioSource.clip = audioClip;
        audioSource.Play();
        audioSource.loop = loop;
        audioSource.clip = audioClip;
        audioSource.pitch = Pitch;

        if (fadeTime > 0.0f)
        {
            audioSource.volume = 0;
            this.LoopWhile(() => audioSource.volume < Volume, () => audioSource.volume += (Volume * (Time.deltaTime / fadeTime)), 0, () => audioSource.volume = Volume);
        }
        else
        {
            audioSource.volume = Volume;
        }

        if (loop == false)
        { 
            this.LoopWhile(() => audioSource.time < audioSource.clip.length, null, 0, () => StopAudio());
        }
        audioManager.VolumeChangedEvent += VolumeChanged;
        audioManager.PitchChangedEvent += PitchChanged;
        return audioSource;
    }

    public AudioSource StopAudio(float fadeTime = 0.0f)
    {
        audioManager.VolumeChangedEvent -= VolumeChanged;
        audioManager.PitchChangedEvent -= PitchChanged;

        Action stop = () =>
        {
            if (audioSource != null)
            {
                audioSource.volume = 0;
                audioSource.clip = null;
                audioSource.Stop();
            }

            Recycle();

            gameObject.name = "AudioObject(Clone)";
        };
        fadeTime = Mathf.Abs(fadeTime);
        if (fadeTime > 0.0f)
        {
            this.LoopWhile(() => audioSource.volume > 0, () => audioSource.volume -= (Volume * (Time.deltaTime / fadeTime)), 0, stop);
        }
        else
        {
            stop.Invoke();
        }
        return audioSource;
    }

    public void PauseAudio()
    {
        audioSource.Pause();
    }

    public void ResumeAudio()
    {
        audioSource.UnPause();
    }

    private void VolumeChanged(AudioVolumeType type)
    {
        audioSource.volume = Volume;
    }

    private void PitchChanged(AudioVolumeType type)
    {
        audioSource.pitch = Pitch;
    }
}
