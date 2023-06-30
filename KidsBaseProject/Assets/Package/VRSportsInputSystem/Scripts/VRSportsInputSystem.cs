using Tools.Singletons;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;
using Tools.IniParser;
using UnityEngine.UI;

namespace AirpassUnity.VRSports
{
    /// Main part.
    public partial class VRSportsInputSystem
    {
        [SerializeField] bool closeVRSportsWhileQuit = false;
        public bool debugging = false;
        public int receiveFrameDelay = 10;
        public float holdDelayOfVRSportsButton = 2.0f;
        public bool singleButtonInteract = true;

        [SerializeField] private Camera physicsRaycastCamera;

        [HideInInspector] public bool useGlobalConfig = false;

        private InteractData debugData;
        private bool enable = true;
        private Dictionary<IVRSportsInteractable, InteractableDummy> interactedList = new();
        private Dictionary<int, InteractData> interactDatas = new();
        private List<int> currentReceivedLidar = new List<int>();
        public static event Action<List<int>> StartReceiveDataEvent;
        public static event Action<InteractData> ReceivedDataEvent;

        public bool Enable
        {
            get { return enable; }
            set { enable = value; }
        }
        public bool Debugging
        {
            get { return debugging; }
            set { debugging = value; }
        }

        public Camera DefaultPhysicsRaycastCamera
        {
            get { return physicsRaycastCamera; }
            set { physicsRaycastCamera = value; }
        }

        public void InvokeIVRSportsInteractable(InteractData data, GameObject interacted)
        {
            bool removed = false;
            if (!interacted.TryGetComponent(out IVRSportsInteractable @interface))
            {
                Debug.LogError("VRSports Exception : Not a legal gameObject!");
                return;
            }
            bool hasData = interactedList.TryGetValue(@interface, out InteractableDummy dummy);
            //@interface.InteractingData.Add(data.id, data);
            switch (data.status)
            {
                case "0":
                    data.type = InteractType.down;
                    break;
                case "1":
                    if (hasData)
                        data.type = InteractType.holding;
                    else
                        data.type = InteractType.enter;
                    break;
                case "2":
                    if (hasData)
                    {
                        if (dummy.isBeenDown)
                            data.type = InteractType.click;
                        else
                            data.type = InteractType.up;
                    }
                    else
                    {
                        data.type = InteractType.exit;
                    }
                    interactedList.Remove(@interface);
                    removed = true;
                    break;
            }
            if (interactedList.ContainsKey(@interface))
            {
                interactedList[@interface].lostFrame = 0;
            }
            else if (!removed)
            {
                interactedList.Add(@interface, new InteractableDummy(data.type == InteractType.down, data, @interface));
            }
            switch (data.type)
            {
                case InteractType.enter: @interface.InteractEnter(data); break;
                case InteractType.down: @interface.InteractDown(data); break;
                case InteractType.holding:
                    @interface.InteractHolding(data);
                    break;
                case InteractType.exit: @interface.InteractExit(data); break;
                case InteractType.up: @interface.InteractUp(data); break;
                case InteractType.click: @interface.InteractClick(data); break;
            }
        }

        public bool RaycastPhysicsHit(Vector2 _position, Camera _cam, float _maxDistance, out RaycastHit _hit)
        {
            bool result = Physics.Raycast(_cam.ScreenPointToRay(_position), out RaycastHit rayhit, _maxDistance);
            _hit = rayhit;
            return result;
        }

        private GameObject FindVIGameObjectInParent(Transform @object)
        {
            GameObject temp = null;
            if (@object.TryGetComponent<IVRSportsInteractable>(out _))
            {
                temp = @object.gameObject;
            }
            else
            {
                if (@object.transform.parent != null)
                {
                    if (!@object.transform.parent.TryGetComponent<Canvas>(out _))
                    {
                        temp = FindVIGameObjectInParent(@object.transform.parent);
                    }
                }
            }
            return temp;
        }

        private void InvokeVRSportsEvent(InteractData data)
        {
            // UI interact.
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = data.position;
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);
            for (int i = 0; i < raycastResults.Count; ++i)
            {
                if (raycastResults[i].gameObject.TryGetComponent(out Graphic graphic))
                {
                    if (graphic.raycastTarget)
                    {
                        GameObject go = FindVIGameObjectInParent(raycastResults[i].gameObject.transform);
                        if (go != null)
                        {
                            InvokeIVRSportsInteractable(data, go);
                        }
                        break;
                    }
                }
            }

