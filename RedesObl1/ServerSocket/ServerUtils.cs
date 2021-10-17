using System;
using System.Collections.Generic;
using System.Net.Sockets;
using SocketUtils;
using Domain;
using ProtocolLibrary;
using System.IO;
using FileStreamLibrary;
using System.Linq;
using System.Threading.Tasks;

namespace ServerSocket
{
    public class ServerUtils
    {
        public GameSystem GameSystem;
        public TcpClient tcpClient;
        public object lockModifyGame = new object();
        public object lockDeleteGame = new object();
        public object lockGetGames = new object();
        public object lockAddGame = new object();
        public ServerUtils(GameSystem gameSystem, TcpClient tcpClient)
        {
            this.GameSystem = gameSystem;
            this.tcpClient = tcpClient;
        }

        public async Task SendFile(string path)
        {
            var fileCommunication = new FileCommunicationHandler(tcpClient.GetStream());
            await fileCommunication.SendFileAsync(path);
        }

        public async Task ReciveFile()
        {
            var fileCommunicationGameList = new FileCommunicationHandler(tcpClient.GetStream());
            await fileCommunicationGameList.ReceiveFileAsync();
        }

        public void GetGamesHandler()
        {
            lock (lockGetGames)
            {
                var gamesMessage = GameSystem.EncodeGames();
                var gamesHeader = new Header(HeaderConstants.Response, CommandConstants.GetGamesOk, gamesMessage.Length);
                Utils.SendData(tcpClient.GetStream(), gamesHeader, gamesMessage);
            }
        }

