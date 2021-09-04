using System;

namespace Server
{
    class Server
    {
        static void Main(string[] args)
        {
            SocketServer server = new SocketServer();
            server.StartListening();
        }
    }
}