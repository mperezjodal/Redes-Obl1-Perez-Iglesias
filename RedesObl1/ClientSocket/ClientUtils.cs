using System.Linq;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using DisplayUtils;
using Domain;
using ProtocolLibrary;
using SocketUtils;
using System.IO;
using FileStreamLibrary;
using System.Text;

namespace ClientSocket
{
    public class ClientUtils
    {
        private Socket clientSocket;
        private User myUser;
        public ClientUtils(Socket clientSocket)
        {
            this.clientSocket = clientSocket;
        }

        public void Login()
        {
            var notLogin = true;

            while (notLogin)
            {
                try{
                    string userName = DialogUtils.Login();
                    var headerLoginRequest = new Header(HeaderConstants.Request, CommandConstants.Login, userName.Length);
                    Utils.SendData(clientSocket, headerLoginRequest, userName);

                    List<string> commandAndMessage = Utils.ReceiveCommandAndMessage(clientSocket);

                    Console.WriteLine(commandAndMessage[1]);

                    if (commandAndMessage[0] == CommandConstants.LoginOk.ToString())
                    {
                        notLogin = false;
                    }
                }
                catch (SocketException)
                {
                    notLogin = false;
                    
                }
                catch (Exception)
                {
                    notLogin = false;
                }
            }

            var userJson = Utils.ReceiveMessageData(clientSocket);
            myUser = User.Decode(userJson);
        }

        public void AcquireGame()
        {
            Game game = DialogUtils.SelectGame(GetGames());
            if (game == null)
            {
                Console.WriteLine("Retorno al menú.");
                return;
            }

            var message = new UserGamePair(myUser, game).Encode();
            var header = new Header(HeaderConstants.Request, CommandConstants.AcquireGame, message.Length);
            Utils.SendData(clientSocket, header, message);

            Console.WriteLine(Utils.ReceiveMessageData(clientSocket));
        }

        public List<Game> GetGames()
        {
            var headerRequestGameList = new Header(HeaderConstants.Request, CommandConstants.GetGames, 0);
            Utils.SendData(clientSocket, headerRequestGameList, "");
            var gamesJson = Utils.ReceiveMessageData(clientSocket);
            List<Game> gameList = GameSystem.DecodeGames(gamesJson);

            foreach (Game g in gameList)
            {
                if (g.Cover != null && g.Cover != "")
                {
                    var fileCommunicationGameList = new FileCommunicationHandler(clientSocket);
                    fileCommunicationGameList.ReceiveFile();
                }
            }

            return gameList;
        }

        public List<User> GetUsers()
        {
            var headerRequestUsersList = new Header(HeaderConstants.Request, CommandConstants.GetUsers, 0);
            Utils.SendData(clientSocket, headerRequestUsersList, "");

            var usersJson = Utils.ReceiveMessageData(clientSocket);
            List<User> users = GameSystem.DecodeUsers(usersJson);

            return users;
        }

        public List<Game> GetAcquiredGames()
        {
            var message = myUser.Encode();
            var headerRequestGameList = new Header(HeaderConstants.Request, CommandConstants.GetAcquiredGames, message.Length);
            Utils.SendData(clientSocket, headerRequestGameList, message);

            var gamesJson = Utils.ReceiveMessageData(clientSocket);

            return GameSystem.DecodeGames(gamesJson);
        }

        public void SendFile(string path, Socket socket)
        {
            var fileCommunication = new FileCommunicationHandler(socket);
            fileCommunication.SendFile(path);
        }

        public void PublishGame()
        {
            Game gameToPublish = DialogUtils.InputGame();

            var message = gameToPublish.Encode();
            var header = new Header(HeaderConstants.Request, CommandConstants.PublishGame, message.Length);

            Utils.SendData(clientSocket, header, message);

            if (File.Exists(gameToPublish.Cover))
            {
                SendFile(gameToPublish.Cover, clientSocket);
            }
            Console.WriteLine(Utils.ReceiveMessageData(clientSocket));
        }

        public void PublishReview()
        {
            Game game = DialogUtils.SelectGame(GetGames());
            if (game == null)
            {
                Console.WriteLine("Retorno al menú.");
                return;
            }

            Review review = DialogUtils.InputReview();

            if(review == null)
            {
                Console.WriteLine("Retorno al menú.");
                return;
            }

            game.AddReview(review);

            var message = game.Encode();
            var header = new Header(HeaderConstants.Request, CommandConstants.PublishReview, message.Length);
            Utils.SendData(clientSocket, header, message);

            Console.WriteLine(Utils.ReceiveMessageData(clientSocket));
        }

        public void ModifyGame()
        {
            List<Game> games = GetGames();
            Game gameToModify = DialogUtils.SelectGame(games);

            if (gameToModify == null)
            {
                Console.WriteLine("Retorno al menú.");
                return;
            }

            var modifyingGameMessage = gameToModify.Encode();
            var modifyingGameHeader = new Header(HeaderConstants.Request, CommandConstants.ModifyingGame, modifyingGameMessage.Length);
            Utils.SendData(clientSocket, modifyingGameHeader, modifyingGameMessage);

            List<string> headerAndMessage = Utils.ReceiveCommandAndMessage(clientSocket);

            Console.WriteLine(headerAndMessage[1]);

            if (headerAndMessage[0] == CommandConstants.ModifyingGameError.ToString())
            {
                Console.WriteLine("Retorno al menú.");
                return;
            }
            
            Console.WriteLine("Ingrese los nuevos datos del juego. Si no quiere modificar el campo, presione ENTER.");
            Game modifiedGame = DialogUtils.InputGame();

            var modifyGameMessage = GameSystem.EncodeGames(new List<Game>() { gameToModify, modifiedGame });
            var modifyGameHeader = new Header(HeaderConstants.Request, CommandConstants.ModifyGame, modifyGameMessage.Length);
            Utils.SendData(clientSocket, modifyGameHeader, modifyGameMessage);

            if (File.Exists(modifiedGame.Cover))
            {
                SendFile(modifiedGame.Cover, clientSocket);
            }

            Console.WriteLine(Utils.ReceiveMessageData(clientSocket));
        }

        public void DeleteGame()
        {
            List<Game> games = GetGames();

            Game gameToDelete = DialogUtils.SelectGame(games);
            if (gameToDelete == null)
            {
                Console.WriteLine("Retorno al menú.");
                return;
            }

            var deleteGameMessage = gameToDelete.Encode();
            var deleteGameHeader = new Header(HeaderConstants.Request, CommandConstants.DeleteGame, deleteGameMessage.Length);
            Utils.SendData(clientSocket, deleteGameHeader, deleteGameMessage);

            Console.WriteLine(Utils.ReceiveMessageData(clientSocket));
        }
    }
}