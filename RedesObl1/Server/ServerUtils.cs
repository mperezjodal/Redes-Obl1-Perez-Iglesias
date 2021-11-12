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
using Grpc.Core;
using GRPCLibrary;
using System.Text.Json;

namespace Server
{
    public class ServerUtils
    {
        public GameSystem GameSystem;
        public TcpClient tcpClient;
        public object lockModifyGame = new object();
        public object lockDeleteGame = new object();
        public object lockAddGame = new object();
        public GameSystemService.GameSystemServiceClient grpcClient;
        public ServerUtils(GameSystem gameSystem, TcpClient tcpClient, GameSystemService.GameSystemServiceClient grpcClient)
        {
            this.GameSystem = gameSystem;
            this.tcpClient = tcpClient;
            this.grpcClient = grpcClient;
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

        public async Task GetGamesHandler()
        {
            await SendData(CommandConstants.GetGamesOk, GameSystem.EncodeGames());
        }

        public async Task GetUsersHandler()
        {
            await SendData(CommandConstants.GetUsersOk, GameSystem.EncodeUsers());
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

        public async Task GetAcquiredGamesHandler(string jsonUser)
        {
            try
            {
                User user = User.Decode(jsonUser);
                var systemUser = GameSystem.Users.Find(u => u.Name.Equals(user.Name));

                await SendData(CommandConstants.GetAcquiredGamesOk, systemUser.EncodeGames());
            }
            catch (Exception)
            {
                await SendData(CommandConstants.GetAcquiredGamesError, "[]");
            }
        }

        public async Task AcquireGameHandler(string jsonAcquireGameData)
        {
            try
            {
                UserGamePair userGame = UserGamePair.Decode(jsonAcquireGameData);
                var user = GameSystem.Users.Find(u => u.Name.Equals(userGame.User.Name));
                var game = GameSystem.Games.Find(g => g.Title.Equals(userGame.Game.Title));
                user.AcquireGame(game);

                await SendData(CommandConstants.AcquireGameOk, "Se ha adquirido el juego: " + game.Title + ".");
            }
            catch (Exception)
            {
                await SendData(CommandConstants.AcquireGameError, "No se ha podido adquirir el juego.");
            }
        }

        public async Task LoginHandler(string userName)
        {
            try
            {
                var response = await grpcClient.LoginAsync(new UserModel { Name = userName });
                if (response is UserModel)
                {
                    await SendData(CommandConstants.LoginOk, "Se ha ingresado con el usuario: " + userName + ".");
                    await SendData(CommandConstants.NewUser, userName);
                    return;
                }
            }
            catch (RpcException rpcException)
            {
                if (rpcException.StatusCode == StatusCode.AlreadyExists)
                {
                    await SendData(CommandConstants.LoginError, "El usuario ya existe y tiene una sesión abierta.");
                    return;
                }
            }
            catch (Exception) { }

            await SendData(CommandConstants.LoginError, "No se ha podido crear el usuario: " + userName + ".");
    }

        public async Task Logout(string jsonUser)
        {
            try
            {
                User userToLogout = User.Decode(jsonUser);
                await grpcClient.LogoutAsync(new UserModel { Name = userToLogout.Name });
            }
            catch { }
        }

        public async Task<List<Game>> BeingModifiedHandler(string jsonData, List<Game> gamesBeingModifiedByClient)
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

                await SendData(CommandConstants.ModifyingGameOk, "Se puede modificar el juego: " + game.Title + ".");
            }
            catch (Exception)
            {
                await SendData(CommandConstants.ModifyingGameError, "No se puede modificar el juego.");
            }
            return gamesBeingModifiedByClient;
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

                await SendData(CommandConstants.ModifyGameOk, "Se ha modificado el juego: " + gameToModify.Title + ".");
            }
            catch (Exception)
            {
                await SendData(CommandConstants.ModifyGameError, "No se ha podido modificar el juego.");
            }

            return gamesBeingModifiedByClient;
        }

        public async Task DeleteGameHandler(string jsonDeleteGameData)
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

                await SendData(CommandConstants.DeleteGameOk, "Se ha eliminado el juego: " + gameToDelete.Title + ".");
            }
            catch (Exception)
            {
                await SendData(CommandConstants.DeleteGameError, "No se ha podido eliminado el juego.");
            }
        }

        public async Task PublishReviewHandler(string jsonPublishReviewData)
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

                await SendData(CommandConstants.PublishReviewOk, "Se ha publicado la calificacion para el juego: " + gameToModify.Title + ".");
            }
            catch (Exception)
            {
                await SendData(CommandConstants.PublishReviewError, "No se ha podido publicar la calificacion del juego.");
            }
        }

        public async Task PublishGameHandler(string jsonPublishGame)
        {
            try
            {
                Game newGame = Game.Decode(jsonPublishGame);

                newGame.Cover = await handleGameCover(newGame.Cover);

                lock (lockAddGame)
                {
                    GameSystem.AddGame(newGame);
                }

                await SendData(CommandConstants.PublishGameOk, "Se ha publicado el juego: " + newGame.Title + ".");
            }
            catch (Exception)
            {
                await SendData(CommandConstants.PublishGameError, "No se ha podido publicar juego.");
            }
        }

        public async Task<string> handleGameCover(string cover)
        {
            if (File.Exists(cover))
            {
                await ReciveFile();
                var fileInfo = new FileInfo(cover);
                return fileInfo.Name;
            }
            else
            {
                return null;
            }
        }

        public async Task SendData(int command, string message)
        {
            var header = new Header(HeaderConstants.Response, command, message.Length);
            await Utils.ServerSendData(tcpClient.GetStream(), header, message);
        }
    }
}