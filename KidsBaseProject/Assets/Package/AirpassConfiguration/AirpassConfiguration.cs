using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using EventType = AirpassUnity.VRSports.EventType;

namespace AirpassUnity.Configuration
{
    [DefaultExecutionOrder(-9999)]
    public class AirpassConfiguration : MonoBehaviour
    {
        void Register()
        {
        }

        #region sourceCode
        float intervalWidth;
        public static float spacing = 10.0f;
        public static Texture2D backgroundTexture;
        public static Texture2D groupLabelBGTexture;
        public static Texture2D toggleBGTexture;
        public static Texture2D toggleCheckTexture;
        public static GUIStyle titleStyle;
        public static GUIStyle toggleStyle;
        public static GUIStyle logStyle;

        Dictionary<string, List<Config>> configurations = new Dictionary<string, List<Config>>();
        List<LogObject> logObjects = new List<LogObject>();
        List<int> collapseLogIndexes = new List<int>();

        [SerializeField] private bool log = true;
        [SerializeField] private bool logCollapse = true;

        private bool visible = false;
        private int visibleCounting = 0;
        private float visibleDetermineTimer = 0.0f;
        private float scrollValue_config = 0;
        private float scrollValue_log = 0;
        private string searchField = string.Empty;
        private const string dataPath = "PlatformConfig.ini";
        private const float visibleDetermineTime = 1.0f;
        private const int visibleDeterminer = 3;

        private static AirpassConfiguration instance;

        public static AirpassConfiguration Instance
        {
            get
            {
                return instance ?? FindObjectOfType<AirpassConfiguration>();
            }
        }

        #region Draw debug logs
        void ReceivedLog(string _text, string _stackTrace, LogType _type)
        {
            if (log)
            {
                string[] temp = _stackTrace.Trim().Split('\n');
                string stack = temp[Mathf.Clamp((temp.Length - 1), 0, temp.Length)];
                LogObject currentLog = new LogObject(_text, stack, _type, Time.time);
                LogObject logTemp = logObjects.LastOrDefault(t => (t.logStr == currentLog.logStr) && (t.logInfo == currentLog.logInfo));
                logObjects.Add(currentLog);
                
                // Collapse
                if (logTemp != null)
                {
                    int index = logObjects.IndexOf(logTemp);
                    currentLog.SetRepeat(logTemp.index + 1);
                    if (collapseLogIndexes.Contains(index))
                    {
                        collapseLogIndexes.Remove(index);
                    }
                }
                collapseLogIndexes.Add(logObjects.Count - 1);
            }
        }

        /// <returns>Return need skip this log or not.</returns>
        float DrawDebugLog(LogObject _log, Rect _rect, Rect _boxRect)
        {
            string logString = _log.logStr;
            // Search
            if (!string.IsNullOrEmpty(searchField))
            {
                if (logString.Contains(searchField))
                {
                    int index = logString.IndexOf(searchField);
                    logString = logString.Insert(index + searchField.Length, " :::      ");
                    logString = logString.Insert(index, "      ::: ");
                }
                else return 0;
            }
            if (Utility.IsMouseHoverOnGUI(new Rect(_boxRect.x + _rect.x, _boxRect.y + _rect.y, _rect.width, _rect.height)))
            {
                GUI.skin.textArea.normal.background = toggleBGTexture;
                GUI.skin.textArea.normal.textColor = Color.white;
                logString = $"[{_log.index}] {_log.time} : {_log.logInfo}";
            }
            else
            {
                GUI.skin.textArea.normal.background = Texture2D.blackTexture;
                GUI.skin.textArea.normal.textColor = Color.black;
                logString = $"[{_log.type}] : {logString}";
            }

            GUI.skin.textArea.fontSize = (int)spacing * 2;
            GUI.skin.textArea.alignment = TextAnchor.MiddleLeft;
            _rect.height = GUI.skin.textArea.CalcHeight(new GUIContent(logString), _rect.width);
            GUI.TextArea(_rect, logString);
            return _rect.height;
        }

