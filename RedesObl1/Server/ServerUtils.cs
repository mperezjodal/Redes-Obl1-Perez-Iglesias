using System;
using System.Collections.Generic;
using System.Net.Sockets;
using ConnectionUtils;
using Domain;
using ProtocolLibrary;
using System.IO;
using FileStreamLibrary;
using System.Linq;
using System.Threading.Tasks;

namespace Server
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
                SendData(CommandConstants.GetGamesOk, GameSystem.EncodeGames());
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

                SendData(CommandConstants.GetAcquiredGamesOk, systemUser.EncodeGames());
            }
            catch (Exception)
            {
                SendData(CommandConstants.GetAcquiredGamesError, "[]");
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

                SendData(CommandConstants.AcquireGameOk, "Se ha adquirido el juego: " + game.Title + ".");
            }
            catch (Exception)
            {
                SendData(CommandConstants.AcquireGameError, "No se ha podido adquirir el juego.");
            }
        }

        public void LoginHandler(string userName)
        {
            User existingUser = this.GameSystem.Users.Find(u => u.Name.Equals(userName));
            if (existingUser != null && existingUser.Login)
            {
                var loginError = "Usuario tiene una sesion abierta.";
                var loginErrorHeader = new Header(HeaderConstants.Response, CommandConstants.LoginError, loginError.Length);
                Utils.ServerSendData(tcpClient.GetStream(), loginErrorHeader, loginError);
                return;
            }

            if (existingUser == null)
            {
                User newUser = GameSystem.AddUser(userName);
            }


            GameSystem.LoginUser(userName);

            SendData(CommandConstants.LoginOk, "Se ha ingresado con el usuario: " + userName + ".");

            SendData(CommandConstants.NewUser, userName);
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

                SendData(CommandConstants.ModifyingGameOk, "Se puede modificar el juego: " + game.Title + ".");
            }
            catch (Exception)
            {
                SendData(CommandConstants.ModifyingGameError, "No se puede modificar el juego.");
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

                SendData(CommandConstants.ModifyGameOk, "Se ha modificado el juego: " + gameToModify.Title + ".");
            }
            catch (Exception)
            {
                SendData(CommandConstants.ModifyGameError, "No se ha podido modificar el juego.");
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

                SendData(CommandConstants.DeleteGameOk, "Se ha eliminado el juego: " + gameToDelete.Title + ".");
            }
            catch (Exception)
            {
                SendData(CommandConstants.DeleteGameError, "No se ha podido eliminado el juego.");
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

                SendData(CommandConstants.PublishReviewOk, "Se ha publicado la calificacion para el juego: " + gameToModify.Title + ".");
            }
            catch (Exception)
            {
                SendData(CommandConstants.PublishReviewError, "No se ha podido publicar la calificacion del juego.");
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

                SendData(CommandConstants.PublishGameOk, "Se ha publicado el juego: " + newGame.Title + ".");
            }
            catch (Exception)
            {
                SendData(CommandConstants.PublishGameError, "No se ha podido publicar juego.");
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

        public void SendData(int command, string message)
        {
            var header = new Header(HeaderConstants.Response, command, message.Length);
            Utils.ServerSendData(tcpClient.GetStream(), header, message);
        }
    }
}