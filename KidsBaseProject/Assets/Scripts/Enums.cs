using System;

#region ProjectTemplate basic utility enums.
namespace Tools.Attributes
{
    public enum ShowOnlyOption
    {
        always,
        editMode,
        playMode
    }
}

namespace Tools.AudioManager
{
    // Keep master, bgm, sfx exsit cause AudioManager class is referencing it.s
    public enum AudioVolumeType
    {
        master,
        bgm,
        sfx,
        Narration
    }
}
#endregion

[Serializable]
public enum GameState
{
    title,
    prepare,
    idle,
    gaming,
    result
}

public enum AudioClipKey
{
	Bgm1,
	Clocksingle,
	Enter,
	Holded,
	Holding,
	Mission,
	Narration1,
	Narration2,
	Whoosh5,
}