        void DrawDebugLogs()
        {
            float positionX = intervalWidth + spacing * 2;
            float width = Screen.width - intervalWidth - spacing * 3;

            // Title
            GUI.Label(new Rect(positionX, Screen.height / 2, width, spacing * 4), "Log", titleStyle);

            // Serach TextField
            GUI.skin.textField.fontSize = 30;
            searchField = GUI.TextField(new Rect(positionX, Screen.height / 2 + spacing * 4, (width / 5) * 2, spacing * 4), searchField);

            // Button (Collapse | UnCollapse) & (Start | Stop) & Clear
            positionX = positionX + (width / 5) * 2;
            GUI.skin.button.fontSize = 32;
            float buttonSize = width / 5;
            if (GUI.Button(new Rect(positionX, Screen.height / 2 + spacing * 4, buttonSize, spacing * 4), logCollapse ? "UnCollapse" : "Collapse"))
            {
                logCollapse = !logCollapse;
            }
            if (GUI.Button(new Rect(positionX + buttonSize, Screen.height / 2 + spacing * 4, buttonSize, spacing * 4), log ? "Stop" : "Start"))
            {
                log = !log;
            }
            if (GUI.Button(new Rect(positionX + buttonSize * 2, Screen.height / 2 + spacing * 4, buttonSize, spacing * 4), "Clear"))
            {
                logObjects.Clear();
                collapseLogIndexes.Clear();
            }
            GUI.skin.button.fontSize = default;

            // Log list
            positionX = intervalWidth + spacing * 2;
            float yPosition = 0.0f;
            float logHeight = 30.0f;
            Rect logRect = new Rect(positionX, Screen.height / 2 + spacing * 7 + spacing * 1.5f, width, Screen.height / 2 - spacing * 10);
            GUI.BeginGroup(logRect);
            if (logCollapse)
            {
                foreach (int index in collapseLogIndexes)
                {
                    yPosition += DrawDebugLog(logObjects[index], new Rect(0, yPosition - scrollValue_log - spacing / 2, logRect.width - spacing, logHeight), logRect);
                }
            }
            else
            {
                foreach (LogObject log in logObjects)
                {
                    yPosition += DrawDebugLog(log, new Rect(0, yPosition - scrollValue_log - spacing / 2, logRect.width - spacing, logHeight), logRect);
                }
            }
            GUI.EndGroup();
            scrollValue_log = GUI.VerticalScrollbar(new Rect(logRect.x + width - spacing, logRect.y, spacing / 2, logRect.height), scrollValue_log, spacing, 0.0f, yPosition - (logRect.height));
        }
        #endregion

