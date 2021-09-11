using System;
using System.ComponentModel.Design;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SocketUtils;
using Domain;

namespace ServerSocket
{
    public class Server
    {
        private const string ServerIpAddress = "127.0.0.1";
        private const int ProtocolFixedSize = 4;
        private const int ServerPort = 6000;
        private const int Backlog = 100;
        private static GameSystem GameSystem;
        private static bool _exit = false;
        static List<Socket> _clients = new List<Socket>();

        static void Main(string[] args)
        {
            GameSystem = new GameSystem();
            _clients = new List<Socket>();

            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverIpEndPoint = new IPEndPoint(IPAddress.Parse(ServerIpAddress), ServerPort);
            serverSocket.Bind(serverIpEndPoint);
            serverSocket.Listen(Backlog);
            
            var threadServer = new Thread(()=> ListenForConnections(serverSocket));
            threadServer.Start();
        }

        private static void ListenForConnections(Socket socketServer)
        {
            Console.WriteLine("Esperando por conexiones...");
            while (!_exit)
            {
                try
                {
                    var clientConnected = socketServer.Accept();
                    _clients.Add(clientConnected);
                    Console.WriteLine("Nueva conexión aceptada.");
                    var threadClient = new Thread(() => HandleClient(clientConnected));
                    threadClient.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _exit = true;
                }
            }
            Console.WriteLine("Saliendo...");
        }

        private static void HandleClient(Socket clientSocket)
        {
            DisplayMenu(clientSocket);

            while (!_exit)
            {
                byte[] dataLength = new byte[ProtocolFixedSize];
                clientSocket.Receive(dataLength);
                int length = BitConverter.ToInt32(dataLength);
                byte[] data = new byte[length];
                clientSocket.Receive(data);
                string clientMessage = Encoding.UTF8.GetString(data);

                try
                {
                    switch (clientMessage)
                    {
                        case "1":
                            Utils.SendData(clientSocket,  "Seleccionaste: Ver catálogo de juegos");
                            break;
                        case "2":
                            Utils.SendData(clientSocket,  "Seleccionaste: Adquirir juego");
                            break;
                        case "3":
                            Utils.SendData(clientSocket,  "Seleccionaste: Publicar juego");
                            break;
                        case "4":
                            Utils.SendData(clientSocket,  "Seleccionaste: Publicar calificación de un juego");
                            break;
                        case "5":
                            Utils.SendData(clientSocket,  "Seleccionaste: Buscar juegos");
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}..");    
                }
            }
        }


        public static void DisplayMenu(Socket socket) {
            Utils.SendData(socket,  "###############################################");
            Utils.SendData(socket,  "#                                             #");
            Utils.SendData(socket, @"#       |¯\ /¯| | ____| |¯\ |¯| | | | |       #");
            Utils.SendData(socket, @"#       |  ¯  | | __|   |  \| | | |_| |       #");
            Utils.SendData(socket, @"#       |     | |_____| | \   | |_____|       #");
            Utils.SendData(socket,  "#                                             #");
            Utils.SendData(socket,  "#   Seleccione una opción:                    #");
            Utils.SendData(socket,  "#                                             #");
            Utils.SendData(socket,  "#   1-   Ver catálogo de juegos               #");
            Utils.SendData(socket,  "#   2-   Adquirir juego                       #");
            Utils.SendData(socket,  "#   3-   Publicar juego                       #");
            Utils.SendData(socket,  "#   4-   Publicar calificación de un juego    #");
            Utils.SendData(socket,  "#   5-   Buscar juegos                        #");
            Utils.SendData(socket,  "#        exit                                 #");
            Utils.SendData(socket,  "#                                             #");
            Utils.SendData(socket,  "###############################################");
        }
    }
}