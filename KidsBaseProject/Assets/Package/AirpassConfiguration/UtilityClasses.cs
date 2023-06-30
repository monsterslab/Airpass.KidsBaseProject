using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using AC = AirpassUnity.Configuration.AirpassConfiguration;

namespace AirpassUnity.Configuration
{
    #region Ini Parser
    public class IniDatas
    {
        public Dictionary<string, IniSection> data;

        public IniSection this[string _key]
        {
            get { return data[_key]; }
            set { data[_key] = value; }
        }

        public void Add(IniSection _section)
        {
            data.Add(_section.name, _section);
        }

        public bool RemoveSection(string _sectionName)
        {
            return data.Remove(_sectionName);
        }

        /// <summary>
        /// Compare two iniDatas has same section and key(not contains value).
        /// </summary>
        public bool Compare(IniDatas _iniDatas)
        {
            var compareData = _iniDatas.data;
            if (data.Count != compareData.Count)
            {
                return false;
            }
            foreach (var section in data.Keys.ToList())
            {
                if (compareData.ContainsKey(section))
                {
                    foreach (var key in data[section].data.Keys.ToList())
                    {
                        if (!compareData[section].data.ContainsKey(key))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public IniDatas()
        {
            data = new Dictionary<string, IniSection>();
        }

        public IniDatas(string _filePath)
        {
            data = new Dictionary<string, IniSection>();
            string[] datas = File.ReadAllLines(_filePath);
            IniSection section = new IniSection();
            List<string> comments = new List<string>();
            for (int i = 0; i < datas.Length; ++i)
            {
                datas[i] = datas[i].Trim();
                if (datas[i] == string.Empty)
                {
                    continue;
                }
                else if (datas[i][0] == '[')
                {
                    if (section.name != string.Empty)
                    {
                        Add(section);
                    }
                    section = new IniSection(datas[i].Replace("[", "").Replace("]", ""), comments);
                    comments = new List<string>();
                }
                else if (datas[i][0] == ';')
                {
                    comments.Add(datas[i].Replace(";", ""));
                }
                else
                {
                    try
                    {
                        section.Add(new IniNode(datas[i].Split('=')[0].Trim(), datas[i].Split('=')[1].Trim(), comments));
                        comments = new List<string>();
                    }
                    catch { }
                }
            }
            Add(section);
        }

        public string Data2String
        {
            get
            {
                string dataString = string.Empty;
                foreach (var key in data.Keys.ToList())
                {
                    foreach (var comment in data[key].comments)
                    {
                        dataString += ';' + comment + '\n';
                    }
                    dataString += '[' + data[key].name + ']' + '\n';
                    foreach (var key_ in data[key].data.Keys.ToList())
                    {
                        foreach (var comment_ in data[key].data[key_].comments)
                        {
                            dataString += ';' + comment_ + '\n';
                        }
                        dataString += data[key].data[key_].key + '=' + data[key].data[key_].value + '\n';
                    }
                    dataString += '\n';
                }
                return dataString;
            }
        }

        public IniDatas Write(string _filePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(_filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
            }
            File.WriteAllText(_filePath, Data2String);
            return this;
        }
    }

    public class IniSection
    {
        public string name = string.Empty;
        public List<string> comments;
        public Dictionary<string, IniNode> data;

        public IniNode this[string _key]
        {
            get { return data[_key]; }
            set { data[_key] = value; }
        }

        public void Add(IniNode _iniData)
        {
            data.Add(_iniData.key, _iniData);
        }

        public bool RemoveNode(string _nodeName)
        {
            return data.Remove(_nodeName);
        }

        public IniSection()
        {
            comments = new List<string>();
            data = new Dictionary<string, IniNode>();
        }

        public IniSection(string _sectionName, string _comment)
        {
            name = _sectionName;
            (comments = new List<string>()).Add(_comment);
            data = new Dictionary<string, IniNode>();
        }

        public IniSection(string _sectionName, List<string> _comments = default)
        {
            name = _sectionName;
            comments = _comments ?? new List<string>();
            data = new Dictionary<string, IniNode>();
        }

        public IniNode Data(string _key)
        {
            return data[_key];
        }
    }

    public class IniNode
    {
        public string key = string.Empty;
        public string value = string.Empty;
        public List<string> comments;

        public int AsInt { get { return int.Parse(value); } }
        public float AsFloat { get { return float.Parse(value); } }
        public bool AsBool { get { return bool.Parse(value); } }

        public IniNode()
        {
            comments = new List<string>();
        }

        public IniNode(string _key, string _value, string _comment)
        {
            key = _key;
            value = _value;
            (comments = new List<string>()).Add(_comment);
        }

        public IniNode(string _key, string _value, List<string> _comments = default)
        {
            key = _key;
            value = _value;
            comments = _comments ?? new List<string>();
        }
    }
    #endregion

    #region LogObject
    public class LogObject
    {
        public string logStr;
        public string logInfo;
        public string time;
        public LogType type;
        public int index;

        public LogObject SetRepeat(int _repeat)
        {
            index = _repeat;
            return this;
        }

        public LogObject() { }

        public LogObject(string _logStr, string _stackTrace)
        {
            logStr = _logStr;
            logInfo = _stackTrace;
        }

        public LogObject(string _logStr, string _stackTrace, LogType _type, float _time)
        {
            logStr = _logStr;
            logInfo = _stackTrace;
            type = _type;
            time = _time.ToString("{0.00}");
        }
    }
    #endregion

    #region Config Object
    public abstract class Config
    {
        protected string label;
        protected string tooltip;
        public string Label { get { return label; } set { label = value; } }
        public string Tooltip { get { return tooltip; } set { tooltip = value; } }

        public abstract IniNode AsIniNode { get; }
        public abstract void LoadValueFromIniNode(IniNode _node);
        public abstract void DrawGUI(Rect _rect, Rect _boxRect);
        public abstract bool IsDatable { get; }
    }

    public class Config<T> : Config
    {
        Action<T> Set;
        Func<T> Get;

        public T Value { get { return Get.Invoke(); } set { Set.Invoke(value); } }

        public override IniNode AsIniNode
        {
            get
            {
                return string.IsNullOrEmpty(Tooltip) ? 
                    new IniNode(label, Get.Invoke().ToString()) :
                    new IniNode(label, Get.Invoke().ToString(), Tooltip);
            }
        }

        public override void LoadValueFromIniNode(IniNode _node)
        {
            Tooltip = _node.comments.Count > 0 ? _node.comments[0] : string.Empty;
            switch (Value)
            {
                case int i:
                    (Set as Action<int>).Invoke(_node.AsInt);
                    break;
                case float f:
                    (Set as Action<float>).Invoke(_node.AsFloat);
                    break;
                case bool b:
                    (Set as Action<bool>).Invoke(_node.AsBool);
                    break;
                case string s:
                    (Set as Action<string>).Invoke(_node.value);
                    break;
                case Vector2 v2:
                    string[] xy = _node.value.Replace("(", string.Empty).Replace(")", string.Empty).Split(',');
                    (Set as Action<Vector2>).Invoke(new Vector2(float.Parse(xy[0]), float.Parse(xy[1])));
                    break;
                case Vector3 v3:
                    string[] xyz = _node.value.Replace("(", string.Empty).Replace(")", string.Empty).Split(',');
                    (Set as Action<Vector3>).Invoke(new Vector3(float.Parse(xyz[0]), float.Parse(xyz[1]), float.Parse(xyz[2])));
                    break;
            }
        }

        public override void DrawGUI(Rect _rect, Rect _boxRect)
        {
            Rect labelRect = new Rect(_rect.x, _rect.y, (_rect.width / 5) * 3, _rect.height);
            GUI.skin.label.fontSize = (int)_rect.height - 10;
            GUI.skin.textField.fontSize = (int)_rect.height - 10;
            GUI.skin.label.fontStyle = FontStyle.Italic;
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.skin.label.normal.textColor = Color.black;
            GUI.Label(labelRect, label);
            GUI.skin.label.fontStyle = FontStyle.Normal;

            Rect configRect = new Rect(_rect.x + (_rect.width / 5) * 3, _rect.y, (_rect.width / 5) * 2, _rect.height);
            switch (Value)
            {
                case int i:
                    string inputField = GUI.TextField(configRect, (Get as Func<int>).Invoke().ToString());
                    try {
                        (Set as Action<int>).Invoke(string.IsNullOrEmpty(inputField) ? 0 : int.Parse(inputField));
                    }
                    catch { }
                    /*guiInput = GUI.TextField(configRect, guiInput);
                    if (string.IsNullOrEmpty(guiInput))
                    {
                        (Set as Action<int>).Invoke(0);
                    }
                    else if (int.TryParse(guiInput, out int value))
                    {
                        (Set as Action<int>).Invoke(value);
                        guiInput = Value.ToString();
                    }
                    else
                    {
                        guiInput = Value.ToString();
                    }*/
                    break;
                case float f:
                    inputField = GUI.TextField(configRect, (Get as Func<float>).Invoke().ToString());
                    try {
                        (Set as Action<float>).Invoke(string.IsNullOrEmpty(inputField) ? 0 : float.Parse(inputField));
                    }
                    catch { }
                    break;
                case bool b:
                    AC.toggleStyle.normal.background = Texture2D.blackTexture;
                    GUI.DrawTexture(new Rect(configRect.x + 1, configRect.y + 1, configRect.height - 2, configRect.height - 2), AC.toggleBGTexture);
                    (Set as Action<bool>).Invoke(GUI.Toggle(new Rect(configRect.x + AC.spacing / 2, configRect.y + AC.spacing / 2, configRect.height - AC.spacing, configRect.height - AC.spacing), (Get as Func<bool>).Invoke(), string.Empty, AC.toggleStyle));
                    break;
                case string s:
                    (Set as Action<string>).Invoke(GUI.TextField(configRect, (Get as Func<string>).Invoke()));
                    break;
                case Vector2 v2:
                    float width = configRect.width / 2;
                    // X
                    Rect currentRect = new Rect(configRect.x, configRect.y, width, configRect.height);
                    inputField = GUI.TextField(currentRect, (Get as Func<Vector2>).Invoke().x.ToString());
                    try {
                        (Set as Action<Vector2>).Invoke(new Vector2(string.IsNullOrEmpty(inputField) ? 0 : float.Parse(inputField), (Get as Func<Vector2>).Invoke().y));
                    }
                    catch { }
                    // Y
                    currentRect = new Rect(configRect.x + width, configRect.y, width, configRect.height);
                    inputField = GUI.TextField(currentRect, (Get as Func<Vector2>).Invoke().y.ToString());
                    try {
                        (Set as Action<Vector2>).Invoke(new Vector2((Get as Func<Vector2>).Invoke().x, string.IsNullOrEmpty(inputField) ? 0 : float.Parse(inputField)));
                    }
                    catch { }
                    break;
                case Vector3 v3:
                    width = configRect.width / 3;
                    //X
                    currentRect = new Rect(configRect.x, configRect.y, width - 2, configRect.height);
                    inputField = GUI.TextField(currentRect, (Get as Func<Vector3>).Invoke().x.ToString());
                    try {
                        (Set as Action<Vector3>).Invoke(new Vector3(string.IsNullOrEmpty(inputField) ? 0 : float.Parse(inputField), (Get as Func<Vector3>).Invoke().y, (Get as Func<Vector3>).Invoke().z));
                    }
                    catch { }
                    // Y
                    currentRect = new Rect(configRect.x + width, configRect.y, width, configRect.height);
                    inputField = GUI.TextField(currentRect, (Get as Func<Vector3>).Invoke().y.ToString());
                    try {
                        (Set as Action<Vector3>).Invoke(new Vector3((Get as Func<Vector3>).Invoke().x, string.IsNullOrEmpty(inputField) ? 0 : float.Parse(inputField), (Get as Func<Vector3>).Invoke().z));
                    }
                    catch { }
                    // Z
                    currentRect = new Rect(configRect.x + width * 2, configRect.y, width, configRect.height);
                    inputField = GUI.TextField(currentRect, (Get as Func<Vector3>).Invoke().z.ToString());
                    try {
                        (Set as Action<Vector3>).Invoke(new Vector3((Get as Func<Vector3>).Invoke().x, (Get as Func<Vector3>).Invoke().y, string.IsNullOrEmpty(inputField) ? 0 : float.Parse(inputField)));
                    }
                    catch { }
                    break;
                case Button btn:
                    AC.toggleStyle.normal.background = AC.toggleBGTexture;
                    if (GUI.Button(configRect, AC.toggleBGTexture, AC.toggleStyle))
                    {
                        (Set as Action<Button>).Invoke(new Button());
                    }
                    break;
            }

            // Draw tooltip
            if (!string.IsNullOrEmpty(Tooltip))
            { 
                if (Utility.IsMouseHoverOnGUI(new Rect(_boxRect.x + _rect.x, _boxRect.y + _rect.y, labelRect.width, labelRect.height)))
                {
                    AC.logStyle.normal.textColor = Color.white;
                    AC.logStyle.alignment = TextAnchor.MiddleLeft;
                    AC.logStyle.normal.background = AC.toggleBGTexture;
                    AC.logStyle.fontSize = GUI.skin.label.fontSize;
                    GUI.Label(_rect, Tooltip, AC.logStyle);
                }
            }
        }

        public Config(string _label, Func<T> _Get, Action<T> _Set, string _tooltip = default)
        {
            label = _label;
            Get = _Get;
            Set = _Set;
            Tooltip = _tooltip;
        }

        public override bool IsDatable
        {
            get
            { 
                switch (Value)
                {
                    case Button btn:
                        return false;
                }
                return true;
            }
        }
    }

    public struct Button { }
    #endregion

    public static class Utility
    {
        public static bool IsMouseHoverOnGUI(Rect _guiRect)
        {
            return _guiRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y));
        }
    }
}