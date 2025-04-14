using System.Net;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkSetupScreen : MonoBehaviour
{
    [SerializeField] private TMP_InputField _name;
    [SerializeField] private TMP_InputField _serverIpField;
    [SerializeField] private TMP_InputField _serverPortField;
    [SerializeField] private Button _startSeverButton;
    [SerializeField] private Button _connectToServerButton;


    void Awake()
    {
        _startSeverButton.onClick.AddListener(OnStartServer);
        _connectToServerButton.onClick.AddListener(OnConnectToServer);
    }

    void OnDestroy()
    {
        _startSeverButton.onClick.RemoveListener(OnStartServer);
        _connectToServerButton.onClick.RemoveListener(OnConnectToServer);
    }


    private void OnStartServer()
    {
        int port = Convert.ToInt32(_serverPortField.text);
        TcpManager.Instance.StartServer(port);

        //MoveToChatScreen();
    }

    private void OnConnectToServer()
    {
        IPAddress ipAddress = IPAddress.Parse(_serverIpField.text);
        int port = Convert.ToInt32(_serverPortField.text);

        TcpManager.Instance.StartClient(ipAddress, port);
        //TcpManager.Instance.OnClientConnected += MoveToChatScreen;
    }
}