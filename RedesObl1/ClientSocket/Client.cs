using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Domain;
using ProtocolLibrary;
using SocketUtils;
using Microsoft.Extensions.Configuration;
using System.IO;
using DisplayUtils;
using Common;

namespace ClientSocket
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

            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Parse(ClientConfig.ClientIpAddress), ClientConfig.ClientPort);
            clientSocket.Bind(clientEndPoint);
            Console.WriteLine("Conectando al servidor...");

            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(ClientConfig.ServerIpAddress), ClientConfig.ServerPort);
            clientSocket.Connect(serverEndPoint);
            var connected = true;
            Console.WriteLine("Conectado al servidor.");

            var headerRequestGameList = new Header(HeaderConstants.Request, CommandConstants.GetGames, 0);
            
            ClientUtils clientUtils = new ClientUtils(clientSocket);
            clientUtils.Login();
            while (connected)
            {
                var option = DialogUtils.Menu(ClientMenuOptions);

                switch (option)
                {
                    case "exit":
                        clientSocket.Shutdown(SocketShutdown.Both); //apagamos el server
                        clientSocket.Close();
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
                        clientUtils.AdquireGame();
                        break;
                    case "7": 
                        DialogUtils.ShowGameDetail(clientUtils.GetAdquiredGames());
                        break;
                    case "8": 
                        DialogUtils.ShowGameDetail(clientUtils.GetGames());
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