            // Physics interact.
            Camera cam = physicsRaycastCamera;
            if (cam == null) cam = Camera.main;
            if (RaycastPhysicsHit(data.position, cam, Mathf.Infinity, out RaycastHit hit))
            {
                if (hit.transform.TryGetComponent(out IVRSportsInteractable _))
                {
                    InvokeIVRSportsInteractable(data, hit.transform.gameObject);
                }
            }
        }

        private void FinishVRSportsEvent()
        {
            List<IVRSportsInteractable> removeIDs = new List<IVRSportsInteractable>();
            // Invoke exit interact.
            foreach (KeyValuePair<IVRSportsInteractable, InteractableDummy> t in interactedList)
            {
                InteractableDummy dummy = t.Value;
                if (dummy.lostFrame >= receiveFrameDelay)
                {
                    dummy.interactData.type = InteractType.exit;
                    dummy.interactable.InteractExit(dummy.interactData);
                    removeIDs.Add(t.Key);
                }
                else
                {
                    dummy.lostFrame++;
                }
            }
            for (int i = 0; i < removeIDs.Count; ++i)
            {
                interactedList.Remove(removeIDs[i]);
            }
        }

        void LoadConfig()
        {
            IniDatas iniDatas = new IniDatas();
            IniSection currentSection = new IniSection("USECONFIG", @"Use local config file(Application.streamingAssetsPath) or global config file(AppData\Local\Airpass\Unity_VRSportsInputSystem).");
            currentSection.Add(new IniNode("UseGlobalConfig", useGlobalConfig.ToString()));
            iniDatas.Add(currentSection);

            currentSection = new IniSection("CONFIGURATION_UDPCONNECTION", "Settings for udp socket connection.");
            currentSection.Add(new IniNode("Enable", enable.ToString(), "Udp socket ipAdress for vVRsports connection."));
            currentSection.Add(new IniNode("IPAddress", ipAdress, "Udp socket ipAdress for vVRsports connection."));
            currentSection.Add(new IniNode("Port_Send", port_Send.ToString(), "Udp socket send port for send message to VRsports connection."));
            currentSection.Add(new IniNode("Port_Receive", port_Receive.ToString(), "Udp socket receive port for get ClickData from VRsports connection."));
            currentSection.Add(new IniNode("ShowClickDataDebug", debugging.ToString(), "Show current receive udp ClickData debug text."));
            currentSection.Add(new IniNode("ReceiveFrameDelay", receiveFrameDelay.ToString(), "Show current receive udp ClickData debug text."));
            iniDatas.Add(currentSection);

            currentSection = new IniSection("CONFIGURATION_VRSPORTSINPUTSYSTEM", "Settings for VRSportsInputSystem.");
            currentSection.Add(new IniNode("DelayOfHoldButton", holdDelayOfVRSportsButton.ToString(), "The holding delay of hold button use with VRSportsInputSystem."));
            iniDatas.Add(currentSection);

            string filePath = Application.streamingAssetsPath;
            if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
            filePath = Path.Combine(filePath, @"Config_VRSportsInputSystem.ini");
            if (File.Exists(filePath))
            {
                iniDatas = new IniDatas(filePath);
            }
            else
            {
                iniDatas.Write(filePath);
            }
            if (useGlobalConfig = iniDatas.data["USECONFIG"].Data("UseGlobalConfig").AsBool)
            {
                filePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                filePath = Path.Combine(filePath, @"Airpass", @"VRSportsInputSystem");
                if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
                filePath = Path.Combine(filePath, @"Config.ini");
                if (File.Exists(filePath))
                {
                    iniDatas = new IniDatas(filePath);
                }
                else
                {
                    iniDatas.RemoveSection("USECONFIG");
                    iniDatas.Write(filePath);
                }
            }

            currentSection = iniDatas.data["CONFIGURATION_UDPCONNECTION"];
            enable = currentSection.Data("Enable").AsBool;
            ipAdress = currentSection.Data("IPAddress").value;
            port_Send = currentSection.Data("Port_Send").AsInt;
            port_Receive = currentSection.Data("Port_Receive").AsInt;
            debugging = currentSection.Data("ShowClickDataDebug").AsBool;
            receiveFrameDelay = currentSection.Data("ReceiveFrameDelay").AsInt;

            currentSection = iniDatas.data["CONFIGURATION_VRSPORTSINPUTSYSTEM"];
            holdDelayOfVRSportsButton = currentSection.Data("DelayOfHoldButton").AsFloat;
        }

