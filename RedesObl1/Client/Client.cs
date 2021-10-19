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

        static void Main(string[] args)
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

            RunApp(tcpClient);
            tcpClient.Close();
        }

        public static async void RunApp(TcpClient tcpClient)
        {
            var connected = true;

            using (var networkStream = tcpClient.GetStream())
            {
                var headerRequestGameList = new Header(HeaderConstants.Request, CommandConstants.GetGames, 0);
            
                ClientUtils clientUtils = new ClientUtils(networkStream);
                await clientUtils.Login();
                while (connected)
                {
                    var option = DialogUtils.Menu(ClientMenuOptions);

                    switch (option)
                    {
                        case "logout":
                            await clientUtils.Logout();
                            connected = false;
                            break;
                        case "1":
                            await clientUtils.PublishGame();
                            break;
                        case "2": 
                            await clientUtils.ModifyGame();
                            break;
                        case "3": 
                            await clientUtils.DeleteGame();
                            break;
                        case "4": 
                            DialogUtils.SearchFilteredGames(await clientUtils.GetGames());
                            break;
                        case "5":
                            await clientUtils.PublishReview();
                            break;
                        case "6": 
                            await clientUtils.AcquireGame();
                            break;
                        case "7": 
                            await clientUtils.ShowGamesAndDetail(await clientUtils.GetAcquiredGames());
                            break;
                        case "8": 
                            await clientUtils.ShowGamesAndDetail(await clientUtils.GetGames());
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