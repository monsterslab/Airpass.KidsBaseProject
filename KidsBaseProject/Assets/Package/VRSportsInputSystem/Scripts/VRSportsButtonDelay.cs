using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AirpassUnity.VRSports;
using UnityEngine.EventSystems;

public class VRSportsButtonDelay : VRSportsButton
{
    float delay = 0.2f;
    float maxDelay = 0.1f;
    override public void Interact(bool isExit = false)
    {
        if (!isExit && delay < maxDelay) return;

        delay = 0.0f;
        base.Interact(isExit);
    }

    override protected void Update()
    {
        if(delay < maxDelay)
            delay += Time.deltaTime;

        base.Update();
    }
}
