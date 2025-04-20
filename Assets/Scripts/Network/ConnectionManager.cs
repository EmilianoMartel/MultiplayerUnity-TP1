using System.Net;

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
            return new UdpClientManager();
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
            return new UdpServerManager();
        }
    }
}
