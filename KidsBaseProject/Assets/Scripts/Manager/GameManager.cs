using UnityEngine;
using Tools.Processor;
using Tools.Attributes;
using Tools.Utility;
using Tools.AudioManager;

/// <summary>
/// 'GameState' Enumeration is just a sample. could be changed.
/// </summary>
public class GameManager : ProcessorEternal<GameManager, GameState>
{
    [UneditableField] public float gameTime;
    public bool result = false;
    public bool isRestart = false;
    public float maxGameTime = 90.0f;
    private AudioManager audioManager => AudioManager.Instance;
    private UIManager uiManager => UIManager.Instance;

    //Use Debug
    GUIStyle style = new GUIStyle();
    private float waitTime;

    void Enable_Title()
    {
        PlatformProtocol.SendData(PlatformProtocol.cmd_Home);
        uiManager.Enable_Title();
        AudioManager.Instance.PlayBGM(AudioClipKey.Bgm1);
    }

    void Enable_Prepare()
    {
        audioManager.SetVolume(AudioVolumeType.bgm, SystemManager.Instance.bgmVolume * 0.7f);
        uiManager.Enable_Prepare();
        this.DelayToDo(audioManager.audioClips[(int)AudioClipKey.Narration1].length + 2, () =>
        {
            if(State == GameState.prepare) State = GameState.idle;
        });
    }
    void Update_Prepare()
    {        
    }

    void Enable_Idle()
    {
        PlatformProtocol.SendData(PlatformProtocol.cmd_Ready);
        uiManager.Enable_Idle();
        waitTime = 0;
    }
    void Update_Idle()
    {
        waitTime += Time.deltaTime;
        if (waitTime > 300)
        {
            Application.Quit();
        }
    }

    void Enable_Gaming()
    {
        PlatformProtocol.SendData(PlatformProtocol.cmd_Start);
        result = false;
        gameTime = 0;
        audioManager.SetVolume(AudioVolumeType.bgm, SystemManager.Instance.bgmVolume * 0.7f);
        audioManager.PlaySFX(AudioClipKey.Enter, AudioVolumeType.bgm);
    }

    void Update_Gaming()
    {
        gameTime += Time.deltaTime;
        if(gameTime >= SystemManager.Instance.timeLimit)
        {
            State = GameState.result;
        }
    }
   
    void Enable_Result()
    {
        PlatformProtocol.SendData(PlatformProtocol.cmd_End);
        isRestart = true;
        waitTime = 0;
        uiManager.Enable_Result();
    }

    void Update_Result()
    {
        waitTime += Time.deltaTime;
        if(waitTime > 62)
        {
            Application.Quit();
        }
    }

    protected override void Awake()
    {
        base.Awake();
        // Init the state for trigger Enable function.
        State = GameState.title;

        //Gui Style
        style.normal.textColor = Color.blue;
        style.fontSize = 50;
    }
}
