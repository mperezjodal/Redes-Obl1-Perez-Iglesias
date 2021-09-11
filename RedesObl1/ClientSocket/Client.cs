using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SocketUtils;
namespace ClientSocket
{
    public class Client
    {
        private const string ServerIpAddress = "127.0.0.1";
        private const int ServerPort = 6000;
        private const string ClientIpAddress = "127.0.0.1";
        private const int ClientPort = 0;
        
        static void Main(string[] args)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Parse(ClientIpAddress), ClientPort);
            
            clientSocket.Bind(clientEndPoint);
            Console.WriteLine("Conectando al servidor...");
            
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(ServerIpAddress), ServerPort);
            clientSocket.Connect(serverEndPoint);
            Console.WriteLine("Conectado al servidor.");
            
            new Thread(() => Utils.ReciveData(clientSocket)).Start();
            Utils.SendData(clientSocket);
        }
    }
}