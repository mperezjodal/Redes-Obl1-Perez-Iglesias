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

        public async Task ShowGames()
        {
            Game gameToShow = DialogUtils.SelectGame(await GetGames());

            if (gameToShow != null && !String.IsNullOrEmpty(gameToShow.Cover))
            {
                await ReceiveGameCover(gameToShow);
            }

            DialogUtils.ShowGameDetail(gameToShow);
        }

        public async Task ReceiveGameCover(Game game)
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
        public async Task<List<Game>> GetGames()
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

        public async Task<List<User>> GetUsers()
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

        public async Task InsertGame()
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

        public async Task InsertReview()
        {
            Game selectedGame = DialogUtils.SelectGame(await GetGames());

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
                Console.WriteLine("Se ha publicado la calificaci??n del juego " + selectedGame.Title + ".");
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

        public async Task InsertUser()
        {
            try
            {
                User userToInsert = DialogUtils.InputUser(await GetUsers());
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

        public async Task ModifyUser()
        {
            try
            {
                var users = await GetUsers();
                User userToModify = DialogUtils.SelectUser(users);

                if (userToModify == null)
                {
                    Console.WriteLine("Retorno al men??.");
                    return;
                }
                else if (userToModify.Login == true)
                {
                    Console.WriteLine("No se puede modificar un usuario con sesi??n abierta.");
                    return;
                }

                Console.WriteLine("Ingrese el nuevo nombre de usuario:");
                User modifiedUser = DialogUtils.InputUser(users);

                if (modifiedUser == null)
                {
                    Console.WriteLine("Retorno al men??.");
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

        public async Task DeleteUser()
        {
            try
            {
                User userToDelete = DialogUtils.SelectUser(await GetUsers());
                if (userToDelete == null)
                {
                    Console.WriteLine("Retorno al men??.");
                    return;
                }
                else if (userToDelete.Login == true)
                {
                    Console.WriteLine("No se puede modificar un usuario con sesi??n abierta.");
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