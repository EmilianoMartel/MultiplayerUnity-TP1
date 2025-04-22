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

    protected virtual void OnRead(IAsyncResult asyncResult) { }

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
        //foreach (TcpClientManager client in serverClients)
        //    client.FlushReceivedData();
    }

    private void OnClientConnectToServer(IAsyncResult asyncResult)
    {
        TcpClient client = listener.EndAcceptTcpClient(asyncResult);
        TcpClientManager connectedClient = new TcpClientManager(client);

        serverClients.Add(connectedClient);

        connectedClient.NetworkStream.BeginRead(connectedClient.ReadBuffer, 0, connectedClient.ReadBuffer.Length, OnRead, connectedClient);

        listener.BeginAcceptTcpClient(OnClientConnectToServer, null);
    }

    public override void Stop()
    {
        listener.Stop();
        foreach (TcpClientManager client in serverClients)
            client.Stop();
    }

    protected override void OnRead(IAsyncResult asyncResult) 
    {
        var clientManager = (TcpClientManager)asyncResult.AsyncState;
        
        var bytesRead = clientManager.NetworkStream.EndRead(asyncResult);

        if(bytesRead <= 0)
        {
            Debug.Log("Disconected client");
            DisconnectClient(clientManager);
            return;
        }

        byte[] dataToRead;

        lock (clientManager.p_readHandler)
        {
            Debug.Log("Processing message");
            dataToRead = new byte[bytesRead];
            Array.Copy(clientManager.ReadBuffer,dataToRead,bytesRead);
        }

        Debug.Log("Sending message");
        BroadcastData(dataToRead);
        Array.Clear(clientManager.ReadBuffer,0,clientManager.ReadBuffer.Length);
        clientManager.NetworkStream.BeginRead(clientManager.ReadBuffer, 0, clientManager.ReadBuffer.Length, OnRead, clientManager);
    }

    public override void BroadcastData(byte[] data)
    {
        if (serverClients.Count == 0) return;

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
    private UdpClient _listener;
    private IPEndPoint _endPoint;
    private readonly List<UdpClientManager> serverClients = new List<UdpClientManager>();

    public override void Setup(IPAddress ip, int port)
    {
        _endPoint = new IPEndPoint(ip, port);
        _listener = new UdpClient(_endPoint);

        BeginReceive();
    }

    public override void Update()
    {
        foreach (UdpClientManager client in serverClients)
        {
            client.FlushReceivedData();
        }
    }

    private void BeginReceive()
    {
        try
        {
            _listener.BeginReceive(OnReceiveData, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting UDP receive: {ex.Message}");
        }
    }

    private void OnReceiveData(IAsyncResult asyncResult)
    {
        try
        {
            IPEndPoint receivedEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] receivedData = _listener.EndReceive(asyncResult, ref receivedEndPoint);

            lock (serverClients)
            {
                foreach (var client in serverClients)
                {
                    client.SendData(receivedData);
                }
            }

            BeginReceive();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing UDP receive: {ex.Message}");
        }
    }

    public override void Stop()
    {
        _listener.Close();
    }

    public override void BroadcastData(byte[] data)
    {
        if (_endPoint != null)
        {
            _listener.Send(data, data.Length, _endPoint);
        }
    }

    public override void DisconnectClient(ClientTypes client)
    {
    }
}