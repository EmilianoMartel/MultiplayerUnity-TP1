using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using UnityEngine;

public class ServerTypes : INetworkManagment
{
    public event Action OnClientConnected;

    public void Connect(IAsyncResult asyncResult)
    {
        throw new NotImplementedException();
    }

    public virtual void Setup(IPAddress serverIp, int port)
    {
        throw new NotImplementedException();
    }

    public virtual void Stop() { }

    public virtual void Update()
    {
        throw new NotImplementedException();
    }

    public virtual void BroadcastData(byte[] data) { }

    public virtual void DisconnectClient(ClientTypes client) { }
}

public class TcpServerManager : ServerTypes
{
    private readonly List<TcpClientManager> serverClients = new List<TcpClientManager>();
    private TcpListener listener;

    public override void Setup(IPAddress ip, int port)
    {
        listener = new TcpListener(IPAddress.Any, port);

        listener.Start();
        listener.BeginAcceptTcpClient(OnClientConnectToServer, null);
    }

    public override void Update()
    {
        foreach (TcpClientManager client in serverClients)
            client.FlushReceivedData();
    }

    private void OnClientConnectToServer(IAsyncResult asyncResult)
    {
        TcpClient client = listener.EndAcceptTcpClient(asyncResult);
        TcpClientManager connectedClient = new TcpClientManager();

        serverClients.Add(connectedClient);
        listener.BeginAcceptTcpClient(OnClientConnectToServer, null);
    }

    public override void Stop()
    {
        listener.Stop();
    }

    public override void BroadcastData(byte[] data)
    {
        foreach (TcpClientManager client in serverClients)
            client.SendData(data);
    }

    public override void DisconnectClient(ClientTypes client)
    {
        if (client is TcpClientManager tcpClientManager)
            if (serverClients.Contains(tcpClientManager))
                serverClients.Remove(tcpClientManager);
    }
}

public class UdpServerManager : ServerTypes
{

}