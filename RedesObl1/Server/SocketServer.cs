using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    public class SocketServer
    {
        private readonly int _portNumber = 9000;
        private readonly int _backLog = 10;

        public void StartListening()
        {
            // 127.0.0.1:9000
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, _portNumber);

            // Crear Socket TCP/IP
            Socket listener = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Asociar el socket con la direccion ip y el puerto
            listener.Bind(endPoint);
            // escuchar por conexiones entrantes
            listener.Listen(_backLog);

            while (true)
            {
                Console.WriteLine("Esperando por conexiones....");
                var handler = listener.Accept();
                Thread threadProcessor = new Thread(() => HandleReceivedClients(handler));
                threadProcessor.Start();
            }

        }

        private void HandleReceivedClients(Socket handler)
        {
            byte[] bytes = new byte[1024];

            string data = null;

            while (true)
            {
                int bytesRec = handler.Receive(bytes);
                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                if (data.IndexOf("*****") > -1)
                {
                    break;
                }
            }
            Console.WriteLine("Texto recibido : {0}", data);

            // Echo the data back to the client.  
            byte[] msg = Encoding.ASCII.GetBytes("mensaje desde el server...");
            handler.Send(msg);

            handler.Shutdown(SocketShutdown.Both);         

            handler.Close();
        }

    }
}