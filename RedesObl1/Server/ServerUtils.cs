using System;
using System.Collections.Generic;
using System.Net.Sockets;
using ConnectionUtils;
using Domain;
using ProtocolLibrary;
using System.IO;
using FileStreamLibrary;
using FileStreamLibrary.Protocol;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using GRPCLibrary;
using System.Text.Json;

namespace Server
{
    public class ServerUtils
    {
        public TcpClient tcpClient;
        public string username;
        public GameSystemModel.GameSystemModelClient grpcClient;
        public ServerUtils(TcpClient tcpClient, GameSystemModel.GameSystemModelClient grpcClient)
        {
            this.tcpClient = tcpClient;
            this.grpcClient = grpcClient;
        }

        public async Task SendFileAsync(string path)
        {
            var fileCommunication = new FileCommunicationHandler(tcpClient.GetStream());
            await fileCommunication.SendFileAsync(path);
        }

        public async Task ReceiveFileAsync()
        {
            var fileCommunicationGameList = new FileCommunicationHandler(tcpClient.GetStream());
            await fileCommunicationGameList.ReceiveFileAsync();
        }

        public async Task TransferFileAsync()
        {
            var fileCommunicationGameList = new FileCommunicationHandler(tcpClient.GetStream());
            await fileCommunicationGameList.TransferFileAsync(grpcClient);
        }

        public async Task GetGamesHandlerAsync()
        {
            try
            {
                var response = await grpcClient.GetGamesAsync(new EmptyRequest());
                List<Game> games = ProtoBuilder.Games(response);
                await SendDataAsync(CommandConstants.GetGamesOk, GameSystem.EncodeGames(games));
            }
            catch (Exception) { }
        }

        public async Task GetUsersHandlerAsync()
        {
            try
            {
                var response = await grpcClient.GetUsersAsync(new EmptyRequest());
                List<User> users = ProtoBuilder.Users(response);
                await SendDataAsync(CommandConstants.GetGamesOk, GameSystem.EncodeUsers(users));
            }
            catch (Exception) { }
        }

        public async Task GetGameCoverAsync(string jsonGame)
        {
            try
            {
                Game g = Game.Decode(jsonGame);

                if (g.Cover != null)
                {
                    CoverSize coverSize = await grpcClient.GetCoverSizeAsync(new CoverRequest { Cover = g.Cover });
                    var parts = Specification.GetParts(coverSize.Size);
                    
                    long offset = 0;

                    for (int i = 1; i <= parts; i++)
                    {
                        CoverModel response = await grpcClient.GetCoverAsync(new CoverRequest { Cover = g.Cover, Part = i, Offset = offset });
                        
                        offset += response.Data.Length;

                        FileStreamHandler _fileStreamHandler = new FileStreamHandler();
                        await _fileStreamHandler.WriteDataAsync(response.FileName, response.Data.ToByteArray());
                    }

                    var path = Path.Combine(Directory.GetCurrentDirectory(), g.Cover);
                    if (File.Exists(path))
                    {
                        await SendFileAsync(path);
                    }
                }

                
            }
            catch (Exception){ }
        }

        public async Task GetAcquiredGamesHandlerAsync()
        {
            try
            {
                var response = await grpcClient.GetAcquiredGamesAsync(new UserModel() { Name = this.username });
                List<Game> games = ProtoBuilder.Games(response);
                await SendDataAsync(CommandConstants.GetAcquiredGamesOk, GameSystem.EncodeGames(games));
            }
            catch (Exception)
            {
                await SendDataAsync(CommandConstants.GetAcquiredGamesError, "[]");
            }
        }

        public async Task AcquireGameHandlerAsync(string jsonGame)
        {
            try
            {
                Game game = Game.Decode(jsonGame);

                if (await grpcClient.AcquireGameAsync(ProtoBuilder.GameModel(game, this.username)) is GameModel)
                {
                    await SendDataAsync(CommandConstants.AcquireGameOk, "Se ha adquirido el juego: " + game.Title + ".");
                }
            }
            catch (Exception)
            {
                await SendDataAsync(CommandConstants.AcquireGameError, "No se ha podido adquirir el juego.");
            }
        }

        public async Task LoginHandlerAsync(string userName)
        {
            try
            {
                if (await grpcClient.LoginAsync(new UserModel { Name = userName }) is UserModel)
                {
                    await SendDataAsync(CommandConstants.LoginOk, "Se ha ingresado con el usuario: " + userName + ".");
                    await SendDataAsync(CommandConstants.NewUser, userName);
                    this.username = userName;
                    return;
                }
            }
            catch (AlreadyExistsException)
            {
                await SendDataAsync(CommandConstants.LoginError, "El usuario ya existe y tiene una sesión abierta.");
                return;
            }
            catch (Exception) 
            { 
                await SendDataAsync(CommandConstants.LoginError, "No se ha podido crear el usuario: " + userName + ".");
            }
        }

