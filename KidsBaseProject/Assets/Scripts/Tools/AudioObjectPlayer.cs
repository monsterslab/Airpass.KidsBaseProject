using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tools.AudioManager;

public class AudioObjectPlayer : MonoBehaviour
{
    public AudioVolumeType type;
    public AudioClip clip;

    public virtual AudioSource PlayAudio(AudioClip clip)
    {
        this.clip = clip ?? this.clip;
        return AudioManager.Instance.PlaySFX($"{gameObject.GetInstanceID()}{clip.name}", clip, type);
    }

    public virtual void StopAudio()
    {
        AudioManager.Instance.StopSFX($"{gameObject.GetInstanceID()}{clip.name}");
    }
}
