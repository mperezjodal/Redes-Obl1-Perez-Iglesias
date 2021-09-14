using System.Globalization;
using System;
using System.ComponentModel.Design;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SocketUtils;
using Domain;
using ProtocolLibrary;

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
            GameSystem.AddGame(new Game {
                Title = "FIFA",
                Genre = "Sports",
                Synopsis = "football game",
                Rating = 10
            });
            GameSystem.AddGame(new Game {
                Title = "COD",
                Genre = "War",
                Synopsis = "war game",
                Rating = 9
            });
            _clients = new List<Socket>();

            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverIpEndPoint = new IPEndPoint(IPAddress.Parse(ServerIpAddress), ServerPort);
            serverSocket.Bind(serverIpEndPoint);
            serverSocket.Listen(Backlog);
            
            var threadServer = new Thread(()=> ListenForConnections(serverSocket));
            threadServer.Start();
            
            while (!_exit)
            {
                Display.ServerMenu();
                var option = Console.ReadLine();
                switch (option)
                {
                    case "exit":
                        _exit = true;
                        serverSocket.Close(0);
                        foreach (var client in _clients)
                        {
                            client.Shutdown(SocketShutdown.Both);
                            client.Close();
                        }
                        var fakeSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
                        fakeSocket.Connect("127.0.0.1",20000);
                        break;
                    case "1": // ver juegos
                        break;
                    case "2": // adquirir   
                        break;
                    case "3": // publicar juego
                        break;
                    case "4": // publicar califiacion
                        break;
                    case "5": // buscar juegos
                        break;
                    default:
                        Console.WriteLine("option invalida");
                        break;
                }
            }
        }

        private static void ListenForConnections(Socket socketServer)
        {
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
                    Utils.ReceiveData(clientSocket, headerLength, ref buffer);
                    var header = new Header();
                    header.DecodeData(buffer);
                    switch (header.ICommand)
                    {
                        case CommandConstants.PublishGame:
                            var bufferData = new byte[header.IDataLength];  
                            Utils.ReceiveData(clientSocket, header.IDataLength, ref bufferData);
                            
                            string jsonGame = Encoding.UTF8.GetString(bufferData);
                            Game newGame = Game.Decode(jsonGame);
                            GameSystem.AddGame(newGame);

                            var message = "Se ha publicado el juego: " + newGame.Title + ".";
                            var responseHeader = new Header(HeaderConstants.Response, CommandConstants.PublishGameOk, message.Length);
                            var data = responseHeader.GetRequest();
                            Utils.SendData(clientSocket, data, message);
                            break;
                        case CommandConstants.GetGames:
                            var gameListMessage = GameSystem.EncodeGameList();
                            var gameListHeader = new Header(HeaderConstants.Response, CommandConstants.GetGamesOk, gameListMessage.Length);
                            var gameListData = gameListHeader.GetRequest();
                            Utils.SendData(clientSocket, gameListData, gameListMessage);
                            break;
                        // case 3:
                        //     Utils.SendData(clientSocket,  "Seleccionaste: Publicar juego");
                        //     break;
                        // case 4:
                        //     Utils.SendData(clientSocket,  "Seleccionaste: Publicar calificación de un juego");
                        //     break;
                        // case 5:
                        //     Utils.SendData(clientSocket,  "Seleccionaste: Buscar juegos");
                        //     break;
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine($"Error: {e.Message}..");    
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}..");    
                }
            }
        }
    }
}