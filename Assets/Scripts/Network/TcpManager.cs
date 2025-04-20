using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;

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
    private List<ConnectionTypes> _networkManagers = new();
    private bool _wasSetted = false;

    public string NameID { get { return _name; } }

    public Action<byte[]> OnDataReceived;
    public Action GoToChatScreen;
    public Action GoToServerScren;

    private void Update()
    {
        if (!_wasSetted) return;

        foreach (var tcp in _networkManagers)
            tcp.Update();
    }

    public bool TcpSetup(Role role, IPAddress serverIp, int port, string name, string typeConecction)
    {
        if (!string.IsNullOrEmpty(name)) _name = name;

        switch (role)
        {
            case Role.None:
                return false;

            case Role.Client:
                _wasSetted = true;

                GoToChatScreen?.Invoke();
                return true;

            case Role.Server:
                _wasSetted = true;
                _networkManagers.Add(new TcpServerManager(serverIp, port));
                GoToServerScren?.Invoke();
                return true;

            case Role.ServerClient:
                _wasSetted = true;
                _networkManagers.Add(new TcpServerManager(serverIp, port));
                _networkManagers.Add(new TcpClientManager(serverIp, port));
                _networkManagers[0].OnDataReceived += OnDataReceived;
                GoToChatScreen?.Invoke();
                return true;
        }

        return false;
    }

    public void ManageData(byte[] data)
    {
        foreach (var tcp in _networkManagers)
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

    private void ConectionElection()
    {

    }

    private void CreateServer(IPAddress serverIp, int port, string typeConecction)
    {
        if (typeConecction == "TCP")
            CreateTcpServer(serverIp, port);
        else
            CreateUdpServer(serverIp, port);
    }

    private void CreateTcpServer(IPAddress serverIp, int port)
    {
        _networkManagers.Add(new TcpServerManager(serverIp, port));
    }

    private void CreateUdpServer(IPAddress serverIp, int port)
    {
        _networkManagers.Add(new UdpServerManager(port));
    }

    private void CreateClient(IPAddress serverIp, int port, string typeConecction)
    {
        if (typeConecction == "TCP")
            CreateTcpClient(serverIp,port);
        else
            CreateUdpClient(serverIp,port);
    }

    private void CreateTcpClient(IPAddress serverIp, int port)
    {
        TcpClientManager temp = new TcpClientManager(serverIp, port);
        _networkManagers.Add(temp);
        temp.OnDataReceived += OnDataReceived;
    }

    private void CreateUdpClient(IPAddress serverIp, int port)
    {
        UdpClientManager temp = new UdpClientManager(serverIp, port);
        _networkManagers.Add(temp);
        temp.OnDataReceived += OnDataReceived;
    }
}