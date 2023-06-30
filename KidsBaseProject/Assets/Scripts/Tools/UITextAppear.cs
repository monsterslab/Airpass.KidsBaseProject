using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UITextAppear : MonoBehaviour
{
    [SerializeField] float speed = 1.0f;
    TMP_Text tmpText = null;
    Text text = null;

    Color ColorAlphaChange(Color c, float value)
    {
        c.a = value;
        return c;
    }

    IEnumerator Appear()
    {
        float t = 0;
        while(t < 1.0f)
        {
            t += Time.deltaTime * speed;
            if (tmpText != null)
                tmpText.color = ColorAlphaChange(tmpText.color, t);

            if (text != null)
                text.color = ColorAlphaChange(text.color, t);

            yield return null;
        }
    }

    private void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
        if (tmpText != null)
            tmpText.color = ColorAlphaChange(tmpText.color, 0);

        text = GetComponent<Text>();
        if (text != null)
            tmpText.color = ColorAlphaChange(text.color, 0);
    }

    private void OnEnable()
    {
        StartCoroutine(Appear());
    }
}
