using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System;
using System.Linq;

public class UdpServerManager : ServerTypes
{
    private UdpClient _listener;
    private IPEndPoint _localEndPoint;

    private readonly List<UdpClientManager> _serverClients = new List<UdpClientManager>();

    public override void Setup(IPAddress ip, int port)
    {
        _localEndPoint = new IPEndPoint(ip, 0);

        _listener = new UdpClient(port);

        BeginReceive();
    }

    public override void Update()
    {
        foreach (UdpClientManager client in _serverClients)
        {
            client.FlushReceivedData();
        }
    }

    private void BeginReceive()
    {
        try
        {
            UnityEngine.Debug.Log("UDP server started recive");
            _listener.BeginReceive(OnReceiveData, null);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log($"Error starting UDP receive: {ex.Message}");
        }
    }

    private void OnReceiveData(IAsyncResult asyncResult)
    {
        UnityEngine.Debug.Log($"Server recived data");
        try
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] receivedData = _listener.EndReceive(asyncResult, ref remoteEP);
            UnityEngine.Debug.Log($"Server recived data from {remoteEP}");

            if (!_serverClients.Any(c => c.RemoteEndPoint.Equals(remoteEP)))
            {
                UnityEngine.Debug.Log("New client connected");
                var dummyClient = new UdpClientManager(_listener);
                dummyClient.Setup(remoteEP.Address, remoteEP.Port);
                _serverClients.Add(dummyClient);
            }

            foreach (var client in _serverClients)
            {
                UnityEngine.Debug.Log($"Sended to {client.RemoteEndPoint}");
                _listener.Send(receivedData, receivedData.Length, client.RemoteEndPoint);
            }

            BeginReceive();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log($"Error UDP: {ex.Message}");
        }
    }

    public override void Stop()
    {
        _serverClients.Clear();
        _listener.Close();
    }

    public override void BroadcastData(byte[] data)
    {
        if (_localEndPoint != null)
        {
            _listener.Send(data, data.Length, _localEndPoint);
        }
    }

    public override void DisconnectClient(ClientTypes client)
    {
    }
}
