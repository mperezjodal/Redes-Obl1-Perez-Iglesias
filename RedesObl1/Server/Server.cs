using System.Threading.Tasks;
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
using ConnectionUtils;
using Domain;
using ProtocolLibrary;
using DisplayUtils;
using System.IO;
using Microsoft.Extensions.Configuration;
namespace Server
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
            {"4", "Buscar juegos"},
            {"5", "Insertar usuario"},
            {"6", "Modificar usuario"},
            {"7", "Eliminar usuario"},
            {" ", "exit"}
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

            Task listenForConnectionsTask = Task.Run(() => ListenForConnections(tcpListener));
            ServerMenuUtils menu = new ServerMenuUtils(GameSystem);

            while (!_exit)
            {
                var option = DialogUtils.Menu(ServerMenuOptions);

                switch (option)
                {
                    case "exit":
                        _exit = true;
                        break;
                    case "1":
                        DialogUtils.ShowGameDetail(GameSystem.Games);
                        break;
                    case "2":
                        menu.InsertGame();
                        break;
                    case "3":
                        menu.InsertReview();
                        break;
                    case "4":
                        DialogUtils.SearchFilteredGames(GameSystem.Games);
                        break;
                    case "5":
                        menu.InsertUser();
                        break;
                    case "6":
                        menu.ModifyUser();
                        break;
                    case "7":
                        menu.DeleteUser();
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
                    Task listenForConnectionsTask = Task.Run(() => HandleClient(acceptedTcpClient));

                    _clients.Add(acceptedTcpClient);
                }
                catch (Exception)
                {
                    _exit = true;
                }
            }
            Console.WriteLine("Saliendo...");
        }

        public static async void HandleClient(TcpClient tcpClient)
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
                    buffer = await Utils.ServerReceiveData(networkStream, headerLength, buffer);
                    var header = new Header();
                    header.DecodeData(buffer);
                    var bufferData = new byte[header.IDataLength];
                    bufferData = await Utils.ServerReceiveData(networkStream, header.IDataLength, bufferData);
                    
                    string jsonData = Encoding.UTF8.GetString(bufferData);

                    switch (header.ICommand)
                    {
                        case CommandConstants.Login:
                            await serverUtils.LoginHandler(jsonData);
                            break;
                        case CommandConstants.Logout:
                            serverUtils.Logout(jsonData);
                            break;
                        case CommandConstants.PublishGame:
                            await serverUtils.PublishGameHandler(jsonData);
                            break;
                        case CommandConstants.GetGames:
                            await serverUtils.GetGamesHandler();
                            break;
                        case CommandConstants.GetUsers:
                            await serverUtils.GetUsersHandler();
                            break;
                        case CommandConstants.PublishReview:
                            await serverUtils.PublishReviewHandler(jsonData);
                            break;
                        case CommandConstants.ModifyingGame:
                            gamesBeingModifiedByClient = await serverUtils.BeingModifiedHandler(jsonData, gamesBeingModifiedByClient);
                            break;
                        case CommandConstants.ModifyGame:
                            gamesBeingModifiedByClient = await serverUtils.ModifyGameHandler(jsonData, gamesBeingModifiedByClient);
                            break;
                        case CommandConstants.DeleteGame:
                            await serverUtils.DeleteGameHandler(jsonData);
                            break;
                        case CommandConstants.AcquireGame:
                            await serverUtils.AcquireGameHandler(jsonData);
                            break;
                        case CommandConstants.GetAcquiredGames:
                            await serverUtils.GetAcquiredGamesHandler(jsonData);
                            break;
                        case CommandConstants.GetGameCover:
                            await serverUtils.GetGameCover(jsonData);
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