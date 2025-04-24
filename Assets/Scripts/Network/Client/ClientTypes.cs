using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Unity.IO.LowLevel.Unsafe;

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