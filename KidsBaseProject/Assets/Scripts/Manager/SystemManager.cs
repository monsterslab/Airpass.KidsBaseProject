using System.Collections.Generic;
using Tools.AudioManager;
using Tools.ObjectPooling;
using Tools.Singletons;
using UnityEngine;
using UnityEngine.UI;

public class SystemManager : SingletonUnityEternal<SystemManager>
{
    [SerializeField] private GameObject pnl_Option;
    [SerializeField] private GameObject pnl_Option2;
    [SerializeField] private ObjectPool obj_TimeImg;
    [SerializeField] private Image img_TimeBar;
    [SerializeField] private GameObject obj_TimeBar;
    [SerializeField] private Text txt_MaxTime;
    [SerializeField] private List<Sprite> img_selectBtn;
    [SerializeField] private Image[] volBtns;
    [SerializeField] private Image[] BGMBtns;
    [SerializeField] private Image[] SpeedBtns;
    [SerializeField] private Image[] TimeBtns;
    [SerializeField] private Button btn_skip;
    [SerializeField] private Button btn_sound;

    public float narrationVolume = 0.5f;
    public float bgmVolume = 0.5f;
    public float timeScale = 1.0f;
    public float timeLimit;
    private int maxIndex;
    Button[] optionButtons;
    public void SetTimeBar(float time)
    {
        maxIndex = (int)(time / 10);

        switch (maxIndex)
        {
            case 8:
                obj_TimeBar.GetComponent<RectTransform>().sizeDelta = new Vector2(-35, 0);
                break;
            case 9:
                obj_TimeBar.GetComponent<RectTransform>().sizeDelta = new Vector2(25, 0);
                break;
            case 12:
                obj_TimeBar.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 0);
                break;
        }
        timeLimit = time;
        txt_MaxTime.text = timeLimit.ToString();

        for (int i = 0; i < obj_TimeImg.initPoolSize; ++i)
        {
            GameObject obj1 = obj_TimeImg.GetObject(i.ToString()).gameObject;

            if (i <= maxIndex)
            {
                obj1.SetActive(true);

                if (i == maxIndex)
                {
                    obj1.SetActive(false);
                }
            }
            else
            {
                obj1.SetActive(false);
            }
        }

        FlowTime((timeLimit - GameManager.Instance.gameTime) / timeLimit);
    }

    void FlowTime(float percentage)
    {
        float value = maxIndex * percentage;
        for (int i = maxIndex - 1; i > -1; --i)
        {
            if (i == (int)value)
            {
                obj_TimeImg.GetObject(i.ToString()).GetComponent<Image>().fillAmount = value % 1;
            }
            else if (i > value)
            {
                obj_TimeImg.GetObject(i.ToString()).GetComponent<Image>().fillAmount = 0;
            }
            else if (i < value)
            {
                obj_TimeImg.GetObject(i.ToString()).GetComponent<Image>().fillAmount = 1;
            }
        }
    }

    public void ClickHome()
    {
        Application.Quit();
    }

    public void Pnl_OptionChangeActive()
    {
        bool move = pnl_Option2.GetComponent<Animator>().GetBool("move");
        pnl_Option2.GetComponent<Animator>().SetBool("move", !move);
        pnl_Option.SetActive(!move);

        SetButtonNavigation(!move);
        if (!move)
        {
            UIManager.Instance.NavigationOff(btn_skip);
            UIManager.Instance.NavigationOff(btn_sound);
            GamePause();
        }
        else
        {
            UIManager.Instance.NavigationOn(btn_skip);
            UIManager.Instance.NavigationOn(btn_sound);
            GamePlay();
        }

    }
    public void TimeLimitChange(int value)
    {
        switch (value)
        {
            case 0:
                timeLimit = 80f;
                break;
            case 1:
                timeLimit = 90f;
                break;
            case 2:
                timeLimit = 120f;
                break;
        }

        GameManager.Instance.maxGameTime = timeLimit;
        SetTimeBar(timeLimit);

        for (int i = 0; i < TimeBtns.Length; i++)
            TimeBtns[i].sprite = img_selectBtn[0];
        TimeBtns[value].sprite = img_selectBtn[1];
    }
    public void TimeScaleChange(int value)
    {
        switch (value)
        {
            case 0:
                timeScale = 0.7f;
                break;
            case 1:
                timeScale = 1.0f;
                break;
            case 2:
                timeScale = 1.2f;
                break;
        }

        if (Time.timeScale != 0)
        {
            Time.timeScale = timeScale;
            AudioManager.Instance.SetPitch(AudioVolumeType.Narration, Time.timeScale);
        }

        for (int i = 0; i < TimeBtns.Length; i++)
            SpeedBtns[i].sprite = img_selectBtn[0];
        SpeedBtns[value].sprite = img_selectBtn[1];
    }
    public void NarrationVolume(int value)
    {
        AudioManager.instance.SetVolume(AudioVolumeType.Narration, value * 0.5f);
        narrationVolume = AudioManager.instance.GetVolume(AudioVolumeType.Narration);

        for (int i = 0; i < TimeBtns.Length; i++)
            volBtns[i].sprite = img_selectBtn[0];
        volBtns[value].sprite = img_selectBtn[1];

    }
    public void BgmVolume(int value)
    {
        AudioManager.instance.SetVolume(AudioVolumeType.bgm, value * 0.5f);
        bgmVolume = AudioManager.instance.GetVolume(AudioVolumeType.bgm);

        for (int i = 0; i < TimeBtns.Length; i++)
            BGMBtns[i].sprite = img_selectBtn[0];
        BGMBtns[value].sprite = img_selectBtn[1];
    }

    public void GamePause()
    {
        Time.timeScale = 0;
        AudioManager.Instance.SetPitch(AudioVolumeType.Narration, Time.timeScale);
    }

    public void GamePlay()
    {
        Time.timeScale = timeScale;
        AudioManager.Instance.SetPitch(AudioVolumeType.Narration, Time.timeScale);
    }

    private void SetButtonNavigation(bool value)
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            Navigation nav = optionButtons[i].navigation;
            if (value) nav.mode = Navigation.Mode.Automatic;
            else nav.mode = Navigation.Mode.None;
            optionButtons[i].navigation = nav;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        SetTimeBar(GameManager.Instance.maxGameTime);
        optionButtons = pnl_Option2.GetComponentsInChildren<Button>();
        SetButtonNavigation(false);
        pnl_Option.SetActive(false);

        NarrationVolume(1);
        BgmVolume(1);

        Cursor.visible = false;
#if UNITY_EDITOR
        Cursor.visible = true;
#endif
    }
    private void FixedUpdate()
    {
        FlowTime((timeLimit - GameManager.Instance.gameTime) / timeLimit);
    }
}