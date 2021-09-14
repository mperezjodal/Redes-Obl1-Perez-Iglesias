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

namespace ServerSocket
{
    public class Server
    {
        private const string ServerIpAddress = "127.0.0.1";
        private const int ProtocolFixedSize = 4;
        private const int ServerPort = 6000;
        private const int Backlog = 100;
        private static GameSystem GameSystem;
        private static bool _exit = false;
        static List<Socket> _clients = new List<Socket>();

        static void Main(string[] args)
        {
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
            IPEndPoint serverIpEndPoint = new IPEndPoint(IPAddress.Parse(ServerIpAddress), ServerPort);
            serverSocket.Bind(serverIpEndPoint);
            serverSocket.Listen(Backlog);
            
            var threadServer = new Thread(()=> ListenForConnections(serverSocket));
            threadServer.Start();
            
            while (!_exit)
            {
                Display.ServerMenu();
                var option = Console.ReadLine();
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
                    case "1": // ver juegos
                        Display.GameList(GameSystem.Games);
                        break;
                    case "2": // adquirir
                        break;
                    case "3": // publicar juego
                        Game gameToPublish = Display.InputGame();
                        GameSystem.AddGame(gameToPublish);
                        Console.WriteLine("Se ha publicado el juego: " + gameToPublish.Title + ".");
                        break;
                    case "4": // publicar califiacion
                        break;
                    case "5": // buscar juegos
                        break;
                    default:
                        Console.WriteLine("option invalida");
                        break;
                }
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

                            Game newGame = Game.Decode(jsonPublishGame);
                            GameSystem.AddGame(newGame);

                            var publishedGameMessage = "Se ha publicado el juego: " + newGame.Title + ".";
                            var publishedGameHeader = new Header(HeaderConstants.Response, CommandConstants.PublishGameOk, publishedGameMessage.Length);
                            Utils.SendData(clientSocket, publishedGameHeader, publishedGameMessage);

                            break;
                        case CommandConstants.GetGames:
                            var gamesMessage = GameSystem.EncodeGames();
                            var gamesHeader = new Header(HeaderConstants.Response, CommandConstants.GetGamesOk, gamesMessage.Length);
                            Utils.SendData(clientSocket, gamesHeader, gamesMessage);

                            break;
                        case CommandConstants.ModifyGame:
                            try
                            {
                                var modifyGameBufferData = new byte[header.IDataLength];  
                                Utils.ReceiveData(clientSocket, header.IDataLength, ref modifyGameBufferData);
                                string jsonModifyGameData = Encoding.UTF8.GetString(modifyGameBufferData);

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
                            break;
                        case CommandConstants.DeleteGame:
                            try
                            {
                                var deleteGameBufferData = new byte[header.IDataLength];  
                                Utils.ReceiveData(clientSocket, header.IDataLength, ref deleteGameBufferData);
                                string jsonDeleteGameData = Encoding.UTF8.GetString(deleteGameBufferData);

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
                            break;
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine($"Error: {e.Message}..");    
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}..");    
                }
            }
        }
    }
}