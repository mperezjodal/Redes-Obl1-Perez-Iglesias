using System.Text.RegularExpressions;
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
using GRPCLibrary;
using Grpc.Core;
using Grpc.Net.Client;

namespace Server
{
    public class Server
    {
        public string ServerIpAddress { get; set; }
        public int ProtocolFixedSize { get; set; }
        public int ServerPort { get; set; }
        public int FakeServerPort { get; set; }
        public int Backlog { get; set; }
        public string GrpcChannelAddress { get; set; }
        private static bool _exit = false;
        static List<TcpClient> _clients = new List<TcpClient>();
        private static GameSystemModel.GameSystemModelClient grpcClient;
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

        static async Task Main(string[] args)
        {
            string directory = Directory.GetCurrentDirectory();
            IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(directory)
                    .AddJsonFile("appsettings.json")
                    .Build();

            var section = configuration.GetSection(nameof(Server));
            var ServerConfig = section.Get<Server>();

            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            // The port number(5001) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress(ServerConfig.GrpcChannelAddress);

            grpcClient = new GameSystemModel.GameSystemModelClient(channel);

            object lockObject = new object();

            IPEndPoint serverIpEndPoint = new IPEndPoint(IPAddress.Parse(ServerConfig.ServerIpAddress), ServerConfig.ServerPort);

            var tcpListener = new TcpListener(serverIpEndPoint);
            tcpListener.Start(100);

            Task listenForConnectionsTask = Task.Run(() => ListenForConnections(tcpListener));
            ServerMenuUtils menu = new ServerMenuUtils(grpcClient);

            while (!_exit)
            {
                var option = DialogUtils.Menu(ServerMenuOptions);

                switch (option)
                {
                    case "exit":
                        _exit = true;
                        break;
                    case "1":
                        await menu.ShowGames();
                        break;
                    case "2":
                        await menu.InsertGame();
                        break;
                    case "3":
                        await menu.InsertReview();
                        break;
                    case "4":
                        DialogUtils.SearchFilteredGames(await menu.GetGames());
                        break;
                    case "5":
                        await menu.InsertUser();
                        break;
                    case "6":
                        await menu.ModifyUser();
                        break;
                    case "7":
                        await menu.DeleteUser();
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
                    Task listenForConnectionsTask = Task.Run(async () => await HandleClient(acceptedTcpClient));

                    _clients.Add(acceptedTcpClient);
                }
                catch (Exception)
                {
                    _exit = true;
                }
            }
            Console.WriteLine("Saliendo...");
        }

        public static async Task HandleClient(TcpClient tcpClient)
        {
            var networkStream = tcpClient.GetStream();

            ServerUtils serverUtils = new ServerUtils(tcpClient, grpcClient);
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
                            await serverUtils.Logout(jsonData);
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
                            await serverUtils.BeingModifiedHandler(jsonData);
                            break;
                        case CommandConstants.ModifyGame:
                            await serverUtils.ModifyGameHandler(jsonData);
                            break;
                        case CommandConstants.DeleteGame:
                            await serverUtils.DeleteGameHandler(jsonData);
                            break;
                        case CommandConstants.AcquireGame:
                            await serverUtils.AcquireGameHandler(jsonData);
                            break;
                        case CommandConstants.GetAcquiredGames:
                            await serverUtils.GetAcquiredGamesHandler();
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