using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIStart : MonoBehaviour
{
    [SerializeField] GameObject obj_btnStart;
    private void OnEnable()
    {
        obj_btnStart.gameObject.SetActive(true);
    }
}
