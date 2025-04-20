using System.Net;
using UnityEditor.PackageManager;

public class ConnectionManager
{
    public static ClientTypes CreateClient(IPAddress serverIp, int port, string connectionType)
    {
        if(connectionType == "TCP")
        {
            TcpClientManager tcpClientManager = new TcpClientManager();
            tcpClientManager.Setup(serverIp,port);
            return tcpClientManager;
        }
        else
        {
            UdpClientManager client = new UdpClientManager();
            client.Setup(IPAddress.Any, port);
            return client;
        }
    }

    public static ServerTypes CreateServer(int port, string connectionType)
    {
        if (connectionType == "TCP")
        {
            TcpServerManager server = new TcpServerManager();
            server.Setup(IPAddress.Any, port);
            return server;
        }
        else
        {
            UdpServerManager server = new UdpServerManager();
            server.Setup(IPAddress.Any, port);
            return server;
        }
    }
    
    public static ServerTypes CreateServerClient(int port, string connectionType, out ClientTypes client)
    {
        if (connectionType == "TCP")
        {
            TcpServerManager server = new TcpServerManager();
            server.Setup(IPAddress.Any, port);
            client = new TcpClientManager();
            client.Setup(IPAddress.Any, port);
            if (client is TcpClientManager tcpClient)
                tcpClient.SetupAsServer();
            return server;
        }
        else
        {
            UdpServerManager server = new UdpServerManager();
            server.Setup(IPAddress.Any, port);
            client = new UdpClientManager();
            client.Setup(IPAddress.Any, port);
            return server;
        }
    }
}
