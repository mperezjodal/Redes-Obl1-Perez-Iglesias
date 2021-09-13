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
            var connected = true;
            Console.WriteLine("Conectado al servidor.");
            DisplayMenu();
            
            while (connected)
            {
                var option = Console.ReadLine();
                switch (option)
                {
                    case "exit":
                        clientSocket.Shutdown(SocketShutdown.Both);
                        clientSocket.Close();
                        connected = false;
                        break;
                    case "1":
                    case "2":
                    case "3":
                    case "4":
                    case "5":  
                        Utils.SendCommand(clientSocket, Int32.Parse(option));
                        break;
                    default:
                        Console.WriteLine("option invalida");
                        break;
                }
            }

            new Thread(() => Utils.ReciveData(clientSocket)).Start();
            Utils.SendData(clientSocket);
        }

        public static void DisplayMenu() {
            Console.WriteLine(@"###############################################");
            Console.WriteLine(@"#                                             #");
            Console.WriteLine(@"#       |¯\ /¯| | ____| |¯\ |¯| | | | |       #");
            Console.WriteLine(@"#       |  ¯  | | __|   |  \| | | |_| |       #");
            Console.WriteLine(@"#       |     | |_____| | \   | |_____|       #");
            Console.WriteLine(@"#                                             #");
            Console.WriteLine(@"#   Seleccione una opción:                    #");
            Console.WriteLine(@"#                                             #");
            Console.WriteLine(@"#   1-   Ver catálogo de juegos               #");
            Console.WriteLine(@"#   2-   Adquirir juego                       #");
            Console.WriteLine(@"#   3-   Publicar juego                       #");
            Console.WriteLine(@"#   4-   Publicar calificación de un juego    #");
            Console.WriteLine(@"#   5-   Buscar juegos                        #");
            Console.WriteLine(@"#        exit                                 #");
            Console.WriteLine(@"#                                             #");
            Console.WriteLine(@"###############################################");
        }
    }
}