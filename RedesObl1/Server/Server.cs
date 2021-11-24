using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ConnectionUtils;
using ProtocolLibrary;
using DisplayUtils;
using System.IO;
using Microsoft.Extensions.Configuration;
using GRPCLibrary;
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
            using var channel = GrpcChannel.ForAddress(ServerConfig.GrpcChannelAddress);

            grpcClient = new GameSystemModel.GameSystemModelClient(channel);

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
                        await menu.ShowGamesAsync();
                        break;
                    case "2":
                        await menu.InsertGameAsync();
                        break;
                    case "3":
                        await menu.InsertReviewAsync();
                        break;
                    case "4":
                        DialogUtils.SearchFilteredGames(await menu.GetGamesAsync());
                        break;
                    case "5":
                        await menu.InsertUserAsync();
                        break;
                    case "6":
                        await menu.ModifyUserAsync();
                        break;
                    case "7":
                        await menu.DeleteUserAsync();
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
                    Task listenForConnectionsTask = Task.Run(async () => await HandleClientAsync(acceptedTcpClient));

                    _clients.Add(acceptedTcpClient);
                }
                catch (Exception)
                {
                    _exit = true;
                }
            }
            Console.WriteLine("Saliendo...");
        }

        public static async Task HandleClientAsync(TcpClient tcpClient)
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
                    buffer = await Utils.ServerReceiveDataAsync(networkStream, headerLength, buffer);
                    var header = new Header();
                    header.DecodeData(buffer);
                    var bufferData = new byte[header.IDataLength];
                    bufferData = await Utils.ServerReceiveDataAsync(networkStream, header.IDataLength, bufferData);

                    string jsonData = Encoding.UTF8.GetString(bufferData);

                    switch (header.ICommand)
                    {
                        case CommandConstants.Login:
                            await serverUtils.LoginHandlerAsync(jsonData);
                            break;
                        case CommandConstants.Logout:
                            await serverUtils.LogoutAsync(jsonData);
                            break;
                        case CommandConstants.PublishGame:
                            await serverUtils.PublishGameHandlerAsync(jsonData);
                            break;
                        case CommandConstants.GetGames:
                            await serverUtils.GetGamesHandlerAsync();
                            break;
                        case CommandConstants.GetUsers:
                            await serverUtils.GetUsersHandlerAsync();
                            break;
                        case CommandConstants.PublishReview:
                            await serverUtils.PublishReviewHandlerAsync(jsonData);
                            break;
                        case CommandConstants.ModifyingGame:
                            await serverUtils.BeingModifiedHandlerAsync(jsonData);
                            break;
                        case CommandConstants.ModifyGame:
                            await serverUtils.ModifyGameHandlerAsync(jsonData);
                            break;
                        case CommandConstants.DeleteGame:
                            await serverUtils.DeleteGameHandlerAsync(jsonData);
                            break;
                        case CommandConstants.AcquireGame:
                            await serverUtils.AcquireGameHandlerAsync(jsonData);
                            break;
                        case CommandConstants.GetAcquiredGames:
                            await serverUtils.GetAcquiredGamesHandlerAsync();
                            break;
                        case CommandConstants.GetGameCover:
                            await serverUtils.GetGameCoverAsync(jsonData);
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