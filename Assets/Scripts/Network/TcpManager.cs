using System;
using System.Net;


public class TcpManager : MonoBehaviourSingleton<TcpManager>
{
    private string _name = "Anonym";
    private ServerTypes _server = null;
    private ClientTypes _client = null;

    public string NameID { get { return _name; } }
    public bool IsServer => _server != null;

    public event Action GoToChatScreen;
    public event Action GoToServerScren;

    public event Action<byte[]> OnDataReceived;

    private void Update()
    {
        if (_server != null)
        {
            _server?.Update();
        }
        else
        {
            _client?.Update();
        }
    }

    private void OnDestroy()
    {
        _server?.Stop();
        _client?.Stop();
    }

    public void NetworkSetup(Role role, IPAddress ipAddress, int port, string name, string connectionType)
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
                StartServerClient(port, connectionType);
                break;
            default:
                break;
        }
    }

    private void StartServerClient(int port, string connectionType)
    {
        _server = ConnectionManager.CreateServerClient(port,connectionType, out _client);
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