        public async Task GetGameCover(string jsonGame)
        {
            Game g = Game.Decode(jsonGame);
            if (g.Cover != null)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), g.Cover);
                if (File.Exists(path))
                {
                    await SendFile(path);
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
                Utils.SendData(tcpClient.GetStream(), AcquireGameHeader, gamesMessage);
            }
            catch (Exception)
            {
                var AcquireGameMessage = "[]";
                var AcquireGameHeader = new Header(HeaderConstants.Response, CommandConstants.GetAcquiredGamesError, AcquireGameMessage.Length);
                Utils.SendData(tcpClient.GetStream(), AcquireGameHeader, AcquireGameMessage);
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
                Utils.SendData(tcpClient.GetStream(), AcquireGameHeader, AcquireGameMessage);
            }
            catch (Exception)
            {
                var AcquireGameMessage = "No se ha podido adquirir el juego.";
                var AcquireGameHeader = new Header(HeaderConstants.Response, CommandConstants.AcquireGameError, AcquireGameMessage.Length);
                Utils.SendData(tcpClient.GetStream(), AcquireGameHeader, AcquireGameMessage);
            }
        }

        public void LoginHandler(string userName)
        {
            User existingUser = this.GameSystem.Users.Find(u => u.Name.Equals(userName));
            if (existingUser != null && existingUser.Login)
            {
                var loginError = "Usuario tiene una sesion abierta.";
                var loginErrorHeader = new Header(HeaderConstants.Response, CommandConstants.LoginError, loginError.Length);
                Utils.SendData(tcpClient.GetStream(), loginErrorHeader, loginError);
                return;
            }

            User newUser = GameSystem.AddUser(userName);
            GameSystem.LoginUser(userName);

            var loginMessage = "Se ha ingresado con el usuario: " + userName + ".";
            var loginHeader = new Header(HeaderConstants.Response, CommandConstants.LoginOk, loginMessage.Length);
            Utils.SendData(tcpClient.GetStream(), loginHeader, loginMessage);

            var userMessage = newUser.Encode();
            var userHeader = new Header(HeaderConstants.Response, CommandConstants.NewUser, userMessage.Length);
            Utils.SendData(tcpClient.GetStream(), userHeader, userMessage);
        }

        public void Logout(string jsonUser)
        {
            try
            {
                User userToLogout = User.Decode(jsonUser);
                GameSystem.LogoutUser(userToLogout.Name);
            }
            catch { }
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
                Utils.SendData(tcpClient.GetStream(), modifyGameHeader, modifyGameMessage);
            }
            catch (Exception)
            {
                var modifyGameMessage = "No se puede modificar el juego.";
                var modifyGameHeader = new Header(HeaderConstants.Response, CommandConstants.ModifyingGameError, modifyGameMessage.Length);
                Utils.SendData(tcpClient.GetStream(), modifyGameHeader, modifyGameMessage);
            }
        }

        public async Task<List<Game>> ModifyGameHandler(string jsonModifyGameData, List<Game> gamesBeingModifiedByClient)
        {
            try
            {
                List<Game> updatingGames = GameSystem.DecodeGames(jsonModifyGameData);
                var gameToModify = GameSystem.Games.Find(g => g.Title.Equals(updatingGames[0].Title));

                updatingGames[1].Cover = await handleGameCover(updatingGames[1].Cover);

                lock (lockModifyGame)
                {
                    if (GameSystem.IsGameBeingModified(gameToModify) && gamesBeingModifiedByClient.FindIndex(g => g.Title == gameToModify.Title) == -1)
                    {
                        throw new Exception();
                    }

                    GameSystem.DeleteGameBeingModified(gameToModify);
                    gamesBeingModifiedByClient.RemoveAll(g => g.Title.Equals(gameToModify.Title));

                    GameSystem.UpdateGame(gameToModify, updatingGames[1]);
                }

                var modifyGameMessage = "Se ha modificado el juego: " + gameToModify.Title + ".";
                var modifyGameHeader = new Header(HeaderConstants.Response, CommandConstants.ModifyGameOk, modifyGameMessage.Length);
                Utils.SendData(tcpClient.GetStream(), modifyGameHeader, modifyGameMessage);
            }
            catch (Exception)
            {
                var modifyGameMessage = "No se ha podido modificar el juego.";
                var modifyGameHeader = new Header(HeaderConstants.Response, CommandConstants.ModifyGameError, modifyGameMessage.Length);
                Utils.SendData(tcpClient.GetStream(), modifyGameHeader, modifyGameMessage);
            }

            return gamesBeingModifiedByClient;
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

                lock (lockDeleteGame)
                {
                    this.GameSystem.DeleteGame(gameToDelete);
                }

                var deleteGameMessage = "Se ha eliminado el juego: " + gameToDelete.Title + ".";
                var deleteGameHeader = new Header(HeaderConstants.Response, CommandConstants.DeleteGameOk, deleteGameMessage.Length);
                Utils.SendData(tcpClient.GetStream(), deleteGameHeader, deleteGameMessage);
            }
            catch (Exception)
            {
                var deleteGameMessage = "No se ha podido eliminado el juego.";
                var deleteGameHeader = new Header(HeaderConstants.Response, CommandConstants.DeleteGameError, deleteGameMessage.Length);
                Utils.SendData(tcpClient.GetStream(), deleteGameHeader, deleteGameMessage);
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
                Utils.SendData(tcpClient.GetStream(), publishReviewHeader, publishReviewMessage);
            }
            catch (Exception)
            {
                var publishReviewMessage = "No se ha podido publicar la calificacion del juego.";
                var publishReviewHeader = new Header(HeaderConstants.Response, CommandConstants.PublishReviewError, publishReviewMessage.Length);
                Utils.SendData(tcpClient.GetStream(), publishReviewHeader, publishReviewMessage);
            }
        }

        public async void PublishGameHandler(string jsonPublishGame)
        {
            try
            {
                Game newGame = Game.Decode(jsonPublishGame);

                newGame.Cover = await handleGameCover(newGame.Cover);

                lock (lockAddGame)
                {
                    GameSystem.AddGame(newGame);
                }

                var publishedGameMessage = "Se ha publicado el juego: " + newGame.Title + ".";
                var publishedGameHeader = new Header(HeaderConstants.Response, CommandConstants.PublishGameOk, publishedGameMessage.Length);
                Utils.SendData(tcpClient.GetStream(), publishedGameHeader, publishedGameMessage);

            }
            catch (Exception)
            {
                var publishGameMessage = "No se ha podido publicar juego.";
                var publishGameHeader = new Header(HeaderConstants.Response, CommandConstants.PublishGameError, publishGameMessage.Length);
                Utils.SendData(tcpClient.GetStream(), publishGameHeader, publishGameMessage);
            }
        }

        public async Task<string> handleGameCover(string cover)
        {
            if (File.Exists(cover))
            {
                await ReciveFile();
                var fileInfo = new FileInfo(cover);
                string fileName = fileInfo.Name;
                return fileName;
            }
            else
            {
                return null;
            }
        } 
    }
}