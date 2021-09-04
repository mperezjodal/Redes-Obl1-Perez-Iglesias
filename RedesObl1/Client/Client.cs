using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public class Client
    {
        static void Main(string[] args)
        {
            StartClient();
            Console.WriteLine("Continue...");
            Console.Read();
        }

        public static void StartClient()
        {
            int port = 9000;
            // 127.0.0.1:9000
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Loopback, port);

            Socket sender = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Conectarse desde un socket client
            sender.Connect(ipEndPoint);
            Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());

            byte[] message = Encoding.ASCII.GetBytes("This is an example of connection. <EOF>");

            // enviar mensaje al server
            int bytesSent = sender.Send(message);

            // recibir respuesta del server
            byte[] bytes = new byte[1024];
            int bytesReceived = sender.Receive(bytes);

            // Parsear texto recibido
            if (bytesReceived > 0)
            {
                Console.WriteLine("Mensaje recibido = {0}",
                    Encoding.ASCII.GetString(bytes, 0, bytesReceived));
            }

            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }
    }
}