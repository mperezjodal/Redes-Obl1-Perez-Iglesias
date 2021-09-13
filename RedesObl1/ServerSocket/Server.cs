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
            while (!_exit)
            {
                var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                   HeaderConstants.DataLength;
                var buffer = new byte[headerLength];
                try
                {
                    ReceiveData(clientSocket, headerLength, buffer);
                    var header = new Header();
                    header.DecodeData(buffer);
                    switch (header.ICommand)
                    {
                        case 1:
                            Console.Write("Seleccionaste: Ver catálogo de juegos");
                            break;
                        case 2:
                            Utils.SendData(clientSocket,  "Seleccionaste: Adquirir juego");
                            break;
                        case 3:
                            Utils.SendData(clientSocket,  "Seleccionaste: Publicar juego");
                            break;
                        case 4:
                            Utils.SendData(clientSocket,  "Seleccionaste: Publicar calificación de un juego");
                            break;
                        case 5:
                            Utils.SendData(clientSocket,  "Seleccionaste: Buscar juegos");
                            break;
                        // case "exit":
                        //     Utils.SendData(clientSocket,  "Terminó la conexión.");
                        //     _exit = true;
                        //     break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}..");    
                }
            }
        }

        private static void ReceiveData(Socket clientSocket,  int Length, byte[] buffer)
        {
            var iRecv = 0;
            while (iRecv < Length)
            {
                try
                {
                    var localRecv = clientSocket.Receive(buffer, iRecv, Length - iRecv, SocketFlags.None);
                    if (localRecv == 0) // Si recieve retorna 0 -> la conexion se cerro desde el endpoint remoto
                    {
                        if (!_exit)
                        {
                            clientSocket.Shutdown(SocketShutdown.Both);
                            clientSocket.Close();
                        }
                        else
                        {
                            throw new Exception("Server is closing");
                        }
                    }

                    iRecv += localRecv;
                }
                catch (SocketException se)
                {
                    Console.WriteLine(se.Message);
                    return;
                }
            }
        }
    }
}