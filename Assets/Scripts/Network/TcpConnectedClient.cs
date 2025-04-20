using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System;
using System.Net;

public class TcpConnectedClient
{
    private TcpClient client;
    private readonly Queue<byte[]> dataReceived = new();
    private readonly byte[] readBuffer = new byte[5000];
    private readonly object readHandler = new();

    private NetworkStream NetworkStream => client?.GetStream();

    public Action<TcpConnectedClient> DisconectClient;
    public Action<byte[]> RecibedData;

    public TcpConnectedClient(TcpClient client)
    {
        this.client = client;
    }

    public void StartBeginRead()
    {
        NetworkStream?.BeginRead(readBuffer, 0, readBuffer.Length, OnRead, null);
    }

    public void StartBeginConnected(IPAddress serverIp, int port, AsyncCallback onConnectClient)
    {
        client.BeginConnect(serverIp, port, onConnectClient, null);
    }

    private void OnRead(IAsyncResult asyncResult)
    {
        if (NetworkStream.EndRead(asyncResult) == 0)
        {
            DisconectClient?.Invoke(this);
            return;
        }

        lock (readHandler)
        {
            byte[] data = readBuffer.TakeWhile(b => (char)b != '\0').ToArray();
            dataReceived.Enqueue(data);
        }

        Array.Clear(readBuffer, 0, readBuffer.Length);
        NetworkStream.BeginRead(readBuffer, 0, readBuffer.Length, OnRead, null);
    }

    public void SendData(byte[] data)
    {
        NetworkStream?.Write(data, 0, data.Length);
    }

    public void FlushReceivedData()
    {
        lock (readHandler)
        {
            while (dataReceived.Count > 0)
                RecibedData?.Invoke(dataReceived.Dequeue());
        }
    }

    public void OnEndConnection(IAsyncResult asyncResult)
    {
        client.EndConnect(asyncResult);
        StartBeginRead();
    }

    public void CloseClient()
    {
        client?.Close();
    }
}