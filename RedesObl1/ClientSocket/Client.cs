using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Domain;
using ProtocolLibrary;
using SocketUtils;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ClientSocket
{
    public class Client
    {
        public string ServerIpAddress  { get; set; }
        public int ServerPort { get; set; }
        public string ClientIpAddress  { get; set; }
        public int ClientPort  { get; set; }

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
                Display.ClientMenu();
                var option = Console.ReadLine();
                string gamesJson;
                List<Game> gameList;
                switch (option)
                {
                    case "exit":
                        clientSocket.Shutdown(SocketShutdown.Both); //apagamos el server
                        clientSocket.Close();
                        connected = false;
                        break;
                    case "1": //publicar
                        Game gameToPublish = Display.InputGame();

                        var message = gameToPublish.Encode();
                        var header = new Header(HeaderConstants.Request, CommandConstants.PublishGame, message.Length);
                        Utils.SendData(clientSocket, header, message);
                        
                        Console.WriteLine(Utils.ReciveMessageData(clientSocket));
                        break;
                    case "2": //modificar        
                        Utils.SendData(clientSocket, headerRequestGameList, "");

                        gamesJson = Utils.ReciveMessageData(clientSocket);
                        gameList = GameSystem.DecodeGames(gamesJson);

                        Game gameToModify = Display.SelectGame(gameList);
                        if(gameToModify == null){
                            Console.WriteLine("Retorno al menú.");
                            break;
                        }
                        
                        Console.WriteLine("Ingrese los nuevos datos del juego. Si no quiere modificar el campo, presione ENTER.");
                        Game modifiedGame = Display.InputGame();

                        var modifyGameMessage = GameSystem.EncodeGames(new List<Game>() {gameToModify, modifiedGame});
                        var modifyGameHeader = new Header(HeaderConstants.Request, CommandConstants.ModifyGame, modifyGameMessage.Length);
                        Utils.SendData(clientSocket, modifyGameHeader, modifyGameMessage);

                        Console.WriteLine(Utils.ReciveMessageData(clientSocket));
                        break;
                    case "3":
                        Utils.SendData(clientSocket, headerRequestGameList, "");

                        gamesJson = Utils.ReciveMessageData(clientSocket);
                        gameList = GameSystem.DecodeGames(gamesJson);

                        Game gameToDelete = Display.SelectGame(gameList);
                        if(gameToDelete == null){
                            Console.WriteLine("Retorno al menú.");
                            break;
                        }

                        var deleteGameMessage = gameToDelete.Encode();
                        var deleteGameHeader = new Header(HeaderConstants.Request, CommandConstants.DeleteGame, deleteGameMessage.Length);
                        Utils.SendData(clientSocket, deleteGameHeader, deleteGameMessage);

                        Console.WriteLine(Utils.ReciveMessageData(clientSocket));
                        break;
                    case "4":
                        Display.GameFilterOptions();
                        var filter = Console.ReadLine();
                        Utils.SendData(clientSocket, headerRequestGameList, "");

                        gamesJson = Utils.ReciveMessageData(clientSocket);
                        gameList = GameSystem.DecodeGames(gamesJson);
                        List<Game> filtedGames = new List<Game>();
                        
                        switch (filter)
                        {
                        case "1":
                            Console.WriteLine("Ingrese categoría.");
                            var cat = Console.ReadLine();
                            filtedGames = gameList.FindAll(g => g.Genre.Equals(cat));
                            Display.GameList(filtedGames);
                        break;
                        case "2": 
                        Console.WriteLine("Ingrese tituilo.");
                            var title = Console.ReadLine();
                            filtedGames = gameList.FindAll(g => g.Title.Equals(title));
                            Display.GameList(filtedGames);
                        break;
                        case "3": 
                        Console.WriteLine("Ingrese categoría.");
                            int rating;
                            Int32.TryParse(Console.ReadLine(), out rating);
                            filtedGames = gameList.FindAll(g => g.Rating.Equals(rating));
                            Display.GameList(filtedGames);
                        break;
                        default:
                        Console.WriteLine("Opción inválida.");
                        break;
                        }
                        break;
                    case "5": //calificar
                        Console.WriteLine("Funcionalidad no implementada.");
                        break;
                    case "6": //Ver juegos y su detalle.
                        Utils.SendData(clientSocket, headerRequestGameList, "");
                        gamesJson = Utils.ReciveMessageData(clientSocket);
                        gameList = GameSystem.DecodeGames(gamesJson);
                        Display.GameList(gameList);
                        Console.WriteLine();
                        Display.ShowGameDetail(gameList);
                        break;
                    default:
                        Console.WriteLine("Opción inválida.");
                        break;
                }
            }
        }
    }
}