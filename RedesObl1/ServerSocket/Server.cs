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

        public string ServerIpAddress  { get; set; }
        public int ProtocolFixedSize { get; set; }
        public int ServerPort  { get; set; }
        public int Backlog  { get; set; }
        private static GameSystem GameSystem;
        private static bool _exit = false;
        static List<Socket> _clients = new List<Socket>();

        public static Dictionary<string, string> ServerMenuOptions = new Dictionary<string, string> {
            {"1", "Ver juegos y detalles"},
            {"2", "Adquirir juego"},
            {"3", "Publicar juego"},
            {"4", "Publicar calificación de un juego"},
            {"5", "Buscar juegos"}
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

            GameSystem = new GameSystem();
            GameSystem.AddGame(new Game {
                Title = "FIFA",
                Genre = "Sports",
                Synopsis = "football game",
                Rating = 10
            });
            GameSystem.AddGame(new Game {
                Title = "COD",
                Genre = "War",
                Synopsis = "war game",
                Rating = 9
            });

            _clients = new List<Socket>();
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverIpEndPoint = new IPEndPoint(IPAddress.Parse(ServerConfig.ServerIpAddress), ServerConfig.ServerPort);
            serverSocket.Bind(serverIpEndPoint);
            serverSocket.Listen(ServerConfig.Backlog);
            
            var threadServer = new Thread(()=> ListenForConnections(serverSocket));
            threadServer.Start();
            
            while (!_exit)
            {
                DialogUtils.Menu(ServerMenuOptions);
                var option = Console.ReadLine();
                if(ServerMenuOptions.ContainsKey(option)){
                    Console.WriteLine("Has seleccionado: " + ServerMenuOptions[option]);
                }
                switch (option)
                {
                    case "exit":
                        _exit = true;
                        serverSocket.Close(0);
                        foreach (var client in _clients)
                        {
                            client.Shutdown(SocketShutdown.Both);
                            client.Close();
                        }
                        var fakeSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
                        fakeSocket.Connect("127.0.0.1",20000);
                        break;
                    case "1":
                        DialogUtils.ShowGameDetail(GameSystem.Games);
                        break;
                    case "2": 
                        Console.WriteLine("Funcionalidad no implementada.");
                        break;
                    case "3": 
                        Game gameToPublish = DialogUtils.InputGame();

                        GameSystem.AddGame(gameToPublish);
                        Console.WriteLine("Se ha publicado el juego: " + gameToPublish.Title + ".");

                        break;
                    case "4": 
                        Console.WriteLine("Funcionalidad no implementada.");
                        break;
                    case "5": 
                        DialogUtils.SearchFilteredGames(GameSystem.Games);
                        break;
                    default:    
                        Console.WriteLine("Opción inválida.");
                        break;
                }
                Console.WriteLine("Ingrese cualquier valor para volver al menú.");
                Console.ReadLine();
            }
        }

        private static void ListenForConnections(Socket socketServer)
        {
            while (!_exit)
            {
                try
                {
                    var clientConnected = socketServer.Accept();
                    _clients.Add(clientConnected);
                    var threadClient = new Thread(() => HandleClient(clientConnected));
                    threadClient.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _exit = true;
                }
            }
            Console.WriteLine("Saliendo...");
        }

        private static void HandleClient(Socket clientSocket)
        {
            while (!_exit)
            {
                var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                   HeaderConstants.DataLength;
                var buffer = new byte[headerLength];
                try
                {
                    Utils.ReceiveData(clientSocket, headerLength, ref buffer);
                    var header = new Header();
                    header.DecodeData(buffer);
                    switch (header.ICommand)
                    {
                        case CommandConstants.PublishGame:
                            var publishGameBufferData = new byte[header.IDataLength];  
                            Utils.ReceiveData(clientSocket, header.IDataLength, ref publishGameBufferData);
                            string jsonPublishGame = Encoding.UTF8.GetString(publishGameBufferData);

                            PublishGameManager(clientSocket, jsonPublishGame);

                            break;
                        case CommandConstants.GetGames:
                            var gamesMessage = GameSystem.EncodeGames();
                            var gamesHeader = new Header(HeaderConstants.Response, CommandConstants.GetGamesOk, gamesMessage.Length);
                            Utils.SendData(clientSocket, gamesHeader, gamesMessage);

                            break;
                        case CommandConstants.ModifyGame:
                            var modifyGameBufferData = new byte[header.IDataLength];  
                            Utils.ReceiveData(clientSocket, header.IDataLength, ref modifyGameBufferData);
                            string jsonModifyGameData = Encoding.UTF8.GetString(modifyGameBufferData);
                            
                            ModifyGameManager(clientSocket, jsonModifyGameData);
                            
                            break;
                        case CommandConstants.DeleteGame:
                            var deleteGameBufferData = new byte[header.IDataLength];  
                            Utils.ReceiveData(clientSocket, header.IDataLength, ref deleteGameBufferData);
                            string jsonDeleteGameData = Encoding.UTF8.GetString(deleteGameBufferData);

                            DeleteGameManager(clientSocket, jsonDeleteGameData);
                            
                            break;
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine($"Socket error: {e.Message}..");    
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}..");    
                }
            }
        }

        private static void ModifyGameManager(Socket clientSocket, string jsonModifyGameData){
            try
            {
                List<Game> updatingGames = GameSystem.DecodeGames(jsonModifyGameData);
                var gameToModify = GameSystem.Games.Find(g => g.Title.Equals(updatingGames[0].Title));
                gameToModify.Update(updatingGames[1]);
                
                var modifyGameMessage = "Se ha modificado el juego: " + gameToModify.Title + ".";
                var modifyGameHeader = new Header(HeaderConstants.Response, CommandConstants.ModifyGameOk, modifyGameMessage.Length);
                Utils.SendData(clientSocket, modifyGameHeader, modifyGameMessage);
            }
            catch (Exception){
                var modifyGameMessage = "No se ha podido modificar el juego.";
                var modifyGameHeader = new Header(HeaderConstants.Response, CommandConstants.ModifyGameError, modifyGameMessage.Length);
                Utils.SendData(clientSocket, modifyGameHeader, modifyGameMessage);
            }
        }

        private static void DeleteGameManager(Socket clientSocket, string jsonDeleteGameData){
            try
            {
                Game gameToDelete = Game.Decode(jsonDeleteGameData);
                GameSystem.DeleteGame(gameToDelete);
                
                var deleteGameMessage = "Se ha eliminado el juego: " + gameToDelete.Title + ".";
                var deleteGameHeader = new Header(HeaderConstants.Response, CommandConstants.DeleteGameOk, deleteGameMessage.Length);
                Utils.SendData(clientSocket, deleteGameHeader, deleteGameMessage);
            }
            catch (Exception){
                var deleteGameMessage = "No se ha podido eliminar el juego.";
                var deleteGameHeader = new Header(HeaderConstants.Response, CommandConstants.DeleteGameError, deleteGameMessage.Length);
                Utils.SendData(clientSocket, deleteGameHeader, deleteGameMessage);
            }
        }

        private static void PublishGameManager(Socket clientSocket, string jsonPublishGame){
            Game newGame = Game.Decode(jsonPublishGame);
            GameSystem.AddGame(newGame);

            var publishedGameMessage = "Se ha publicado el juego: " + newGame.Title + ".";
            var publishedGameHeader = new Header(HeaderConstants.Response, CommandConstants.PublishGameOk, publishedGameMessage.Length);
            Utils.SendData(clientSocket, publishedGameHeader, publishedGameMessage);
        }
    }
}