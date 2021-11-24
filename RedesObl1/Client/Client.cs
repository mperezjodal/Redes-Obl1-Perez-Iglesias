using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using ProtocolLibrary;
using Microsoft.Extensions.Configuration;
using System.IO;
using DisplayUtils;
using networkStream;
using Domain;
using System.Threading.Tasks;

namespace Client
{
    public class Client
    {
        public string ServerIpAddress  { get; set; }
        public int ServerPort { get; set; }
        public string ClientIpAddress  { get; set; }
        public int ClientPort  { get; set; }

        public static Dictionary<string, string> ClientMenuOptions = new Dictionary<string, string> {
            {"1", "Publicar juego"},
            {"2", "Modificar juego"},
            {"3", "Eliminar juego"},
            {"4", "Buscar juego"},
            {"5", "Calificar juego"},
            {"6", "Adquirir juego"},
            {"7", "Ver juegos adquiridos"},
            {"8", "Ver juegos y detalles"},
            {" ", "logout"}
        };

        static async Task Main(string[] args)
        {
            string directory = Directory.GetCurrentDirectory();
            IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(directory)
                    .AddJsonFile("appsettings.json")
                    .Build();

            var section = configuration.GetSection(nameof(Client));
		    var ClientConfig = section.Get<Client>();

            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Parse(ClientConfig.ClientIpAddress), ClientConfig.ClientPort);
            var tcpClient = new TcpClient(clientEndPoint);            
            Console.WriteLine("Conectando al servidor...");

            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(ClientConfig.ServerIpAddress), ClientConfig.ServerPort);
            tcpClient.Connect(serverEndPoint);
            Console.WriteLine("Conectado al servidor.");

            await RunAppAsync(tcpClient);
            tcpClient.Close();
        }

        public static async Task RunAppAsync(TcpClient tcpClient)
        {
            var connected = true;

            using (var networkStream = tcpClient.GetStream())
            {
                var headerRequestGameList = new Header(HeaderConstants.Request, CommandConstants.GetGames, 0);
            
                ClientUtils clientUtils = new ClientUtils(networkStream);
                await clientUtils.LoginAsync();
                while (connected)
                {
                    var option = DialogUtils.Menu(ClientMenuOptions);

                    switch (option)
                    {
                        case "logout":
                            await clientUtils.LogoutAsync();
                            connected = false;
                            break;
                        case "1":
                            await clientUtils.PublishGameAsync();
                            break;
                        case "2": 
                            await clientUtils.ModifyGameAsync();
                            break;
                        case "3": 
                            await clientUtils.DeleteGameAsync();
                            break;
                        case "4": 
                            DialogUtils.SearchFilteredGames(await clientUtils.GetGamesAsync());
                            break;
                        case "5":
                            await clientUtils.PublishReviewAsync();
                            break;
                        case "6": 
                            await clientUtils.AcquireGameAsync();
                            break;
                        case "7": 
                            await clientUtils.ShowGamesAndDetailAsync(await clientUtils.GetAcquiredGamesAsync());
                            break;
                        case "8": 
                            await clientUtils.ShowGamesAndDetailAsync(await clientUtils.GetGamesAsync());
                            break;
                        default:
                            Console.WriteLine("Opción inválida.");
                            break;
                    }
                    DialogUtils.ReturnToMenu();
                }
            }
        }
    }
}