using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AirpassUnity.VRSports;

public class VRSportsButtonEnable : MonoBehaviour
{
    Button button;
    [SerializeField] Canvas canvas;
    [SerializeField] Sprite enableSprite;
    [SerializeField] Sprite disableSprite;
    bool isOn = true;

    [ContextMenu("Remove all holded vrsports button's audioclip")]
    public void RemoveAudio__()
    {
        foreach (var t in FindObjectsOfType(typeof(VRSportsButton)))
        {
            VRSportsButton vb = (t as VRSportsButton);
            if (vb.type == ButtonType.hold)
            {
                vb.sfxInteracted = null;
            }
        }
    }

    public void Change()
    {
        isOn = !isOn;

        if(isOn)
        {
            button.image.sprite = enableSprite;
        }
        else
        {
            button.image.sprite = disableSprite;
        }
    }

    private void Awake()
    {
        isOn = true;
        button = GetComponent<Button>();
        button.onClick.AddListener(Change);
    }
}
