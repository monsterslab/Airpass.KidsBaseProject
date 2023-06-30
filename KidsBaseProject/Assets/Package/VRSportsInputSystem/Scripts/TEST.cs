using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AirpassUnity.VRSportsInputSystem
{
    public class TEST : MonoBehaviour
    {
        //#if UNITY_EDITOR
        [SerializeField]
        public string ipAddress = "127.0.0.1";
        [SerializeField]
        public int port = 8052;
        public int port_Sub = 8053;
        private UdpClient udpClient;
        private UdpClient udpClient_Sub;

        public List<Text> texts
            = new List<Text>();

        public void ManageSending(int _press, Vector2 pos, int _lidar = 0)
        {
            JObject json = new JObject();
            json.Add("lidar", _lidar);
            json.Add("id", _lidar);
            json.Add("status", _press);
            json.Add("x", pos.x);
            json.Add("y", pos.y);
            JArray jArray = new JArray();
            jArray.Add(json);
            json = new JObject();
            json.Add("ClickData", jArray);
            SendData(json.ToString());
        }

        public void SendData(string _data)
        {
            byte[] sendingData = Encoding.UTF8.GetBytes(_data);
            udpClient.Send(sendingData, sendingData.Length);
            udpClient_Sub.Send(sendingData, sendingData.Length);
        }

        private void OnApplicationQuit()
        {
            udpClient.Close();
            udpClient_Sub.Close();
        }

        public void Initialize()
        {
            udpClient = new UdpClient();
            udpClient_Sub = new UdpClient();
            udpClient.EnableBroadcast = true;
            udpClient_Sub.EnableBroadcast = true;
            udpClient.Connect(IPAddress.Parse(ipAddress), port);
            udpClient_Sub.Connect(IPAddress.Parse(ipAddress), port_Sub);
        }

        public void Restart()
        {
            udpClient.Close();
            Initialize();
        }

        public void Btn_Reload()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MultiDisplayContentSample");
        }

        void Awake()
        {

        }

        // Start is called before the first frame update
        void Start()
        {
            Initialize();
        }

        // Update is called once per frame
        void Update()
        {
            int number = -1;
            if (Input.GetKeyDown(KeyCode.T))
            {
                number = 0;
                ManageSending(number, Input.mousePosition);
            }
            else if (Input.GetKey(KeyCode.T))
            {
                number = 1;
                ManageSending(number, Input.mousePosition);
            }
            else if (Input.GetKeyUp(KeyCode.T))
            {
                number = 2;
                ManageSending(number, Input.mousePosition);
            }
            if (number == -1)
            {
                ManageSending(number, Vector2.zero);
            }
            else
            {
                ManageSending(number, Input.mousePosition);
            }
        }
        //#endif
    }
}