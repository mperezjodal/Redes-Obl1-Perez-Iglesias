using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DisplayUtils;
using Domain;
using FileStreamLibrary;
using GRPCLibrary;
using FileStreamLibrary.Protocol;
using System.Net.Sockets;
using Grpc.Core;

namespace Server
{
    public class ServerMenuUtils
    {
        private GameSystemModel.GameSystemModelClient grpcClient;
        public ServerMenuUtils(GameSystemModel.GameSystemModelClient grpcClient)
        {
            this.grpcClient = grpcClient;
        }

        public async Task ShowGamesAsync()
        {
            Game gameToShow = DialogUtils.SelectGame(await GetGamesAsync());

            if (gameToShow != null && !String.IsNullOrEmpty(gameToShow.Cover))
            {
                await ReceiveGameCoverAsync(gameToShow);
            }

            DialogUtils.ShowGameDetail(gameToShow);
        }

        public async Task ReceiveGameCoverAsync(Game game)
        {
            try
            {
                if (game.Cover != null)
                {
                    CoverSize coverSize = await grpcClient.GetCoverSizeAsync(new CoverRequest { Cover = game.Cover });
                    var parts = Specification.GetParts(coverSize.Size);
                    
                    long offset = 0;

                    for (int i = 1; i <= parts; i++)
                    {
                        CoverModel response = await grpcClient.GetCoverAsync(new CoverRequest { Cover = game.Cover, Part = i, Offset = offset });
                        
                        offset += response.Data.Length;

                        FileStreamHandler _fileStreamHandler = new FileStreamHandler();
                        await _fileStreamHandler.WriteDataAsync(response.FileName, response.Data.ToByteArray());
                    }
                }

            }
            catch (Exception){ }
        }
        public async Task<List<Game>> GetGamesAsync()
        {
            try 
            {
                return ProtoBuilder.Games(await grpcClient.GetGamesAsync(new EmptyRequest()));
            }
            catch (RpcException)
            {
                Console.WriteLine("No se ha podido conectar al servidor.");
                return new List<Game>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new List<Game>();
            }
        }

        public async Task<List<User>> GetUsersAsync()
        {
            try 
            {
                return ProtoBuilder.Users(await grpcClient.GetUsersAsync(new EmptyRequest()));
            }
            catch (RpcException)
            {
                Console.WriteLine("No se ha podido conectar al servidor.");
                return new List<User>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new List<User>();
            }
        }

        public async Task InsertGameAsync()
        {
            Game gameToPublish = DialogUtils.InputGame();
            try
            {
                if (gameToPublish.Cover != null && File.Exists(gameToPublish.Cover))
                {                    
                    await FileCommunicationHandler.GrpcSendFileAsync(grpcClient, gameToPublish.Cover);

                    gameToPublish.Cover = gameToPublish.Cover.Split("/").Last();
                }

                if(await grpcClient.PostGameAsync(ProtoBuilder.GameModel(gameToPublish)) is GameModel response)
                {
                    Console.WriteLine("Se ha publicado el juego: " + gameToPublish.Title + ".");
                }
            }
            catch (RpcException)
            {
                Console.WriteLine("No se ha podido conectar al servidor.");
            }
            catch (Exception)
            {
                Console.WriteLine("No se ha podido publicar el juego.");
            }

        }

        public async Task InsertReviewAsync()
        {
            Game selectedGame = DialogUtils.SelectGame(await GetGamesAsync());

            if (selectedGame == null)
            {
                return;
            }

            Review selectedGameReview = DialogUtils.InputReview();

            if (selectedGameReview == null)
            {
                return;
            }
            try
            {
                selectedGame.AddReview(selectedGameReview);
                await grpcClient.PostReviewAsync(ProtoBuilder.GameModel(selectedGame));
                Console.WriteLine("Se ha publicado la calificación del juego " + selectedGame.Title + ".");
            }
            catch (RpcException)
            {
                Console.WriteLine("No se ha podido conectar al servidor.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public async Task InsertUserAsync()
        {
            try
            {
                User userToInsert = DialogUtils.InputUser(await GetUsersAsync());
                userToInsert.Login = false;
                if (userToInsert == null)
                {
                    Console.WriteLine("No se puede insertar este usuario.");
                    return;
                }
                else
                {
                    await grpcClient.PostUserAsync(ProtoBuilder.UserModel(userToInsert));
                    Console.WriteLine("Se ha insertado el usuario: " + userToInsert.Name + ".");
                }
            }
            catch (RpcException)
            {
                Console.WriteLine("No se ha podido conectar al servidor.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public async Task ModifyUserAsync()
        {
            try
            {
                var users = await GetUsersAsync();
                User userToModify = DialogUtils.SelectUser(users);

                if (userToModify == null)
                {
                    Console.WriteLine("Retorno al menú.");
                    return;
                }
                else if (userToModify.Login == true)
                {
                    Console.WriteLine("No se puede modificar un usuario con sesión abierta.");
                    return;
                }

                Console.WriteLine("Ingrese el nuevo nombre de usuario:");
                User modifiedUser = DialogUtils.InputUser(users);

                if (modifiedUser == null)
                {
                    Console.WriteLine("Retorno al menú.");
                    return;
                }

                await grpcClient.UpdateUserAsync(ProtoBuilder.UsersModel(new List<User> { userToModify, modifiedUser }));
                Console.WriteLine("Se ha modificado el usuario: " + modifiedUser.Name + ".");
            }
            catch (RpcException)
            {
                Console.WriteLine("No se ha podido conectar al servidor.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public async Task DeleteUserAsync()
        {
            try
            {
                User userToDelete = DialogUtils.SelectUser(await GetUsersAsync());
                if (userToDelete == null)
                {
                    Console.WriteLine("Retorno al menú.");
                    return;
                }
                else if (userToDelete.Login == true)
                {
                    Console.WriteLine("No se puede modificar un usuario con sesión abierta.");
                    return;
                }

                await grpcClient.DeleteUserAsync(ProtoBuilder.UserModel(userToDelete));
                Console.WriteLine("Se ha eliminado el usuario: " + userToDelete.Name + ".");
            }
            catch (RpcException)
            {
                Console.WriteLine("No se ha podido conectar al servidor.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}