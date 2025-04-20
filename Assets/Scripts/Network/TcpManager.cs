using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;


public class TcpManager : MonoBehaviourSingleton<TcpManager>
{
    private string _name = "Anonym";
    private ServerTypes _server = null;
    private ClientTypes _client = null;

    public string NameID { get { return _name; } }
    
    public event Action GoToChatScreen;
    public event Action GoToServerScren;

    public event Action<byte[]> OnDataReceived;

    private void Update()
    {
        _server?.Update();
        _client?.Update();
    }

    private void OnDestroy()
    {
        /*
        listener?.Stop();
        _server?.Stop();
        foreach (TcpConnectedClient client in serverClients)
            client.CloseClient();

        connectedClient.CloseClient();
    */}

    public void TcpSetup(Role role, IPAddress ipAddress, int port, string name, string connectionType)
    {
        GoToChatScreen?.Invoke();
        _name = name;
        switch (role)
        {
            case Role.None:
                break;
            case Role.Client:
                StartClient(ipAddress,port,connectionType);
                break;
            case Role.Server:
                StartServer(port, connectionType);
                break;
            case Role.ServerClient:
                StartServer(port, connectionType);
                break;
            default:
                break;
        }
    }


    private void StartServer(int port, string connectionType)
    {
        _server = ConnectionManager.CreateServer(port,connectionType);
    }

    private void StartClient(IPAddress serverIp, int port, string connectionType)
    {
        _client = ConnectionManager.CreateClient(serverIp,port,connectionType);
    }

    public void ReceiveData(byte[] data)
    {
        OnDataReceived?.Invoke(data);
    }

    public void DisconnectClient(ClientTypes client)
    {
        _server.DisconnectClient(client);
    }

    public void BroadcastData(byte[] data)
    {
        _server.BroadcastData(data);
    }

    public void SendDataToServer(byte[] data)
    {
        _client.SendData(data);
    }
}