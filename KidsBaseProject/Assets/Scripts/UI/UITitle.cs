using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITitle : MonoBehaviour
{
    IEnumerator CoroutineAnimation()
    {
        yield return new WaitForSeconds(2.0f);
        GameManager.Instance.State = GameState.prepare;
    }
    private void Start()
    {
        StartCoroutine(CoroutineAnimation());
    }
}
