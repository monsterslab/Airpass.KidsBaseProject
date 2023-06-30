//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class AudioPlayer : MonoBehaviour
//{
//    public string audioSourceID;    // using while bgm playing for a specific audioSource
//    public AudioClip audioClip;
//    public bool loop = false;
//    public bool autoPlay = false;

//    public void Play_SFX()
//    {
//        AudioManager.Instance.PlaySFX(audioClip);
//    }

//    public void Play_BGM()
//    {
//        AudioManager.Instance.PlayBGM(audioSourceID, audioClip);
//    }

//    public void Play_SFX(AudioClip _audioClip)
//    {
//        AudioManager.Instance.PlaySFX(_audioClip);
//    }

//    public void Play_BGM(AudioClip _audioClip)
//    {
//        AudioManager.Instance.PlayBGM(audioSourceID, _audioClip);
//    }

//    public void Stop_BGM()
//    {
//        AudioManager.Instance.StopBGM(audioSourceID);
//    }

//    void OnEnable()
//    {
//        if (autoPlay)
//        {
//            if (loop)
//            {
//                Play_BGM();
//            }
//            else
//            {
//                Play_SFX();
//            }
//        }
//    }
//}
