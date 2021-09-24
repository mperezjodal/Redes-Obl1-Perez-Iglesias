using System.Security.AccessControl;
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
using DisplayUtils;
using System.IO;
using Microsoft.Extensions.Configuration;
using Common.Protocol;
using Common;

namespace ServerSocket
{
    public class Server
    {
        public string ServerIpAddress { get; set; }
        public int ProtocolFixedSize { get; set; }
        public int ServerPort { get; set; }
        public int Backlog { get; set; }
        private static GameSystem GameSystem;
        private static bool _exit = false;
        static List<Socket> _clients = new List<Socket>();
        public static Dictionary<string, string> ServerMenuOptions = new Dictionary<string, string> {
            {"1", "Ver juegos y detalles"},
            {"2", "Publicar juego"},
            {"3", "Publicar calificación de un juego"},
            {"4", "Buscar juegos"}
        };
        static void Main(string[] args)
        {
            string directory = Directory.GetCurrentDirectory();
            IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(directory)
                    .AddJsonFile("appsettings.json")
                    .Build();

            var section = configuration.GetSection(nameof(Server));
            var ServerConfig = section.Get<Server>();

            GameSystem = new GameSystem();
            // GameSystem.AddGame(new Game {
            //     Title = "FIFA",
            //     Genre = "Sports",
            //     Synopsis = "football game",
            //     Rating = 1,
            //     Reviews = new List<Review>()
            // });
            // GameSystem.AddGame(new Game {
            //     Title = "COD",
            //     Genre = "War",
            //     Synopsis = "war game",
            //     Rating = 1,
            //     Reviews = new List<Review>()
            // });

            _clients = new List<Socket>();
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverIpEndPoint = new IPEndPoint(IPAddress.Parse(ServerConfig.ServerIpAddress), ServerConfig.ServerPort);
            serverSocket.Bind(serverIpEndPoint);
            serverSocket.Listen(ServerConfig.Backlog);

            var threadServer = new Thread(() => ListenForConnections(serverSocket));
            threadServer.Start();

            while (!_exit)
            {
                var option = DialogUtils.Menu(ServerMenuOptions);

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
                        var fakeSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        fakeSocket.Connect("127.0.0.1", 20000);
                        break;

                    case "1":
                        DialogUtils.ShowGameDetail(GameSystem.Games);
                        break;
                    case "2":
                        Game gameToPublish = DialogUtils.InputGame();
                        GameSystem.AddGame(gameToPublish);
                        Console.WriteLine("Se ha publicado el juego: " + gameToPublish.Title + ".");
                        break;
                    case "3":
                        Game selectedGame = DialogUtils.SelectGame(GameSystem.Games);
                        Review selectedGameReview = DialogUtils.InputReview();
                        selectedGame.AddReview(selectedGameReview);
                        Console.WriteLine("Se ha publicado la calificación del juego " + selectedGame.Title + ".");
                        break;
                    case "4":
                        DialogUtils.SearchFilteredGames(GameSystem.Games);
                        break;
                    default:
                        Console.WriteLine("Opción inválida.");
                        break;
                }
                DialogUtils.ReturnToMenu();
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
                    var threadClient = new Thread(() => HandleClient(clientConnected, socketServer));
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

        private static void HandleClient(Socket clientSocket, Socket serverSocket)
        {
            ServerUtils serverUtils = new ServerUtils(GameSystem, clientSocket);
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
                    var bufferData = new byte[header.IDataLength];
                    Utils.ReceiveData(clientSocket, header.IDataLength, ref bufferData);
                    string jsonData = Encoding.UTF8.GetString(bufferData);
                    
                    switch (header.ICommand)
                    {
                        case CommandConstants.Login:
                            serverUtils.LoginManager(jsonData);
                            break;
                        case CommandConstants.PublishGame:
                            serverUtils.PublishGameManager(jsonData, serverSocket);
                            break;
                        case CommandConstants.GetGames:
                            serverUtils.GetGamesManager(clientSocket);
                            break;
                        case CommandConstants.GetUsers:
                            var usersMessage = GameSystem.EncodeUsers();
                            var usersHeader = new Header(HeaderConstants.Response, CommandConstants.GetUsersOk, usersMessage.Length);
                            Utils.SendData(clientSocket, usersHeader, usersMessage);

                            break;
                        case CommandConstants.PublishReview:
                            serverUtils.PublishReviewManager(jsonData);
                            break;
                        case CommandConstants.ModifyGame:
                            serverUtils.ModifyGameManager(jsonData);
                            break;
                        case CommandConstants.DeleteGame:
                            serverUtils.DeleteGameManager(jsonData);
                            break;
                        case CommandConstants.AcquireGame:
                            serverUtils.AcquireGameManager(jsonData);
                            break;
                        case CommandConstants.GetAcquiredGames:
                            serverUtils.GetAcquiredGamesManager(jsonData);
                            break;
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine($"Socket error: {e.Message}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }
            }
        }
    }
}