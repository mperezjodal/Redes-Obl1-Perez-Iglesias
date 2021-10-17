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
            {"8", "Ver juegos y detalles"}
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

        public static void RunApp(TcpClient tcpClient)
        {
            var connected = true;

            using (var networkStream = tcpClient.GetStream())
            {
                var headerRequestGameList = new Header(HeaderConstants.Request, CommandConstants.GetGames, 0);
            
                ClientUtils clientUtils = new ClientUtils(networkStream);
                clientUtils.Login();
                while (connected)
                {
                    var option = DialogUtils.Menu(ClientMenuOptions);

                    switch (option)
                    {
                        case "exit":
                            clientUtils.Logout();
                            connected = false;
                            break;
                        case "1":
                            clientUtils.PublishGame();
                            break;
                        case "2": 
                            clientUtils.ModifyGame();
                            break;
                        case "3": 
                            clientUtils.DeleteGame();
                            break;
                        case "4": 
                            DialogUtils.SearchFilteredGames(clientUtils.GetGames());
                            break;
                        case "5":
                            clientUtils.PublishReview();
                            break;
                        case "6": 
                            clientUtils.AcquireGame();
                            break;
                        case "7": 
                            clientUtils.ShowGamesAndDetail(clientUtils.GetAcquiredGames());
                            break;
                        case "8": 
                            clientUtils.ShowGamesAndDetail(clientUtils.GetGames());
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