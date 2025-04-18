using NUnit.Framework;
using System.Collections.Generic;
using System.Net;
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
    private List<TcpConnectionManager> _tcpManagers = new();

    private bool _wasSetted = false;

    private void Update()
    {
        if(!_wasSetted) return;

        foreach (TcpConnectionManager tcp in _tcpManagers)
        {
            tcp.Update();
        }
    }

    /// <summary>
    /// This function return false in case the conection or election fail.
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    public bool TcpSetup(Role role, IPAddress serverIp, int port, string name)
    {
        if(name != "") _name = name;

        switch (role)
        {
            case Role.None:
                return false;
            case Role.Client:
                _wasSetted = true;
                _tcpManagers.Add(new TcpClientManager(serverIp,port));
                return true;
            case Role.Server:
                _wasSetted = true;
                _tcpManagers.Add(new TcpServerManager(serverIp, port));
                return true;
            case Role.ServerClient:
                _wasSetted = true;
                _tcpManagers.Add(new TcpServerManager(serverIp, port));
                _tcpManagers.Add(new TcpClientManager(serverIp, port));
                return true;
        }

        return false;
    }
}