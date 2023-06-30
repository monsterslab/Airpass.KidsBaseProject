using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGame : MonoBehaviour
{
    public void Clear()
    {
        GameManager.Instance.result = true;
        GameManager.Instance.State = GameState.result;
    }
}
