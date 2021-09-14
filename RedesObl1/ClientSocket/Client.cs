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

namespace ClientSocket
{
    public class Client
    {
        private const string ServerIpAddress = "127.0.0.1";
        private const int ServerPort = 6000;
        private const string ClientIpAddress = "127.0.0.1";
        private const int ClientPort = 0;
        
        static void Main(string[] args)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Parse(ClientIpAddress), ClientPort);
            clientSocket.Bind(clientEndPoint);
            Console.WriteLine("Conectando al servidor...");
            
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(ServerIpAddress), ServerPort);
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
                    case "4": //buscar
                        Console.WriteLine("Funcionalidad no implementada.");
                        break;
                    case "5": //calificar
                        Console.WriteLine("Funcionalidad no implementada.");
                        break;
                    default:
                        Console.WriteLine("Opción inválida/");
                        break;
                }
            }
        }
    }
}