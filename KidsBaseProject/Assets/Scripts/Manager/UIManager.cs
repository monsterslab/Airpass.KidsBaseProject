using System;
using Tools.AudioManager;
using Tools.Singletons;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : SingletonUnity<UIManager>
{
    [SerializeField] GameObject obj_DefaultSelect;

    [Header("Title")]
    [SerializeField] GameObject obj_Title;

    [Header("Game")]
    [SerializeField] GameObject obj_System;
    [SerializeField] GameObject obj_FunDescript;
    [SerializeField] GameObject pnl_Start;
    [SerializeField] GameObject pnl_Game;
    [SerializeField] GameObject pnl_Result;

    public UIResult uiResult { get; private set; }
    public void StateChange(string value)
    {
        GameManager.Instance.State = (GameState)Enum.Parse(typeof (GameState), value);
    }

    public void MuteNarration()
    {
        AudioManager.Instance.SetVolume(AudioVolumeType.Narration, 0);
    }

    public void Enable_Title()
    {
        obj_Title.SetActive(true);
    }

    public void Enable_Prepare()
    {
        obj_Title.SetActive(false);
        obj_FunDescript.SetActive(true);
    }
    public void Enable_Idle()
    {
        obj_FunDescript.SetActive(false);
        pnl_Start.SetActive(true);
        pnl_Game.SetActive(true);
    }
    public void Enable_Result()
    {
        pnl_Game.SetActive(false);
        pnl_Result.SetActive(true);
    }
    public void SetSelectNoneGameObject()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void NavigationOn(Selectable selectable)
    {
        Navigation nav = selectable.navigation;
        nav.mode = Navigation.Mode.Automatic;
        selectable.navigation = nav;
    }

    public void NavigationOff(Selectable selectable)
    {
        Navigation nav = selectable.navigation;
        nav.mode = Navigation.Mode.None;
        selectable.navigation = nav;
    }

    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow) ||
                        Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Return))
            {
                EventSystem.current.SetSelectedGameObject(obj_DefaultSelect);
            }
        }
    }
}
