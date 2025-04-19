using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public enum Role
{
    None,
    Client,
    Server,
    ServerClient
}

public class TcpManager : MonoBehaviourSingleton<TcpManager>
{
    private string _name = "Anonym";
    private List<TcpConnectionTypes> _tcpManagers = new();
    private bool _wasSetted = false;

    public string NameID { get { return _name; } }

    public List<TcpConnectionTypes> TcpManagers => _tcpManagers;

    public Action<byte[]> OnDataReceived;
    public Action GoToChatScreen;
    public Action GoToServerScren;

    private void Update()
    {
        if (!_wasSetted) return;

        foreach (var tcp in _tcpManagers)
            tcp.Update();
    }

    public bool TcpSetup(Role role, IPAddress serverIp, int port, string name)
    {
        if (!string.IsNullOrEmpty(name)) _name = name;

        switch (role)
        {
            case Role.None:
                return false;

            case Role.Client:
                _wasSetted = true;
                _tcpManagers.Add(new TcpClientManager(serverIp, port));
                _tcpManagers[0].OnDataReceived += OnDataReceived;
                GoToChatScreen?.Invoke();
                return true;

            case Role.Server:
                _wasSetted = true;
                _tcpManagers.Add(new TcpServerManager(serverIp, port));
                GoToServerScren?.Invoke();
                return true;

            case Role.ServerClient:
                _wasSetted = true;
                _tcpManagers.Add(new TcpServerManager(serverIp, port));
                _tcpManagers.Add(new TcpClientManager(serverIp, port));
                _tcpManagers[0].OnDataReceived += OnDataReceived;
                GoToChatScreen?.Invoke();
                return true;
        }

        return false;
    }

    public void ManageData(byte[] data)
    {
        foreach (var tcp in _tcpManagers)
        {
            switch (tcp)
            {
                case TcpServerManager server:
                    server.BroadcastData(data);
                    break;
                case TcpClientManager client:
                    client.SendDataToServer(data);
                    break;
            }
        }
    }
}