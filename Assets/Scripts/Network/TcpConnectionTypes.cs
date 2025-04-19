using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager;

public abstract class TcpConnectionTypes
{
    public Action<byte[]> OnDataReceived;
    public Action OnClientConnected;

    public virtual void Stop() { }
    public virtual void Update() { }
}

public class TcpServerManager : TcpConnectionTypes
{
    private readonly List<TcpConnectedClient> _serverClients = new();
    private readonly TcpListener _listener;

    public TcpServerManager(IPAddress serverIp, int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        _listener.BeginAcceptTcpClient(OnClientConnectToServer, null);
    }

    public override void Stop()
    {
        foreach (var client in _serverClients)
            client.CloseClient();
    }

    public override void Update()
    {
        foreach (var client in _serverClients)
            client.FlushReceivedData();
    }

    public void BroadcastData(byte[] data)
    {
        foreach (var client in _serverClients)
            client.SendData(data);
    }

    private void OnClientConnectToServer(IAsyncResult asyncResult)
    {
        TcpClient client = _listener.EndAcceptTcpClient(asyncResult);
        TcpConnectedClient connectedClient = new TcpConnectedClient(client);
        connectedClient.RecibedData += (data) => OnDataReceived?.Invoke(data);
        connectedClient.DisconectClient += DisconnectClient;

        connectedClient.StartBeginRead();
        _serverClients.Add(connectedClient);

        _listener.BeginAcceptTcpClient(OnClientConnectToServer, null);
    }

    private void DisconnectClient(TcpConnectedClient client)
    {
        if (_serverClients.Contains(client))
            _serverClients.Remove(client);
    }
}

public class TcpClientManager : TcpConnectionTypes
{
    private TcpConnectedClient _client;
    private bool _clientJustConnected;

    public TcpClientManager(IPAddress serverIp, int port)
    {
        _client = new TcpConnectedClient(new TcpClient());
        _client.RecibedData += (data) => OnDataReceived?.Invoke(data);
        _client.DisconectClient += (_) => { };

        _client.StartBeginConnected(serverIp, port, OnConnectClient);
    }

    public override void Stop()
    {
        _client.CloseClient();
    }

    public override void Update()
    {
        if (_clientJustConnected)
        {
            _clientJustConnected = false;
            OnClientConnected?.Invoke();
        }

        _client.FlushReceivedData();
    }

    public void SendDataToServer(byte[] data)
    {
        _client.SendData(data);
    }

    private void OnConnectClient(IAsyncResult asyncResult)
    {
        _client.OnEndConnection(asyncResult);
        _clientJustConnected = true;
    }
}