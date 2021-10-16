using System.Linq;
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
namespace ServerSocket
{
    public class Server
    {
        public string ServerIpAddress { get; set; }
        public int ProtocolFixedSize { get; set; }
        public int ServerPort { get; set; }
        public int FakeServerPort { get; set; }
        public int Backlog { get; set; }
        private static GameSystem GameSystem;
        private static bool _exit = false;
        static List<TcpClient> _clients = new List<TcpClient>();

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

            object lockObject = new object();
            GameSystem = new GameSystem();

            IPEndPoint serverIpEndPoint = new IPEndPoint(IPAddress.Parse(ServerConfig.ServerIpAddress), ServerConfig.ServerPort);
            
            var tcpListener = new TcpListener(serverIpEndPoint);
            tcpListener.Start(100);

            var threadServer = new Thread(() => ListenForConnections(tcpListener));
            threadServer.Start();

            while (!_exit)
            {
                var option = DialogUtils.Menu(ServerMenuOptions);

                switch (option)
                {
                    case "exit":
                        _exit = true;
                        foreach (var client in _clients)
                        {
                            client.Close();
                        }
                        break;
                    case "1":
                        DialogUtils.ShowGameDetail(GameSystem.Games);
                        break;
                    case "2":
                        Game gameToPublish = DialogUtils.InputGame();

                        try
                        {
                            if (gameToPublish.Cover != null)
                            {
                                var fileName = gameToPublish.Cover.Split("/").Last();
                                System.IO.File.Copy(gameToPublish.Cover, Directory.GetCurrentDirectory().ToString() + "/" + fileName);
                                gameToPublish.Cover = fileName;
                            }
                        }
                        catch (Exception) { }

                        GameSystem.AddGame(gameToPublish);
                        Console.WriteLine("Se ha publicado el juego: " + gameToPublish.Title + ".");
                        break;
                    case "3":
                        Game selectedGame = DialogUtils.SelectGame(GameSystem.Games);
                        if (selectedGame == null)
                        {
                            break;
                        }
                        if (GameSystem.IsGameBeingModified(selectedGame))
                        {
                            Console.WriteLine("No se puede publicar una califiación de este juego.");
                            break;
                        }

                        Review selectedGameReview = DialogUtils.InputReview();

                        if (selectedGameReview == null)
                        {
                            break;
                        }
                        if (GameSystem.IsGameBeingModified(selectedGame) || !GameSystem.GameExists(selectedGame))
                        {
                            Console.WriteLine("No se puede publicar una califiación de este juego.");
                            break;
                        }

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
        public static void ListenForConnections(TcpListener tcpListener)
        {
            while (!_exit)
            {
                try
                {
                    var acceptedTcpClient = tcpListener.AcceptTcpClient();
                    new Thread(() => HandleClient(acceptedTcpClient)).Start();

                    _clients.Add(acceptedTcpClient);
                }
                catch (Exception)
                {
                    _exit = true;
                }
            }
            Console.WriteLine("Saliendo...");
        }

        public static void HandleClient(TcpClient tcpClient)
        {
            List<Game> gamesBeingModifiedByClient = new List<Game>();
            var networkStream = tcpClient.GetStream();

            ServerUtils serverUtils = new ServerUtils(GameSystem, tcpClient);
            while (!_exit)
            {
                var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                   HeaderConstants.DataLength;
                var buffer = new byte[headerLength];
                try
                {
                    Utils.ReceiveData(networkStream, headerLength, ref buffer);
                    var header = new Header();
                    header.DecodeData(buffer);
                    var bufferData = new byte[header.IDataLength];
                    Utils.ReceiveData(networkStream, header.IDataLength, ref bufferData);
                    string jsonData = Encoding.UTF8.GetString(bufferData);

                    switch (header.ICommand)
                    {
                        case CommandConstants.Login:
                            serverUtils.LoginHandler(jsonData);
                            break;
                        case CommandConstants.Logout:
                            serverUtils.Logout(jsonData);
                            break;
                        case CommandConstants.PublishGame:
                            serverUtils.PublishGameHandler(jsonData);
                            break;
                        case CommandConstants.GetGames:
                            serverUtils.GetGamesHandler();
                            break;
                        case CommandConstants.GetUsers:
                            var usersMessage = GameSystem.EncodeUsers();
                            var usersHeader = new Header(HeaderConstants.Response, CommandConstants.GetUsersOk, usersMessage.Length);
                            Utils.SendData(networkStream, usersHeader, usersMessage);
                            break;
                        case CommandConstants.PublishReview:
                            serverUtils.PublishReviewHandler(jsonData);
                            break;
                        case CommandConstants.ModifyingGame:
                            serverUtils.BeingModifiedHandler(jsonData, ref gamesBeingModifiedByClient);
                            break;
                        case CommandConstants.ModifyGame:
                            serverUtils.ModifyGameHandler(jsonData, ref gamesBeingModifiedByClient);
                            break;
                        case CommandConstants.DeleteGame:
                            serverUtils.DeleteGameHandler(jsonData);
                            break;
                        case CommandConstants.AcquireGame:
                            serverUtils.AcquireGameHandler(jsonData);
                            break;
                        case CommandConstants.GetAcquiredGames:
                            serverUtils.GetAcquiredGamesHandler(jsonData);
                            break;
                        case CommandConstants.GetGameCover:
                            serverUtils.GetGameCover(jsonData);
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