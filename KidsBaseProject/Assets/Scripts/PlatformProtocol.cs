using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class PlatformProtocol : MonoBehaviour
{
    #region Singleton.
    private static PlatformProtocol instance;
    public static PlatformProtocol Instance
    {
        get
        {
            return instance ??
                    (instance = FindObjectOfType<PlatformProtocol>()) ??
                    (instance = (new GameObject(typeof(PlatformProtocol).Name)).AddComponent<PlatformProtocol>());
        }
    }
    #endregion

    public static readonly string cmd_Load = "load";
    public static readonly string cmd_End = "end";
    public static readonly string cmd_Home = "home";
    public static readonly string cmd_Restart = "restart";
    public static readonly string cmd_Start = "start";
    public static readonly string cmd_Ready = "ready";
    public static readonly string cmd_Pause = "pause";
    public static readonly string cmd_ConGame = "congame";
    public static readonly string cmd_Close = "close";

    private static UdpClient udpClientSend;
    private string ipAddress = "127.0.0.1";
    private int port_Send = 13809;

    void Awake()
    {
        // Initialization for Singleton.
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            if ((instance != this) && (gameObject != null))
            {
                Destroy(gameObject);
            }
        }
        DontDestroyOnLoad(gameObject);

        udpClientSend = new UdpClient();
        udpClientSend.Connect(IPAddress.Parse(ipAddress), port_Send);
        SendData(cmd_Load);
    }

    public static void SendData(string _data)
    {
        byte[] sendingData = Encoding.UTF8.GetBytes(_data);
        udpClientSend.Send(sendingData, sendingData.Length);
    }

    void ShutDown_Udp()
    {
        udpClientSend?.Close();
    }

    void OnApplicationQuit()
    {
        SendData(cmd_Close);
        ShutDown_Udp();
    }
}
