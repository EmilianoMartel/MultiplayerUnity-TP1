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

            TcpClient tcp = new();
            client = new TcpClientManager(tcp);
            client.Setup(IPAddress.Loopback, port);

            //if (client is TcpClientManager tcpClient)
            //    tcpClient.SetupAsServer();
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
