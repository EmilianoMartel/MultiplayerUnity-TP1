using System.Net;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkSetupScreen : MonoBehaviour
{
    [SerializeField] private TMP_InputField serverIpField;
    [SerializeField] private TMP_InputField serverPortField;
    [SerializeField] private Button startSeverButton;
    [SerializeField] private Button connectToServerButton;


    void Awake()
    {
        startSeverButton.onClick.AddListener(OnStartServer);
        connectToServerButton.onClick.AddListener(OnConnectToServer);
    }

    void OnDestroy()
    {
        startSeverButton.onClick.RemoveListener(OnStartServer);
        connectToServerButton.onClick.RemoveListener(OnConnectToServer);
    }


    private void OnStartServer()
    {
        int port = Convert.ToInt32(serverPortField.text);
        TcpManager.Instance.StartServer(port);

        //MoveToChatScreen();
    }

    private void OnConnectToServer()
    {
        IPAddress ipAddress = IPAddress.Parse(serverIpField.text);
        int port = Convert.ToInt32(serverPortField.text);

        TcpManager.Instance.StartClient(ipAddress, port);
        //TcpManager.Instance.OnClientConnected += MoveToChatScreen;
    }
}