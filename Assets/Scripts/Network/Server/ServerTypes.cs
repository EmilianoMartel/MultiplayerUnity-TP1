using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using UnityEngine;

public class ServerTypes : INetworkManagment
{
    protected List<MessageData> messages = new List<MessageData>();

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

    protected MessageData AddMessageToList(MessageData data)
    {
        MessageData temp = data;
        temp.MessageID = messages.Count;
        messages.Add(temp);
        return temp;
    }
}