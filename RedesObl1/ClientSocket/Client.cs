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
            {"5", "Calificar juegos"},
            {"6", "Ver juegos y detalles"}
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
            
            while (connected)
            {
                DialogUtils.Menu(ClientMenuOptions);
                var option = Console.ReadLine();
                if(ClientMenuOptions.ContainsKey(option)){
                    Console.WriteLine("Has seleccionado: " + ClientMenuOptions[option]);
                }
                switch (option)
                {
                    case "exit":
                        clientSocket.Shutdown(SocketShutdown.Both); //apagamos el server
                        clientSocket.Close();
                        connected = false;
                        break;
                    case "1":
                        PublishGame(clientSocket);
                        break;
                    case "2": 
                        ModifyGame(clientSocket);
                        break;
                    case "3": 
                        DeleteGame(clientSocket);
                        break;
                    case "4": 
                        DialogUtils.SearchFilteredGames(GetGames(clientSocket));
                        break;
                    case "5": 
                        Game selectedGame = DialogUtils.SelectGame(GetGames(clientSocket));
                        PublishReview(clientSocket, selectedGame);
                        Console.WriteLine("Se ha publicado la review al juego " + selectedGame.Title + ".");
                        break;
                    case "6": 
                        DialogUtils.ShowGameDetail(GetGames(clientSocket));
                        break;
                    default:
                        Console.WriteLine("Opción inválida.");
                        break;
                }
                Console.WriteLine("Ingrese cualquier valor para volver al menú.");
                Console.ReadLine();
            }
        }

        private static List<Game> GetGames(Socket clientSocket){
            var headerRequestGameList = new Header(HeaderConstants.Request, CommandConstants.GetGames, 0);
            Utils.SendData(clientSocket, headerRequestGameList, "");

            var gamesJson = Utils.ReciveMessageData(clientSocket);
            return GameSystem.DecodeGames(gamesJson);
        }

        private static void PublishGame(Socket clientSocket){
            Game gameToPublish = DialogUtils.InputGame();

            var message = gameToPublish.Encode();
            var header = new Header(HeaderConstants.Request, CommandConstants.PublishGame, message.Length);
            Utils.SendData(clientSocket, header, message);
            
            Console.WriteLine(Utils.ReciveMessageData(clientSocket));
        }

        private static void PublishReview(Socket clientSocket, Game game){
            Review review = DialogUtils.InputReview();
            game.AddReview(review);

            var message = game.Encode();
            var header = new Header(HeaderConstants.Request, CommandConstants.PublishReview, message.Length);
            Utils.SendData(clientSocket, header, message);
            
            Console.WriteLine(Utils.ReciveMessageData(clientSocket));
        }

        private static void ModifyGame(Socket clientSocket) {
            List<Game> games = GetGames(clientSocket);
            
            Game gameToModify = DialogUtils.SelectGame(games);
            if(gameToModify == null){
                Console.WriteLine("Retorno al menú.");
                return;
            }
            
            Console.WriteLine("Ingrese los nuevos datos del juego. Si no quiere modificar el campo, presione ENTER.");
            Game modifiedGame = DialogUtils.InputGame();

            var modifyGameMessage = GameSystem.EncodeGames(new List<Game>() {gameToModify, modifiedGame});
            var modifyGameHeader = new Header(HeaderConstants.Request, CommandConstants.ModifyGame, modifyGameMessage.Length);
            Utils.SendData(clientSocket, modifyGameHeader, modifyGameMessage);

            Console.WriteLine(Utils.ReciveMessageData(clientSocket));
        }

        private static void DeleteGame(Socket clientSocket) {
            List<Game> games = GetGames(clientSocket);

            Game gameToDelete = DialogUtils.SelectGame(games);
            if(gameToDelete == null){
                Console.WriteLine("Retorno al menú.");
                return ;
            }

            var deleteGameMessage = gameToDelete.Encode();
            var deleteGameHeader = new Header(HeaderConstants.Request, CommandConstants.DeleteGame, deleteGameMessage.Length);
            Utils.SendData(clientSocket, deleteGameHeader, deleteGameMessage);

            Console.WriteLine(Utils.ReciveMessageData(clientSocket));
        }
    }
}