        #region Draw informations
        void DrawInformations()
        {
            float positionX = intervalWidth + spacing * 2;
            float width = Screen.width - intervalWidth - spacing * 3;
            float positionY = spacing;
            float labelWidth = width / 3;
            // Title
            GUI.Label(new Rect(positionX, positionY, width, spacing * 4), "Infromations", titleStyle);
            // Content title
            positionY += (spacing * 3.5f);
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.fontSize = (int)(spacing * 2.5f);
            GUI.skin.label.normal.background = groupLabelBGTexture;

            GUI.Label(new Rect(positionX, positionY, width, spacing * 3), "<color=white>Device</color>");
            GUI.skin.label.normal.background = null;
            positionY += (spacing * 2.5f);
            // Content
            GUI.skin.label.alignment = TextAnchor.LowerLeft;
            GUI.skin.label.fontSize = (int)spacing * 2;

            GUI.Label(new Rect(positionX, positionY, labelWidth, spacing * 3), "Model");
            GUI.Label(new Rect(positionX + labelWidth, positionY, labelWidth * 2, spacing * 3), SystemInfo.deviceModel);
            positionY += (spacing * 2.5f);
            GUI.Label(new Rect(positionX, positionY, labelWidth, spacing * 3), "Name");
            GUI.Label(new Rect(positionX + labelWidth, positionY, labelWidth * 2, spacing * 3), SystemInfo.deviceName);
            positionY += (spacing * 2.5f);
            GUI.Label(new Rect(positionX, positionY, labelWidth, spacing * 3), "Memory");
            GUI.Label(new Rect(positionX + labelWidth, positionY, labelWidth * 2, spacing * 3), SystemInfo.systemMemorySize.ToString());
            positionY += (spacing * 2.5f);
            GUI.Label(new Rect(positionX, positionY, labelWidth, spacing * 3), "OS");
            GUI.Label(new Rect(positionX + labelWidth, positionY, labelWidth * 2, spacing * 3), SystemInfo.operatingSystem);
            positionY += (spacing * 2.5f);
            GUI.Label(new Rect(positionX, positionY, labelWidth, spacing * 3), "Battery");
            GUI.Label(new Rect(positionX + labelWidth, positionY, labelWidth * 2, spacing * 3), $"{SystemInfo.batteryStatus}, Level : {SystemInfo.batteryLevel}");
            // Content title
            positionY += (spacing * 3.5f);
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.fontSize = (int)(spacing * 2.5f);
            GUI.skin.label.normal.background = groupLabelBGTexture;

            GUI.Label(new Rect(positionX, positionY, width, spacing * 3), "<color=white>Graphic</color>");
            GUI.skin.label.normal.background = null;
            positionY += (spacing * 2.5f);
            // Content
            GUI.skin.label.alignment = TextAnchor.LowerLeft;
            GUI.skin.label.fontSize = (int)spacing * 2;

            GUI.Label(new Rect(positionX, positionY, labelWidth, spacing * 3), "DeviceName");
            GUI.Label(new Rect(positionX + labelWidth, positionY, labelWidth * 2, spacing * 3), SystemInfo.graphicsDeviceName);
            positionY += (spacing * 2.5f);
            GUI.Label(new Rect(positionX, positionY, labelWidth, spacing * 3), "DeviceType");
            GUI.Label(new Rect(positionX + labelWidth, positionY, labelWidth * 2, spacing * 3), SystemInfo.graphicsDeviceType.ToString());
            positionY += (spacing * 2.5f);
            GUI.Label(new Rect(positionX, positionY, labelWidth, spacing * 3), "Memory");
            GUI.Label(new Rect(positionX + labelWidth, positionY, labelWidth * 2, spacing * 3), $"{SystemInfo.graphicsMemorySize}MB");
            // Content title
            positionY += (spacing * 3.5f);
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.fontSize = (int)(spacing * 2.5f);
            GUI.skin.label.normal.background = groupLabelBGTexture;

            GUI.Label(new Rect(positionX, positionY, width, spacing * 3), "<color=white>ContentInfo</color>");
            GUI.skin.label.normal.background = null;
            positionY += (spacing * 2.5f);
            // Content
            GUI.skin.label.alignment = TextAnchor.LowerLeft;
            GUI.skin.label.fontSize = (int)spacing * 2;

            GUI.Label(new Rect(positionX, positionY, labelWidth, spacing * 3), "Unity Version");
            GUI.Label(new Rect(positionX + labelWidth, positionY, labelWidth * 2, spacing * 3), Application.unityVersion);
            positionY += (spacing * 2.5f);
            GUI.Label(new Rect(positionX, positionY, labelWidth, spacing * 3), "Application Version");
            GUI.Label(new Rect(positionX + labelWidth, positionY, labelWidth * 2, spacing * 3), Application.version);
            positionY += (spacing * 2.5f);
            GUI.Label(new Rect(positionX, positionY, labelWidth, spacing * 3), "Scene Name");
            GUI.Label(new Rect(positionX + labelWidth, positionY, labelWidth * 2, spacing * 3), UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            positionY += (spacing * 2.5f);
            GUI.Label(new Rect(positionX, positionY, labelWidth, spacing * 3), "Time.time");
            GUI.Label(new Rect(positionX + labelWidth, positionY, labelWidth * 2, spacing * 3), Time.time.ToString());
            positionY += (spacing * 2.5f);
            GUI.Label(new Rect(positionX, positionY, labelWidth, spacing * 3), "Fps");
            GUI.Label(new Rect(positionX + labelWidth, positionY, labelWidth * 2, spacing * 3), ((int)(1.0f / Time.deltaTime)).ToString());
            positionY += (spacing * 2.5f);
            GUI.Label(new Rect(positionX, positionY, labelWidth, spacing * 3), "Memory Usage");
            GUI.Label(new Rect(positionX + labelWidth, positionY, labelWidth * 2, spacing * 3), $"{((int)(Profiler.GetTotalAllocatedMemoryLong() / 1000000))}MB");
        }
        #endregion

        #region Draw configurations
        void DrawConfigurations()
        {
            intervalWidth = Screen.width / 5 * 3 - spacing * 2;

            // Title
            GUI.Label(new Rect(spacing, spacing, intervalWidth, spacing * 4.5f), "Airpass Config", titleStyle);

            // Button Save & Load.
            GUI.skin.button.fontSize = 32;
            if (GUI.Button(new Rect(spacing, spacing * 6, intervalWidth / 2 - spacing, spacing * 4.5f), "Save"))
            {
                SaveConfig();
            }
            if (GUI.Button(new Rect(intervalWidth / 2 + spacing, spacing * 6, intervalWidth / 2 - spacing, spacing * 4.5f), "Load"))
            {
                LoadConfig();
            }
            GUI.skin.button.fontSize = default;

            // Draw Configurations.
            Rect configRect = new Rect(spacing, spacing * 11, intervalWidth - spacing * 2.5f, Screen.height - (spacing * 13));
            float configHeight = 50.0f;
            List<string> keyList = configurations.Keys.ToList();
            GUI.BeginGroup(configRect);
            float yPosition = 0.0f;
            for (int j = 0; j < keyList.Count; ++j)
            {
                List<Config> configList = configurations[keyList[j]];
                // Draw groupLabel.
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.skin.label.fontStyle = FontStyle.Bold;
                GUI.skin.label.fontSize = (int)configHeight - 10;
                GUI.skin.label.normal.background = groupLabelBGTexture;
                GUI.skin.label.normal.textColor = Color.white;
                GUI.Label(new Rect(0, yPosition - scrollValue_config, configRect.width, configHeight), keyList[j]);
                GUI.skin.label.normal.background = null;
                yPosition += configHeight;
                for (int i = 0; i < configList.Count; ++i)
                {
                    // Draw config.
                    configList[i].DrawGUI(new Rect(0, yPosition - scrollValue_config, configRect.width, configHeight), configRect);
                    yPosition += configHeight;
                }
            }
            GUI.EndGroup();
            scrollValue_config = GUI.VerticalScrollbar(new Rect(intervalWidth - spacing, configRect.y, spacing, configRect.height), scrollValue_config, spacing, 0.0f, yPosition - (configRect.height));
        }
        #endregion

        void DrawGUI()
        {
            // Draw background.
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), backgroundTexture);
            //GUI.color = Color.white;