        public async Task LogoutAsync(string jsonUser)
        {
            try
            {
                User userToLogout = User.Decode(jsonUser);
                await grpcClient.LogoutAsync(new UserModel { Name = userToLogout.Name });
            }
            catch { }
        }

        public async Task BeingModifiedHandlerAsync(string jsonData)
        {
            try
            {
                Game game = Game.Decode(jsonData);

                if (await grpcClient.ToModifyAsync(ProtoBuilder.GameModel(game, this.username)) is GameModel)
                {
                    await SendDataAsync(CommandConstants.ModifyingGameOk, "Se puede modificar el juego: " + game.Title + ".");
                }
            }
            catch (Exception)
            {
                await SendDataAsync(CommandConstants.ModifyingGameError, "No se puede modificar el juego.");
            }
        }

        public async Task ModifyGameHandlerAsync(string jsonModifyGameData)
        {
            try
            {
                List<Game> updatingGames = GameSystem.DecodeGames(jsonModifyGameData);

                if (updatingGames[1].Cover != null)
                {
                    updatingGames[1].Cover = await handleGameCoverAsync(updatingGames[1].Cover);
                }

                var response = await grpcClient.UpdateGameAsync(ProtoBuilder.GamesModel(updatingGames, this.username));
                if (response.Code != 4)
                {
                    await SendDataAsync(CommandConstants.ModifyingGameOk, "Se ha modificado el juego: " + response.Title + ".");
                }
                else
                {
                    await SendDataAsync(CommandConstants.DeleteGameError, "El juego está siendo modificado.");
                }
            }
            catch (Exception)
            {
                await SendDataAsync(CommandConstants.ModifyGameError, "No se ha podido modificar el juego.");
            }
        }

        public async Task DeleteGameHandlerAsync(string jsonDeleteGameData)
        {
            try
            {
                Game gameToDelete = Game.Decode(jsonDeleteGameData);
                
                var result = await grpcClient.DeleteGameAsync(ProtoBuilder.GameModel(gameToDelete));
                if (result.Code != 4)
                {
                    await SendDataAsync(CommandConstants.DeleteGameOk, "Se ha eliminado el juego: " + gameToDelete.Title + ".");
                }
                else
                {
                    await SendDataAsync(CommandConstants.DeleteGameError, "El juego está siendo modificado.");
                }
            }
            catch (Exception)
            {
                await SendDataAsync(CommandConstants.DeleteGameError, "No se ha podido eliminado el juego.");
            }
        }

        public async Task PublishReviewHandlerAsync(string jsonPublishReviewData)
        {
            try
            {
                Game publishReviewGame = Game.Decode(jsonPublishReviewData);
                if (await grpcClient.PostReviewAsync(ProtoBuilder.GameModel(publishReviewGame, this.username)) is GameModel)
                {
                    await SendDataAsync(CommandConstants.PublishReviewOk, "Se ha publicado la calificacion para el juego: " + publishReviewGame.Title + ".");
                    return;
                }
            }
            catch (AlreadyModifyingException)
            {
                await SendDataAsync(CommandConstants.LoginError, "No se ha podido publicar la calificacion del juego porque está siendo modificado.");
                return;
            }
            catch (Exception) { }

            await SendDataAsync(CommandConstants.PublishReviewError, "No se ha podido publicar la calificacion del juego.");
        }

        public async Task PublishGameHandlerAsync(string jsonPublishGame)
        {
            try
            {
                Game newGame = Game.Decode(jsonPublishGame);

                if (newGame.Cover != null)
                {
                    newGame.Cover = await handleGameCoverAsync(newGame.Cover);
                }

                if (await grpcClient.PostGameAsync(ProtoBuilder.GameModel(newGame)) is GameModel)
                {
                    await SendDataAsync(CommandConstants.PublishGameOk, "Se ha publicado el juego: " + newGame.Title + ".");
                    return;
                }
            }
            catch (Exception) { }

            await SendDataAsync(CommandConstants.PublishGameError, "No se ha podido publicar juego.");
        }

        public async Task<string> handleGameCoverAsync(string cover)
        {
            if (File.Exists(cover))
            {
                await TransferFileAsync();
                var fileInfo = new FileInfo(cover);
                return fileInfo.Name;
            }
            else
            {
                return null;
            }
        }

        public async Task SendDataAsync(int command, string message)
        {
            var header = new Header(HeaderConstants.Response, command, message.Length);
            await Utils.ServerSendDataAsync(tcpClient.GetStream(), header, message);
        }
    }
}