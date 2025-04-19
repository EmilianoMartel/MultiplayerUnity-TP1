using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public abstract class ConnectionTypes
{
    public Action<byte[]> OnDataReceived;
    public Action OnClientConnected;

    public virtual void Stop() { }
    public virtual void Update() { }
}

public class TcpServerManager : ConnectionTypes
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

public class TcpClientManager : ConnectionTypes
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

public class UdpServerManager : ConnectionTypes
{
    private UdpClient _udp;
    private IPEndPoint _listenEndPoint;

    public UdpServerManager(int port)
    {
        _listenEndPoint = new IPEndPoint(IPAddress.Any, port);
        _udp = new UdpClient(_listenEndPoint);

        _udp.BeginReceive(OnReceive, null);
    }

    private void OnReceive(IAsyncResult ar)
    {
        byte[] data = _udp.EndReceive(ar, ref _listenEndPoint);
        OnDataReceived?.Invoke(data);

        _udp.BeginReceive(OnReceive, null);
    }

    public void BroadcastData(byte[] data, List<IPEndPoint> clients)
    {
        foreach (var ep in clients)
            _udp.Send(data, data.Length, ep);
    }

    public override void Stop()
    {
        _udp?.Close();
    }
}

public class UdpClientManager : ConnectionTypes
{
    private UdpClient _udp;
    private IPEndPoint _serverEndPoint;

    public UdpClientManager(IPAddress serverIp, int port)
    {
        _udp = new UdpClient();
        _serverEndPoint = new IPEndPoint(serverIp, port);

        _udp.Connect(_serverEndPoint);
        _udp.BeginReceive(OnReceive, null);
    }

    private void OnReceive(IAsyncResult ar)
    {
        IPEndPoint remote = null;
        byte[] data = _udp.EndReceive(ar, ref remote);

        OnDataReceived?.Invoke(data);

        _udp.BeginReceive(OnReceive, null);
    }

    public void SendDataToServer(byte[] data)
    {
        _udp.Send(data, data.Length);
    }

    public override void Stop()
    {
        _udp?.Close();
    }
}