        protected override void Awake()
        {
            base.Awake();
            LoadConfig();
            Initialization_UDP();
        }

        void OnApplicationQuit()
        {
            ShutDown_Udp();
        }
    }

    /// Protocol part
    public partial class VRSportsInputSystem : SingletonUnityEternal<VRSportsInputSystem>
    {
        public int port_Receive = 8052;
        public int port_Send = 9099;

        private UdpClient udpClientReceive;
        private UdpClient udpClientSend;
        private IEnumerator udpReceiver;
        private IPEndPoint ipEndPoint;
        private string ipAdress = "127.0.0.1";
        //private readonly object receiveLock = new object();

        private IEnumerator UdpReceiver()
        {
            while (true)
            {
                if (udpClientReceive == null) break;
                currentReceivedLidar.Clear();
                interactDatas.Clear();
                while (udpClientReceive.Available > 0)
                {
                    if (enable)
                    {
                        byte[] byteData = udpClientReceive.Receive(ref ipEndPoint);
                        if (byteData != null)
                        {
                            JObject jsonObj = JObject.Parse(Encoding.UTF8.GetString(byteData).Trim());
                            JArray jArray = JArray.Parse(jsonObj["ClickData"].ToString());
                            for (int i = 0; i < jArray.Count; ++i)
                            {
                                JObject tmp = jArray[i] as JObject;
                                int lidar = (int)tmp["lidar"];
                                currentReceivedLidar.Add(lidar);
                                InteractData temp = debugData = new InteractData(lidar, (int)tmp["id"], tmp["status"].ToString(), (float)tmp["x"], (float)tmp["y"]);
                                if (interactDatas.TryGetValue(temp.id, out InteractData t))
                                {
                                    if (t.status == "1" ||
                                        (t.status == "0" && temp.status == "2"))
                                    {
                                        interactDatas[temp.id] = temp;
                                    }
                                }
                                else
                                {
                                    interactDatas.Add(temp.id, temp);
                                }
                            }
                        }
                    }
                }
                if (interactDatas.Count > 0)
                {
                    StartReceiveDataEvent?.Invoke(currentReceivedLidar);
                    foreach (KeyValuePair<int, InteractData> t in interactDatas)
                    {
                        ReceivedDataEvent?.Invoke(t.Value);
                        InteractData data = t.Value;
                        InvokeVRSportsEvent(data);
                    }
                    FinishVRSportsEvent();
                }

                yield return null;
            }
        }

        /// <summary>
        /// Change VRSports event type.
        /// </summary>
        /// <param name="_type"></param>
        /// <param name="_active"></param>
        public void ChangeEvent(EventType _type)
        {
            SendData(_type.ToString());
        }

        private void SendData(string _data)
        {
            byte[] sendingData = Encoding.UTF8.GetBytes(_data);
            udpClientSend.Send(sendingData, sendingData.Length);
        }

        void Initialization_UDP()
        {
            udpClientSend = new UdpClient();
            udpClientSend.Connect(IPAddress.Parse(ipAdress), port_Send);

            if (enable)
            {
                udpClientReceive = new UdpClient();
                udpClientReceive.EnableBroadcast = true;
                udpClientReceive.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpClientReceive.Client.Bind(ipEndPoint = new IPEndPoint(IPAddress.Any, port_Receive));
                StartCoroutine(udpReceiver = UdpReceiver());
            }
        }

        public void ShutDown_Udp()
        {
            if(closeVRSportsWhileQuit)
                SendData("Exit");

            if (udpReceiver != null)
            {
                StopCoroutine(udpReceiver);
            }
            udpClientReceive?.Close();
            udpClientSend?.Close();
        }

        void OnGUI()
        {
            if (enable && debugging)
            {
                GUI.color = Color.black;
                GUI.enabled = false;
                GUI.skin.label.fontSize = 30;
                string text;
                if (udpClientReceive == null)
                    text = "UdpClient Inactive";
                else if (debugData == null)
                    text = "No Data";
                else
                {
                    text = "";
                    foreach (var data in interactDatas)
                    {
                        text += ($"{data.Value.id} : {data.Value.position}{Environment.NewLine}");
                    }
                }
                GUI.Label(new Rect(0, 0, 1000, 500), text);
            }
        }
    }
}