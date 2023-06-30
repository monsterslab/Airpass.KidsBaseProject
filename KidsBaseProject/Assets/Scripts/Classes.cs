using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Tools.Utility;
using Tools.IniParser;

#region ProjectTemplate basic utility classes.

#region Tools.Utility (Useful Classes or Functions)
namespace Tools.Utility
{
    #region Common Utilities
    public static class Utility
    {
        private static Dictionary<float, WaitForSeconds> waitForSeconds= new Dictionary<float, WaitForSeconds>();
        /// <summary>
        /// Start a Coroutine for Delay action.
        /// </summary>
        /// <param name="delaySecond">How much delay will be to Invoke the action.</param>
        /// <param name="toDo">Action that will Invoke after delay.</param>
        public static void DelayToDo(this MonoBehaviour self, float delaySecond, Action toDo)
        {
            self.StartCoroutine(DelayToDo(delaySecond, toDo));
        }

        /// <summary>
        /// Loop a action while a condition is true until condition is false.
        /// </summary>
        /// <param name="condition">The condition of keep looping.</param>
        /// <param name="toDo">Action that will loop per 'loopInterval' while condition is true.</param>
        /// <param name="loopInterval">Interval second of each loop.</param>
        /// <param name="endAction">Action that will be invoke untill condition false.</param>
        public static void LoopWhile(this MonoBehaviour self, Func<bool> condition, Action toDo, float loopInterval = 0.0f, Action endAction = null)
        {
            self.StartCoroutine(LoopWhile(condition, toDo, loopInterval, endAction));
        }

        /// <summary>
        /// Loop a action until a condition is true while condition is false.
        /// </summary>
        /// <param name="condition">The loop end.</param>
        /// <param name="toDo">Action that will loop per 'loopInterval' while condition is false.</param>
        /// <param name="loopInterval">Interval second of each loop.</param>
        /// <param name="endAction">Action that will be invoke untill condition true.</param>
        public static void LoopUntil(this MonoBehaviour self, Func<bool> condition, Action toDo, float loopInterval = 0.0f, Action endAction = null)
        {
            self.StartCoroutine(LoopWhile(() => !condition.Invoke(), toDo, loopInterval, endAction));
        }

        /// <summary>
        /// Check stirng could be used as script define naming. Judge with is start with alphabet and without special character except'_'.
        /// </summary>
        public static bool IsScripatableNaming(this string self)
        {
            if (!self[0].IsAlphabet())
            {
                return false;
            }
            for (int i = 0; i < self.Length; ++i)
            {
                if (!(self[i].IsNumber() || self[i].IsAlphabet() || self[i] == '_'))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsAlphabet(this char self)
        {
            return (self >= 'A' && self <= 'z');
        }

        public static bool IsNumber(this char self)
        {
            return (self >= '0' && self <= '9');
        }

        public static T TryAddComponent<T>(this GameObject self) where T : MonoBehaviour
        {
            if (self.GetComponent<T>() == null)
            {
                self.AddComponent<T>();
            }
            return self.GetComponent<T>();
        }

        /// <summary>
        /// Convert Texture2D to Sprite.
        /// </summary>
        public static Sprite ToSprite(this Texture2D self)
        {
            Sprite spriteTemp = Sprite.Create(self, new Rect(0, 0, self.width, self.height), new Vector2(0.5f, 0.5f));
            spriteTemp.name = self.name;
            return spriteTemp;
        }

        /// <summary>
        /// Get a WaitForSecond instance. To avoid GC memory uses.
        /// </summary>
        public static WaitForSeconds GetWaitForSecond(float time)
        {
            if (!waitForSeconds.ContainsKey(time))
            {
                waitForSeconds.Add(time, new WaitForSeconds(time));
            }
            return waitForSeconds[time];
        }

        private static IEnumerator DelayToDo(float delaySecond, Action toDo)
        {
            yield return GetWaitForSecond(delaySecond);
            toDo();
        }

        private static IEnumerator LoopWhile(Func<bool> condition, Action toDo, float loopInterval = 0.0f, Action endAction = null)
        {
            while (condition.Invoke())
            {
                toDo?.Invoke();
                yield return (loopInterval == 0.0f ? null : GetWaitForSecond(loopInterval));
            }
            endAction?.Invoke();
        }
    }
    #endregion

    #region KeyOperator
    public class KeyOperator<T>
    {
        private Dictionary<T, Action> actions = new Dictionary<T, Action>();

        public void Register(T _enum, Action _action)
        {
            if (actions.ContainsKey(_enum))
            {
                actions[_enum] = _action;
            }
            else
            {
                actions.Add(_enum, _action);
            }
        }

        public void Operate(T _enum)
        {
            if (actions.ContainsKey(_enum))
            {
                actions[_enum]?.Invoke();
            }
        }
    }
    #endregion
}
#endregion

#region Tools.Singletons
namespace Tools.Singletons
{
    // The Singleton with Monobehavior & has the DontDestroyOnload().
    #region SingletonUnityEternal
    /// <summary>
    /// Eternal Unity Singleton is the singleton with Monobehavior & DontDestroyOnLoad() Function.
    /// </summary>
    /// <typeparam name="SingletonUnityEternal"></typeparam>
    public abstract class SingletonUnityEternal<T> : SingletonUnity<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            base.Awake();
            if (instance == this)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }
    #endregion

    // The Singleton with Monobehavior.
    #region SingletonUnity
    /// <summary>
    /// Unity Singleton is the singleton with Monobehavior.
    /// </summary>
    /// <typeparam name="SingletonUnity"></typeparam>
    public abstract class SingletonUnity<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T instance;

