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
        public string username;
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
            try
            {
                var response = await grpcClient.GetGamesAsync(new EmptyRequest());
                List<Game> games = ProtoBuilder.Games(response);
                await SendData(CommandConstants.GetGamesOk, GameSystem.EncodeGames(games));
            }
            catch (Exception) { }
        }

        public async Task GetUsersHandler()
        {
            try
            {
                var response = await grpcClient.GetUsersAsync(new EmptyRequest());
                List<User> users = ProtoBuilder.Users(response);
                await SendData(CommandConstants.GetGamesOk, GameSystem.EncodeUsers(users));
            }
            catch (Exception) { }
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
                if (await grpcClient.LoginAsync(new UserModel { Name = userName }) is UserModel)
                {
                    await SendData(CommandConstants.LoginOk, "Se ha ingresado con el usuario: " + userName + ".");
                    await SendData(CommandConstants.NewUser, userName);
                    this.username = userName;
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

        public async Task BeingModifiedHandler(string jsonData)
        {
            try
            {
                Game game = Game.Decode(jsonData);

                if (await grpcClient.ToModifyAsync(ProtoBuilder.GameModel(game, this.username)) is GameModel)
                {
                    await SendData(CommandConstants.ModifyingGameOk, "Se puede modificar el juego: " + game.Title + ".");
                }
            }
            catch (Exception) { }

            await SendData(CommandConstants.ModifyingGameError, "No se puede modificar el juego.");
        }

        public async Task ModifyGameHandler(string jsonModifyGameData)
        {
            try
            {
                List<Game> updatingGames = GameSystem.DecodeGames(jsonModifyGameData);

                var response = await grpcClient.UpdateGameAsync(ProtoBuilder.GamesModel(updatingGames, this.username));
                if (response is GameModel)
                {
                    await SendData(CommandConstants.ModifyingGameOk, "Se ha modificado el juego: " + response.Title + ".");
                }
            }
            catch (Exception) { }

            await SendData(CommandConstants.ModifyGameError, "No se ha podido modificar el juego.");
        }

        public async Task DeleteGameHandler(string jsonDeleteGameData)
        {
            try
            {
                Game gameToDelete = Game.Decode(jsonDeleteGameData);

                if (await grpcClient.DeleteGameAsync(ProtoBuilder.GameModel(gameToDelete)) is GameModel)
                {
                    await SendData(CommandConstants.DeleteGameOk, "Se ha eliminado el juego: " + gameToDelete.Title + ".");
                }
            }
            catch (Exception) { }

            await SendData(CommandConstants.DeleteGameError, "No se ha podido eliminado el juego.");
        }

        public async Task PublishReviewHandler(string jsonPublishReviewData)
        {
            try
            {
                Game publishReviewGame = Game.Decode(jsonPublishReviewData);
                if (await grpcClient.PostReviewAsync(ProtoBuilder.GameModel(publishReviewGame)) is GameModel)
                {
                    await SendData(CommandConstants.PublishReviewOk, "Se ha publicado la calificacion para el juego: " + publishReviewGame.Title + ".");
                    return;
                }
            }
            catch (RpcException rpcException)
            {
                if (rpcException.StatusCode == StatusCode.AlreadyExists)
                {
                    await SendData(CommandConstants.LoginError, "No se ha podido publicar la calificacion del juego porque está siendo modificado.");
                    return;
                }
            }
            catch (Exception) { }

            await SendData(CommandConstants.PublishReviewError, "No se ha podido publicar la calificacion del juego.");
        }

        public async Task PublishGameHandler(string jsonPublishGame)
        {
            try
            {
                Game newGame = Game.Decode(jsonPublishGame);
                string cover = await handleGameCover(newGame.Cover);

                if (await grpcClient.PostGameAsync(ProtoBuilder.GameModel(newGame)) is GameModel)
                {
                    await SendData(CommandConstants.PublishGameOk, "Se ha publicado el juego: " + newGame.Title + ".");

                    if (cover != null)
                    {
                        await grpcClient.PostGameCoverAsync(new GameCoverModel { Id = newGame.Id, Cover = cover });
                    }
                    return;
                }
            }
            catch (Exception) { }

            await SendData(CommandConstants.PublishGameError, "No se ha podido publicar juego.");
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