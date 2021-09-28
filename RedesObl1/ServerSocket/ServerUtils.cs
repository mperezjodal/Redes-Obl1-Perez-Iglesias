using System.Security.AccessControl;
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
using Common;
using System.Text.Json;

namespace ServerSocket
{
    public class ServerUtils
    {
        public GameSystem GameSystem;
        public Socket clientSocket;
        public object lockModifyObject = new object();
        public object lockDeleteObject = new object();
        public ServerUtils(GameSystem gameSystem, Socket clientSocket)
        {
            this.GameSystem = gameSystem;
            this.clientSocket = clientSocket;
        }

        public void GetGamesHandler(Socket clientSocket)
        {
            var gamesMessage = GameSystem.EncodeGames();
            var gamesHeader = new Header(HeaderConstants.Response, CommandConstants.GetGamesOk, gamesMessage.Length);
            Utils.SendData(clientSocket, gamesHeader, gamesMessage);

            foreach (Game g in GameSystem.Games)
            {
                if (g.Cover != null)
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), g.Cover);
                    if (File.Exists(path))
                    {
                        var fileCommunicationGameList = new FileCommunicationHandler(clientSocket);
                        fileCommunicationGameList.SendFile(path);
                    }
                }
            }
        }

        public void GetAcquiredGamesHandler(string jsonUser)
        {
            try
            {
                User user = User.Decode(jsonUser);
                var systemUser = GameSystem.Users.Find(u => u.Name.Equals(user.Name));

                var gamesMessage = systemUser.EncodeGames();
                var AcquireGameHeader = new Header(HeaderConstants.Response, CommandConstants.GetAcquiredGamesOk, gamesMessage.Length);
                Utils.SendData(clientSocket, AcquireGameHeader, gamesMessage);
            }
            catch (Exception)
            {
                var AcquireGameMessage = "[]";
                var AcquireGameHeader = new Header(HeaderConstants.Response, CommandConstants.GetAcquiredGamesError, AcquireGameMessage.Length);
                Utils.SendData(clientSocket, AcquireGameHeader, AcquireGameMessage);
            }
        }

        public void AcquireGameHandler(string jsonAcquireGameData)
        {
            try
            {
                UserGamePair userGame = UserGamePair.Decode(jsonAcquireGameData);
                var user = GameSystem.Users.Find(u => u.Name.Equals(userGame.User.Name));
                var game = GameSystem.Games.Find(g => g.Title.Equals(userGame.Game.Title));
                user.AcquireGame(game);

                var AcquireGameMessage = "Se ha adquirido el juego: " + game.Title + ".";
                var AcquireGameHeader = new Header(HeaderConstants.Response, CommandConstants.AcquireGameOk, AcquireGameMessage.Length);
                Utils.SendData(clientSocket, AcquireGameHeader, AcquireGameMessage);
            }
            catch (Exception)
            {
                var AcquireGameMessage = "No se ha podido adquirir el juego.";
                var AcquireGameHeader = new Header(HeaderConstants.Response, CommandConstants.AcquireGameError, AcquireGameMessage.Length);
                Utils.SendData(clientSocket, AcquireGameHeader, AcquireGameMessage);
            }
        }

        public void LoginHandler(string userName)
        {
            if (this.GameSystem.Users.FindIndex(u => u.Name.Equals(userName)) != -1)
            {
                var loginError = "Usuario ya existe.";
                var loginErrorHeader = new Header(HeaderConstants.Response, CommandConstants.LoginError, loginError.Length);
                Utils.SendData(clientSocket, loginErrorHeader, loginError);
                return;
            }

            User newUser = GameSystem.AddUser(userName);

            var loginMessage = "Se ha creado el usuario: " + userName + ".";
            var loginHeader = new Header(HeaderConstants.Response, CommandConstants.LoginOk, loginMessage.Length);
            Utils.SendData(clientSocket, loginHeader, loginMessage);

            var userMessage = newUser.Encode();
            var userHeader = new Header(HeaderConstants.Response, CommandConstants.NewUser, userMessage.Length);
            Utils.SendData(clientSocket, userHeader, userMessage);
        }

        public void BeingModifiedHandler(string jsonData, ref List<Game> gamesBeingModifiedByClient)
        {
            try
            {
                Game game = Game.Decode(jsonData);

                if (GameSystem.GamesBeingModified.FindIndex(g => g.Title == game.Title) != -1)
                {
                    throw new Exception();
                }

                GameSystem.AddGameBeingModified(game);
                gamesBeingModifiedByClient.Add(game);

                var modifyGameMessage = "Se puede modificar el juego: " + game.Title + ".";
                var modifyGameHeader = new Header(HeaderConstants.Response, CommandConstants.ModifyingGameOk, modifyGameMessage.Length);
                Utils.SendData(clientSocket, modifyGameHeader, modifyGameMessage);
            }
            catch (Exception)
            {
                var modifyGameMessage = "No se puede modificar el juego.";
                var modifyGameHeader = new Header(HeaderConstants.Response, CommandConstants.ModifyingGameError, modifyGameMessage.Length);
                Utils.SendData(clientSocket, modifyGameHeader, modifyGameMessage);
            }
        }

        public void ModifyGameHandler(string jsonModifyGameData, ref List<Game> gamesBeingModifiedByClient)
        {
            try
            {
                List<Game> updatingGames = GameSystem.DecodeGames(jsonModifyGameData);
                var gameToModify = GameSystem.Games.Find(g => g.Title.Equals(updatingGames[0].Title));

                lock (lockModifyObject)
                {
                    if (GameSystem.IsGameBeingModified(gameToModify) && gamesBeingModifiedByClient.FindIndex(g => g.Title == gameToModify.Title) == -1)
                    {
                        throw new Exception();
                    }

                    GameSystem.DeleteGameBeingModified(gameToModify);
                    gamesBeingModifiedByClient.RemoveAll(g => g.Title.Equals(gameToModify.Title));
                    
                    GameSystem.UpdateGame(gameToModify, updatingGames[1]);
                }

                if (File.Exists(gameToModify.Cover))
                {
                    var fileCommunication = new FileCommunicationHandler(clientSocket);
                    var fileName = fileCommunication.ReceiveFile();
                    gameToModify.Cover = fileName;
                }

                var modifyGameMessage = "Se ha modificado el juego: " + gameToModify.Title + ".";
                var modifyGameHeader = new Header(HeaderConstants.Response, CommandConstants.ModifyGameOk, modifyGameMessage.Length);
                Utils.SendData(clientSocket, modifyGameHeader, modifyGameMessage);
            }
            catch (Exception)
            {
                var modifyGameMessage = "No se ha podido modificar el juego.";
                var modifyGameHeader = new Header(HeaderConstants.Response, CommandConstants.ModifyGameError, modifyGameMessage.Length);
                Utils.SendData(clientSocket, modifyGameHeader, modifyGameMessage);
            }
        }

        public void DeleteGameHandler(string jsonDeleteGameData)
        {
            try
            {
                Game gameToDelete = Game.Decode(jsonDeleteGameData);

                if (GameSystem.IsGameBeingModified(gameToDelete))
                {
                    throw new Exception();
                }

                lock (lockDeleteObject)
                {
                    this.GameSystem.DeleteGame(gameToDelete);
                }

                var deleteGameMessage = "Se ha eliminado el juego: " + gameToDelete.Title + ".";
                var deleteGameHeader = new Header(HeaderConstants.Response, CommandConstants.DeleteGameOk, deleteGameMessage.Length);
                Utils.SendData(clientSocket, deleteGameHeader, deleteGameMessage);
            }
            catch (Exception)
            {
                var deleteGameMessage = "No se ha podido eliminado el juego.";
                var deleteGameHeader = new Header(HeaderConstants.Response, CommandConstants.DeleteGameError, deleteGameMessage.Length);
                Utils.SendData(clientSocket, deleteGameHeader, deleteGameMessage);
            }
        }

        public void PublishReviewHandler(string jsonPublishReviewData)
        {
            try
            {
                Game publishReviewGame = Game.Decode(jsonPublishReviewData);
                var gameToModify = GameSystem.Games.Find(g => g.Title.Equals(publishReviewGame.Title));

                if (GameSystem.IsGameBeingModified(gameToModify))
                {
                    throw new Exception();
                }

                GameSystem.UpdateReviews(gameToModify, publishReviewGame.Reviews);

                var publishReviewMessage = "Se ha publicado la calificacion para el juego: " + gameToModify.Title + ".";
                var publishReviewHeader = new Header(HeaderConstants.Response, CommandConstants.PublishReviewOk, publishReviewMessage.Length);
                Utils.SendData(clientSocket, publishReviewHeader, publishReviewMessage);
            }
            catch (Exception)
            {
                var publishReviewMessage = "No se ha podido publicar la calificacion del juego.";
                var publishReviewHeader = new Header(HeaderConstants.Response, CommandConstants.PublishReviewError, publishReviewMessage.Length);
                Utils.SendData(clientSocket, publishReviewHeader, publishReviewMessage);
            }
        }

        public void PublishGameHandler(string jsonPublishGame, Socket serverSocket)
        {
            try
            {
                Game newGame = Game.Decode(jsonPublishGame);
                GameSystem.AddGame(newGame);

                var publishedGameMessage = "Se ha publicado el juego: " + newGame.Title + ".";
                var publishedGameHeader = new Header(HeaderConstants.Response, CommandConstants.PublishGameOk, publishedGameMessage.Length);
                Utils.SendData(clientSocket, publishedGameHeader, publishedGameMessage);

                if (File.Exists(newGame.Cover))
                {
                    var fileCommunication = new FileCommunicationHandler(clientSocket);
                    var fileName = fileCommunication.ReceiveFile();
                    newGame.Cover = fileName;
                }

            }
            catch (Exception)
            {
                var publishGameMessage = "No se ha podido publicar juego.";
                var publishGameHeader = new Header(HeaderConstants.Response, CommandConstants.PublishGameError, publishGameMessage.Length);
                Utils.SendData(clientSocket, publishGameHeader, publishGameMessage);
            }
        }
    }
}