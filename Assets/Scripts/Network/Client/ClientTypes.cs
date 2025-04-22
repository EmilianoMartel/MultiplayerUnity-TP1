using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

public interface INetworkManagment
{
    public event Action OnClientConnected;

    public void Setup(IPAddress serverIp, int port);
    public void Connect(IAsyncResult asyncResult);
    public void Stop();
    public void Update();
}

public abstract class ClientTypes : INetworkManagment
{
    protected Queue<byte[]> p_dataReceived = new Queue<byte[]>();
    protected byte[] p_readBuffer = new byte[5000];
    public object p_readHandler = new object();

    public event Action OnClientConnected;

    public virtual void Setup(IPAddress serverIp, int port) { }

    protected virtual void OnRead(IAsyncResult asyncResult) { }

    public virtual void SendData(byte[] data) { }

    public virtual void FlushReceivedData() { }

    public virtual void Connect(IAsyncResult asyncResult) { }

    public virtual void OnEndConnection(IAsyncResult asyncResult) { }

    public virtual void CloseClient() { }

    public virtual void Stop()
    {
        throw new NotImplementedException();
    }

    public virtual void Update()
    {
        throw new NotImplementedException();
    }

    protected void InvokeConnectedClient()
    {
        OnClientConnected?.Invoke();
    }
}

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
        if (NetworkStream.EndRead(asyncResult) == 0)
        {
            TcpManager.Instance.DisconnectClient(this);
            return;
        }

        lock (p_readHandler)
        {
            byte[] data = p_readBuffer.TakeWhile(b => (char)b != '\0').ToArray();
            p_dataReceived.Enqueue(data);
        }

        Array.Clear(p_readBuffer, 0, p_readBuffer.Length);
        NetworkStream.BeginRead(p_readBuffer, 0, p_readBuffer.Length, OnRead, null);
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

public class UdpClientManager : ClientTypes
{
    private UdpClient _client = new();
    private IPEndPoint _endPoint;
    private bool _isConnected = false;

    public override void Setup(IPAddress serverIp, int port)
    {
        _endPoint = new IPEndPoint(serverIp, port);
        _isConnected = true;
        BeginReceive();
    }

    protected override void OnRead(IAsyncResult asyncResult)
    {
        try
        {
            byte[] receivedData = _client.EndReceive(asyncResult, ref _endPoint);

            lock (p_readHandler)
            {
                byte[] data = receivedData.TakeWhile(b => (char)b != '\0').ToArray();
                p_dataReceived.Enqueue(data);
            }

            BeginReceive();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while receiving data: {ex.Message}");
        }
    }

    private void BeginReceive()
    {
        try
        {
            _client.BeginReceive(OnRead, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting receive: {ex.Message}");
        }
    }

    public override void SendData(byte[] data)
    {
        if (!_isConnected)
        {
            Console.WriteLine("UDP client is not connected.");
            return;
        }

        try
        {
            _client.Send(data, data.Length, _endPoint);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending data: {ex.Message}");
        }
    }

    public override void FlushReceivedData()
    {
        lock (p_readHandler)
        {
            while (p_dataReceived.Count > 0)
            {
                byte[] data = p_dataReceived.Dequeue();
                TcpManager.Instance.ReceiveData(data);
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