using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public abstract class TcpConnectionManager
{
    public Action<byte[]> OnDataReceived;
    public Action OnClientConnected;

    public TcpConnectionManager(IPAddress serverIp, int port){}
    public virtual void Stop(){}
    public virtual void Update() { }
}

public class TcpServerManager : TcpConnectionManager
{
    private readonly List<TcpConnectedClient> _serverClients = new List<TcpConnectedClient>();
    private TcpListener _listener;

    public TcpServerManager(IPAddress serverIp, int port) : base(serverIp, port)
    {
        _listener = new TcpListener(IPAddress.Any, port);

        _listener.Start();
        _listener.BeginAcceptTcpClient(OnClientConnectToServer, null);
    }

    public override void Stop()
    {
        foreach (TcpConnectedClient client in _serverClients)
            client.CloseClient();
    }

    public override void Update() 
    {
        foreach (TcpConnectedClient client in _serverClients)
            client.FlushReceivedData();
    }

    public void DisconnectClient(TcpConnectedClient client)
    {
        if (_serverClients.Contains(client))
            _serverClients.Remove(client);
    }

    public void ReceiveData(byte[] data)
    {
        OnDataReceived?.Invoke(data);
    }

    private void OnClientConnectToServer(IAsyncResult asyncResult)
    {
        TcpClient client = _listener.EndAcceptTcpClient(asyncResult);
        TcpConnectedClient connectedClient = new TcpConnectedClient(client);

        _serverClients.Add(connectedClient);
        _listener.BeginAcceptTcpClient(OnClientConnectToServer, null);
    }
}

public class TcpClientManager : TcpConnectionManager
{
    private TcpConnectedClient _connectedClient;

    private bool _clientJustConnected;

    public TcpClientManager(IPAddress serverIp, int port) : base(serverIp, port)
    {
        TcpClient client = new TcpClient();

        _connectedClient = new TcpConnectedClient(client);

        client.BeginConnect(serverIp, port, OnConnectClient, null);
    }

    public override void Stop()
    {
        _connectedClient.CloseClient();
    }

    public override void Update()
    {
        if (_clientJustConnected)
            OnClientConnected?.Invoke();

        _connectedClient?.FlushReceivedData();
    }
    public void SendDataToServer(byte[] data)
    {
        _connectedClient.SendData(data);
    }

    private void OnConnectClient(IAsyncResult asyncResult)
    {
        _connectedClient.OnEndConnection(asyncResult);
        _clientJustConnected = true;
    }
}