using System.Net.Sockets;
using System.Net;
using System;
using UnityEngine;

public class TcpClientManager : ClientTypes
{
    private TcpClient _client;

    public NetworkStream NetworkStream => _client?.GetStream();
    public byte[] ReadBuffer { get { return p_readBuffer; } }

    private bool clientJustConnected = false;

    public TcpClientManager(TcpClient client)
    {
        _client = client;
    }

    public override void Setup(IPAddress serverIp, int port)
    {
        _client.BeginConnect(serverIp, port, Connect, null);
    }

    public override void Connect(IAsyncResult asyncResult)
    {
        OnEndConnection(asyncResult);
        clientJustConnected = true;
    }

    protected override void OnRead(IAsyncResult asyncResult)
    {
        UnityEngine.Debug.Log("Message received in client");
        var bytesRead = NetworkStream.EndRead(asyncResult);

        if (bytesRead <= 0)
        {
            UnityEngine.Debug.Log($"Message received but no bytes read");
            TcpManager.Instance.DisconnectClient(this);
            return;
        }

        lock (p_readHandler)
        {
            UnityEngine.Debug.Log("Processing message received in client");
            var dataToBroadcast = new byte[bytesRead];
            Array.Copy(ReadBuffer, dataToBroadcast, bytesRead);
            p_dataReceived.Enqueue(dataToBroadcast);
            UnityEngine.Debug.Log($"Message enqueued in client: {p_dataReceived.Count}");
        }

        Array.Clear(ReadBuffer, 0, ReadBuffer.Length);
        NetworkStream?.BeginRead(ReadBuffer, 0, ReadBuffer.Length, OnRead, null);
        UnityEngine.Debug.Log("Buffer cleared and client listening again");
    }

    public override void SendData(byte[] data)
    {
        NetworkStream.Write(data, 0, data.Length);
    }

    public override void FlushReceivedData()
    {
        lock (p_readHandler)
        {
            while (p_dataReceived.Count > 0)
            {
                byte[] data = p_dataReceived.Dequeue();
                TcpManager.Instance.ReceiveData(data);
                UnityEngine.Debug.Log("Message dequeued in client");
            }
        }
    }

    public override void OnEndConnection(IAsyncResult asyncResult)
    {
        _client.EndConnect(asyncResult);
        NetworkStream.BeginRead(p_readBuffer, 0, p_readBuffer.Length, OnRead, null);
    }

    public override void CloseClient()
    {
        _client.Close();
    }

    public override void Stop()
    {
        CloseClient();
    }

    public override void Update()
    {
        if (clientJustConnected)
            InvokeConnectedClient();

        FlushReceivedData();
    }
}