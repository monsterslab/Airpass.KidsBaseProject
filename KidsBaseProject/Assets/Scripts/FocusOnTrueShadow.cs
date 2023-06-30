using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LeTai.TrueShadow;
using UnityEngine.EventSystems;
using Tools.AudioManager;

public class FocusOnTrueShadow : MonoBehaviour
{
    TrueShadow trueShadow;
    EventTrigger eventTrigger;
    Coroutine cor_scaleAnimation;
    private void Awake()
    {
        trueShadow = GetComponent<TrueShadow>();
        if(trueShadow == null) trueShadow = transform.parent.GetComponentInChildren<TrueShadow>();
        eventTrigger = GetComponent<EventTrigger>();
        if (eventTrigger == null) eventTrigger = transform.parent.GetComponentInChildren<EventTrigger>();
        if (eventTrigger == null) eventTrigger = gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Select;
        entry.callback.AddListener((data) => Enable());
        eventTrigger.triggers.Add(entry);

        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Deselect;
        entry.callback.AddListener((data) => Disable());
        eventTrigger.triggers.Add(entry);
    }

    public void Enable()
    {
        trueShadow.enabled = true; 
        cor_scaleAnimation = StartCoroutine(ScaleAnimation(Vector3.one * 1.2f));
    }
    public void Disable()
    {
        trueShadow.enabled = false;
        if (cor_scaleAnimation != null)
        {
            StopCoroutine(cor_scaleAnimation);
            cor_scaleAnimation = null;
        }
        transform.localScale = Vector3.one;
    }

    IEnumerator ScaleAnimation(Vector3 dest)
    {
        float t = 0;
        float sinN = 0;
        float speed = 5.0f;
        Vector3 start = transform.localScale;
        while (t < 1)
        {
            sinN += Time.deltaTime * speed;
            t = Mathf.Sin(sinN);
            transform.localScale = Vector3.Lerp(start, dest, t);
            yield return null;

            if (sinN > Mathf.PI * 0.5f)
            {
                transform.localScale = dest;
                break;
            }
        }
    }

    private void OnDisable()
    {
        Disable();
    }
}