        /// <summary>
        /// Get instance while there is any GameObject.
        /// </summary>
        public static T Instance => instance = instance ?? FindObjectOfType<T>();

        /// <summary>
        /// Get instance. Additionally instantiate new gameObject while Instance is null.
        /// </summary>
        public static T INWN => instance = Instance ?? new GameObject().AddComponent<T>();

        protected virtual void Awake()
        {
            // Initialization for Singleton.
            gameObject.SetActive((instance ?? (instance = this as T)) == this);
        }
    }
    #endregion

    // Just simple SIngleton.
    #region Singleton
    /// <summary>
    /// Normal singleton.
    /// </summary>
    /// <typeparam name="T">Component name.</typeparam>
    public abstract class Singleton<T> where T : class, new()
    {
        protected volatile static T instance;
        private static readonly object lockObj = new object();

        public static T Instance
        {
            get
            {
                lock (lockObj)
                {
                    return instance ?? (instance = new T());
                }
            }
        }
    }
    #endregion
}
#endregion

#region Tools.Processor
namespace Tools.Processor
{
    using Tools.Utility;
    using Tools.Singletons;
    using Tools.Attributes;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public abstract class Processor<T, E> : SingletonUnity<T>
        where T : Processor<T, E>
        where E : Enum
    {
        private const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        [UneditableField, SerializeField] private E state;
        [UneditableField, SerializeField] private E preState;

        public event Action<E> StateEnableEvent;
        public event Action<E> StateUpdateEvent;

        private KeyOperator<E> stateEnableKO = new KeyOperator<E>();
        private KeyOperator<E> stateUpdateKO = new KeyOperator<E>();

        public E PreState => preState;
        public E State
        {
            get => state;
            set
            {
                preState = state;
                state = value;
                StateEnableEvent?.Invoke(state);
            }
        }

        /// <summary>
        /// Initialization that could be defined to called in any where not specific.
        /// </summary>
        public virtual void Initialization()
        {
            // Register operations.
            MethodInfo[] methodInfos = GetType().GetMethods(bindingFlags);
            for (int i = 0; i < methodInfos.Length; ++i)
            {
                if (methodInfos[i].GetCustomAttributes(typeof(CompilerGeneratedAttribute), true).Length == 0)
                {
                    string methodName = methodInfos[i].Name;
                    if (methodName.Count(t => t == '_') == 1)
                    {
                        string[] nameSplitted = methodName.Split('_');
                        for (int j = 0; j < nameSplitted.Length; ++j)
                        {
                            if (Enum.TryParse(typeof(E), nameSplitted[j].ToLower(), out object @enum))
                            {
                                string other = nameSplitted[j == 0 ? 1 : 0].ToLower();
                                if (other == "enable" || other == "update")
                                {
                                    (other == "enable" ? stateEnableKO : stateUpdateKO).Register((E)@enum, Delegate.CreateDelegate(typeof(Action), this, methodInfos[i]) as Action);
                                }
                            }
                        }
                    }
                }
            }
            StateEnableEvent += stateEnableKO.Operate;
            StateUpdateEvent += stateUpdateKO.Operate;
        }

        protected override void Awake()
        {
            base.Awake();
            Initialization();
        }

        protected virtual void Update()
        {
            StateUpdateEvent?.Invoke(state);
        }
    }

    public abstract class ProcessorEternal<T, E> : Processor<T, E>
        where T : ProcessorEternal<T, E>
        where E : Enum
    {
        protected override void Awake()
        {
            base.Awake();
            try
            {
                DontDestroyOnLoad(gameObject);
            }
            catch { }
        }
    }
}
#endregion

#region Tools.IniParser
namespace Tools.IniParser
{
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
}
#endregion

#region Tools.Attributes
namespace Tools.Attributes
{
    #region UneditableField
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class UneditableFieldAttribute : PropertyAttribute
    {
        public ShowOnlyOption Option { get; set; } = ShowOnlyOption.always;
        public UneditableFieldAttribute() { }
        public UneditableFieldAttribute(ShowOnlyOption option) => Option = option;
    }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(UneditableFieldAttribute), true)]
    public class UneditableFieldAttributeDrawer : PropertyDrawer
    {
        UneditableFieldAttribute Attribute => attribute as UneditableFieldAttribute;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool uneditable = true;
            switch (Attribute.Option)
            {
                case ShowOnlyOption.editMode:
                    uneditable = !Application.isPlaying;
                    break;
                case ShowOnlyOption.playMode:
                    uneditable = Application.isPlaying;
                    break;
            }
            using (var scope = new EditorGUI.DisabledGroupScope(uneditable))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }
    }
    #endif
    #endregion

    //#region SerializableDictioanary
    //[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    //public class VisibleDictioanaryAttribute : PropertyAttribute
    //{
    //    public VisibleDictioanaryAttribute() { }
    //}

    //[CustomPropertyDrawer(typeof(VisibleDictioanaryAttribute), false)]
    //public class VisibleDictioanaryAttributeDrawer : PropertyDrawer
    //{
    //    VisibleDictioanaryAttribute Attribute => attribute as VisibleDictioanaryAttribute;


    //    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //    {
    //        var keyArrayProperty = property.FindPropertyRelative("m_keys");

    //        EditorGUI.PropertyField(position, property, label, true);
    //    }
    //}
    //#endregion
}
#endregion

#endregion