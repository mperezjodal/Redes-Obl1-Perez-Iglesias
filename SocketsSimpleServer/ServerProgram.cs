using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketsSimpleServer
{
    class ServerProgram
    {
        private const string ServerIpAddress = "127.0.0.1";
        private const int ProtocolFixedSize = 4;
        private const int ServerPort = 6000;
        private const int Backlog = 1;

        static void Main(string[] args)
        {
            Socket serverSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            IPEndPoint serverIpEndPoint = new IPEndPoint(
                IPAddress.Parse(ServerIpAddress),
                ServerPort);

            // Asocio el socket a un par ip / puerto (endpoint)
            serverSocket.Bind(serverIpEndPoint);

            // Dejo al socket en estado de escucha y escucho conexiones
            // el backlog es la cantidad de clientes que podemos tener sin atender
            // conectados al servidor
            serverSocket.Listen(Backlog);
            Console.WriteLine("Start listening for client");

            // Capturo al primer cliente que se quiera conectar
            Socket clientSocket = serverSocket.Accept();
            serverSocket.Close();

            new Thread(() => SocketUtils.Listen(clientSocket)).Start();
            
            while (true)
            {
                //1 Leo el mensaje
                string message = Console.ReadLine();

                //2 Codifico el mensaje a bytes
                byte[] data = Encoding.UTF8.GetBytes(message);

                //3 Leo el largo del mensaje codificado
                //    (largo codificado <> largo mensaje)
                int length = data.Length;

                //4 Codifico dicho largo a bytes
                byte[] dataLength = BitConverter.GetBytes(length);

                //5 Envío el largo en bytes
                SocketUtils.SendData(clientSocket, dataLength);
            
                //6 Envío el mensaje
                SocketUtils.SendData(clientSocket, length);
                
            }
        }
    }
}
