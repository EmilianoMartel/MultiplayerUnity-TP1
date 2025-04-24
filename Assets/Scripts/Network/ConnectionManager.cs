using System.Net;
using System.Net.Sockets;

public class ConnectionManager
{
    public static ClientTypes CreateClient(IPAddress serverIp, int port, string connectionType)
    {
        if(connectionType == "TCP")
        {
            TcpClient client = new();
            TcpClientManager tcpClientManager = new TcpClientManager(client);
            tcpClientManager.Setup(serverIp,port);
            return tcpClientManager;
        }
        else
        {
            UdpClient udpClient = new();
            UdpClientManager client = new UdpClientManager(udpClient);
            client.Setup(serverIp, port);
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

            TcpClient tcp = new();
            client = new TcpClientManager(tcp);
            client.Setup(IPAddress.Loopback, port);

            return server;
        }
        else
        {
            UdpServerManager server = new UdpServerManager();
            server.Setup(IPAddress.Any, port);

            IPAddress ip = IPAddress.Parse("127.0.0.1");
            UdpClient udpClient = new();

            client = new UdpClientManager(udpClient);
            client.Setup(ip, port);
            return server;
        }
    }
}
