using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Tools.AudioManager;
public class UINarration : MonoBehaviour
{
    [SerializeField] Image img_NarrationOnOff;
    [SerializeField] Sprite sprite_NarrationOn;
    [SerializeField] Sprite sprite_NarrationOff;
    private AudioManager audioManager => AudioManager.Instance;

    bool onOff = true;

    public void NarrationOnOff()
    {
        onOff = !onOff;

        if (onOff)
        {
            audioManager.SetVolume(AudioVolumeType.Narration, SystemManager.Instance.narrationVolume);
            img_NarrationOnOff.sprite = sprite_NarrationOn;
        }

        else
        {
            audioManager.SetVolume(AudioVolumeType.Narration, 0);
            img_NarrationOnOff.sprite = sprite_NarrationOff;
        }
    }

    public void Skip()
    {
        audioManager.StopBGM(AudioClipKey.Narration1);
        if (GameManager.Instance.State == GameState.prepare) GameManager.Instance.State = GameState.idle;
    }

    IEnumerator NarrationStart()
    {
        audioManager.PlayBGM(AudioClipKey.Narration1, 0, false, AudioVolumeType.Narration);

        yield return null;
    }

    private void OnEnable()
    {
        StartCoroutine(NarrationStart());
    }
}
