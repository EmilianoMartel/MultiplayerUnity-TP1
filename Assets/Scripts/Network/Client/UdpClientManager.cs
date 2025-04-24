using System.Linq;
using System.Net.Sockets;
using System.Net;
using System;

public class UdpClientManager : ClientTypes
{
    private UdpClient _client;
    private IPEndPoint _endPoint;

    private bool _isConnected = false;

    public IPEndPoint RemoteEndPoint => _endPoint;

    public UdpClientManager(UdpClient client)
    {
        _client = client;
    }

    public override void Setup(IPAddress serverIp, int port)
    {
        UnityEngine.Debug.Log("Setup Client UDP");

        _endPoint = new IPEndPoint(serverIp, port);
        _isConnected = true;
        BeginReceive();
    }

    protected override void OnRead(IAsyncResult asyncResult)
    {
        UnityEngine.Debug.Log("Message received in client");

        try
        {
            byte[] receivedData = _client.EndReceive(asyncResult, ref _endPoint);

            lock (p_readHandler)
            {
                byte[] data = receivedData.TakeWhile(b => (char)b != '\0').ToArray();
                p_dataReceived.Enqueue(data);
                UnityEngine.Debug.Log("Message enqueue");
            }

            BeginReceive();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log($"Error while receiving data: {ex.Message}");
        }
    }

    private void BeginReceive()
    {
        try
        {
            UnityEngine.Debug.Log("Client setted to receive");
            _client.BeginReceive(OnRead, null);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log($"Error starting receive: {ex.Message}");
        }
    }

    public override void SendData(byte[] data)
    {
        if (!_isConnected)
        {
            UnityEngine.Debug.Log("Client is´t connected");
            return;
        }

        UnityEngine.Debug.Log("Client sended data");
        _client.Send(data, data.Length, _endPoint);
    }

    public override void FlushReceivedData()
    {
        lock (p_readHandler)
        {
            while (p_dataReceived.Count > 0)
            {
                byte[] data = p_dataReceived.Dequeue();
                TcpManager.Instance.ReceiveData(data);
                UnityEngine.Debug.Log("Cliend dequeue data");
            }
        }
    }

    public override void OnEndConnection(IAsyncResult asyncResult)
    {
    }

    public override void CloseClient()
    {
        _client.Close();
        _isConnected = false;
    }

    public override void Stop()
    {
        CloseClient();
    }

    public override void Update()
    {
        FlushReceivedData();
    }
}