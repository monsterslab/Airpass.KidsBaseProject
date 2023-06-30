using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AirpassUnity.VRSports;

public class VRSportsHoldButtonAudioPlayer : AudioObjectPlayer
{
    public AudioClip holdingClip;
    public AudioClip holdedClip;

    private void Awake()
    {
        if (TryGetComponent(out VRSportsButton vb))
        {
            if (vb.type == ButtonType.hold)
            {
                vb.OnHoldBegin.AddListener(() => PlayAudio(holdingClip));
                vb.OnHoldLost.AddListener(() =>
                {
                    StopAudio();
                });
                vb.OnHolded.AddListener(() =>
                {
                    StopAudio();
                    PlayAudio(holdedClip);
                });
            }
        }
    }

    public override AudioSource PlayAudio(AudioClip clip)
    {
        AudioSource @as = base.PlayAudio(clip);
        if (clip == holdingClip)
        { 
            @as.pitch = holdingClip.length / VRSportsInputSystem.Instance.holdDelayOfVRSportsButton;
        }
        return @as;
    }
}