            // Draw configurations.
            DrawConfigurations();
            // Draw informations.
            DrawInformations();
            // Draw debug logs.
            DrawDebugLogs();
        }

        void SaveConfig()
        {
            IniDatas ini = new IniDatas();
            foreach (var key in configurations.Keys.ToList())
            {
                ini.Add(new IniSection(key));
                foreach (var config in configurations[key])
                {
                    if (!config.IsDatable) continue;
                    try
                    {
                        ini[key].Add(config.AsIniNode);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("acc.ini Data Save Exception : " + e);
                    }
                }
            }
            ini.Write(Path.Combine(Application.streamingAssetsPath, dataPath));
        }

        void LoadConfig()
        {
            string path = Path.Combine(Application.streamingAssetsPath, dataPath);
            if (File.Exists(path))
            {
                IniDatas ini = new IniDatas(path);
                foreach (var key in configurations.Keys.ToList())
                {
                    foreach (var config in configurations[key])
                    {
                        if (!config.IsDatable) continue;
                        try
                        {
                            config.LoadValueFromIniNode(ini[key][config.Label]);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("acc.ini Data Load Exception[" + key + ", " + config.Label + "] : \n" +
                                "Proberly loading data's key from file is newly added?\n" + e);
                        }
                    }
                }
            }
            SaveConfig();
        }

