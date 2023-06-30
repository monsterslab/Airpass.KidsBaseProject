using System.Collections;
using UnityEngine;
using Tools.AudioManager;
public class UIResult : MonoBehaviour
{
    [SerializeField]
    GameObject obj_Fail;
    [SerializeField]
    GameObject obj_Success;
    [SerializeField]
    GameObject obj_Record;

    private void OnEnable()
    {
        StartCoroutine(ResultAnimation());
    }

    IEnumerator ResultAnimation()
    {
        yield return new WaitForSeconds(2.0f);
        obj_Success.SetActive(GameManager.Instance.result);
        obj_Fail.SetActive(!GameManager.Instance.result);
        
        if(GameManager.Instance.result)
        {
            AudioManager.Instance.SetVolume(AudioVolumeType.bgm, SystemManager.Instance.bgmVolume * 0.7f);
            yield return new WaitForSeconds(7.0f);
            obj_Record.SetActive(true);
        }
    }
}
