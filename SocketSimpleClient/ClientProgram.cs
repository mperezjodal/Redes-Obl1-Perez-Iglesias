using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketSimpleClient
{
    class ClientProgram
    {
        private const string ServerIpAddress = "127.0.0.1";
        private const int ServerPort = 6000;
        private const string ClientIpAddress = "127.0.0.1";
        private const int ClientPort = 0;
        private const int ProtocolFixedSize = 4;

        static void Main(string[] args)
        {
            Socket clientSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            IPEndPoint clientEndPoint = new IPEndPoint(
                IPAddress.Parse(ClientIpAddress),
                ClientPort);

            clientSocket.Bind(clientEndPoint);

            Console.WriteLine("Trying to connect to server...");
            
            IPEndPoint serverEndPoint = new IPEndPoint(
                IPAddress.Parse(ServerIpAddress),
                ServerPort);

            clientSocket.Connect(serverEndPoint);

            new Thread(() => SocketUtils.Listen(clientSocket)).Start();

            Console.WriteLine("Connected to server");

            // esto es un ejemplo para mostrar el pasaje de datos
            // por ende es válido un while (true) para hacer más sencilla la tarea
            // el while (true) nunca sería válido en un trabajo que requiera corrección
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
                clientSocket.Send(dataLength);
                //6 Envío el mensaje
                clientSocket.Send(data);
            }
        }
    }
}
