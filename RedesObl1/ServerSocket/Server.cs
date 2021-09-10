using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SocketUtils;

namespace ServerSocket
{
    public class Server
    {
        private const string ServerIpAddress = "127.0.0.1";
        private const int ProtocolFixedSize = 4;
        private const int ServerPort = 6000;
        private const int Backlog = 2;

        static void Main(string[] args)
        {
            Socket serverSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            IPEndPoint serverIpEndPoint = new IPEndPoint(
                IPAddress.Parse(ServerIpAddress),
                ServerPort);

            serverSocket.Bind(serverIpEndPoint);

            serverSocket.Listen(Backlog);
            Console.WriteLine("Start listening for client");

            Socket clientSocket = serverSocket.Accept();
            serverSocket.Close();
            new Thread(() => Utils.ReciveData(clientSocket)).Start();
            Utils.SendData(clientSocket);
        }
    }
}