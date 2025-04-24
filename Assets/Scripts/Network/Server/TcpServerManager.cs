using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System;
using UnityEngine;

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
        Debug.Log("Client connected to server");
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

        if (bytesRead <= 0)
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
            Array.Copy(clientManager.ReadBuffer, dataToRead, bytesRead);
        }

        Debug.Log("Sending message");
        BroadcastData(dataToRead);
        Array.Clear(clientManager.ReadBuffer, 0, clientManager.ReadBuffer.Length);
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
        if (client is TcpClientManager tcpClientManager && serverClients.Contains(tcpClientManager))
        {
            serverClients.Remove(tcpClientManager);
            tcpClientManager.Stop();
        }
    }
}