        private void Initialization()
        {
            // Init Singleton
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                if (instance != this)
                {
                    DestroyImmediate(gameObject);
                }
            }
            try
            {
                DontDestroyOnLoad(gameObject);
            }
            catch { }

            // Init gui texture & style.
            (backgroundTexture = new Texture2D(1, 1)).SetPixel(0, 0, new Color(1, 1, 1, 0.7f));
            backgroundTexture.Apply(false);
            
            (groupLabelBGTexture = new Texture2D(1, 1)).SetPixel(0, 0, new Color(0, 0, 0, 0.5f));
            groupLabelBGTexture.Apply(false);

            (toggleBGTexture = new Texture2D(1, 1)).SetPixel(0, 0, new Color(0.15f, 0.15f, 0.15f, 1));
            toggleBGTexture.Apply();

            (toggleCheckTexture = new Texture2D(1, 1)).SetPixel(0, 0, Color.white);
            toggleCheckTexture.Apply();

            toggleStyle = new GUIStyle();
            toggleStyle.padding = new RectOffset(0, 0, 0, 0);
            toggleStyle.alignment = TextAnchor.MiddleRight;
            toggleStyle.imagePosition = ImagePosition.ImageOnly;
            toggleStyle.normal.background = toggleStyle.onActive.background = Texture2D.blackTexture;
            toggleStyle.hover.background = toggleStyle.onHover.background = Texture2D.grayTexture;
            toggleStyle.active.background = toggleStyle.onNormal.background = toggleCheckTexture;

            titleStyle = new GUIStyle();
            titleStyle.alignment = TextAnchor.MiddleLeft;
            titleStyle.normal.textColor = Color.white;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = (int)spacing * 4 - 10;

            logStyle = new GUIStyle();
            logStyle.normal.background = Texture2D.blackTexture;
            logStyle.normal.textColor = Color.black;
            logStyle.fontSize = 20;

            // Debug log
            Application.logMessageReceived += ReceivedLog;

            intervalWidth = Screen.width / 5 * 3 - spacing * 2;
        }

        void Awake()
        {
            Initialization();
        }

        private void Start()
        {
            // Register value.
            Register();
            LoadConfig();

            // ?
        }

        void Register<T>(string _groupLabel, string _label, Func<T> _Get, Action<T> _Set, string _tooltip = default)
        {
            if (!configurations.ContainsKey(_groupLabel))
            {
                configurations.Add(_groupLabel, new List<Config>());
            }
            configurations[_groupLabel].Add(new Config<T>(_label, _Get, _Set, _tooltip));
        }

        void RegisterButton(string _groupLabel, string _label, Action _onClick, string _tooltip = default)
        {
            Register(_groupLabel, _label, () => new Button(), (t) => _onClick.Invoke(), _tooltip);
        }

        void Update()
        {
            if (visible)
            {
                if (Input.GetKeyDown(KeyCode.F12) || Input.GetKeyDown(KeyCode.Escape))
                {
                    visible = false;
                }
            }
            else 
            {
                if (Input.GetKeyDown(KeyCode.F12))
                {
                    visibleCounting += 1;
                    visibleDetermineTimer = Time.time;
                }
                if (visibleCounting > 0)
                {
                    if (Time.time - visibleDetermineTimer > visibleDetermineTime)
                    {
                        visibleCounting = 0;
                        return;
                    }
                    if (visibleCounting >= visibleDeterminer)
                    {
                        visibleCounting = 0;
                        visible = true;
                    }
                }
            }
        }

        void OnGUI()
        {
            if (visible)
            {
                DrawGUI();
            }
        }
        #endregion
    }
}
