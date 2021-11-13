using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DisplayUtils;
using Domain;
using GRPCLibrary;
using System.Text;
using System.Text.Json;
using DisplayUtils;
using Domain;
using RabbitMQ.Client;

namespace Server
{
    public class ServerMenuUtils
    {
        private GameSystemService.GameSystemServiceClient grpcClient;
        private ServerUtils serverUtils;
        private IModel channel;
        private const string SimpleQueue = "m6bBasicQueue";
        // APP CONFIG

        public ServerMenuUtils(IModel channel, GameSystemService.GameSystemServiceClient grpcClient)
        {
            this.grpcClient = grpcClient;
            channel = channel;
        }

        public async Task<List<Game>> GetGames()
        {
            try 
            {
                return ProtoBuilder.Games(await grpcClient.GetGamesAsync(new EmptyRequest()));
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
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new List<User>();
            }
        }

        public async Task<Game> InsertGame()
        {
            Game gameToPublish = DialogUtils.InputGame();
            try
            {
                if (gameToPublish.Cover != null)
                {
                    var fileName = gameToPublish.Cover.Split("/").Last();
                    System.IO.File.Copy(gameToPublish.Cover, Directory.GetCurrentDirectory().ToString() + "/" + fileName);
                    gameToPublish.Cover = fileName;
                }

                await grpcClient.PostGameAsync(ProtoBuilder.GameModel(gameToPublish));
                Console.WriteLine("Se ha publicado el juego: " + gameToPublish.Title + ".");
                return gameToPublish;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

        }
        public void PublishMessage(IModel channel, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(
                exchange: string.Empty,
                routingKey: SimpleQueue,
                body: data
            );
        }

        public async Task<Game> InsertReview()
        {
            Game selectedGame = DialogUtils.SelectGame(await GetGames());

            if (selectedGame == null)
            {
                return null;
            }

            Review selectedGameReview = DialogUtils.InputReview();

            if (selectedGameReview == null)
            {
                return null;
            }
            try
            {
                selectedGame.AddReview(selectedGameReview);
                await grpcClient.PostReviewAsync(ProtoBuilder.GameModel(selectedGame));
                Console.WriteLine("Se ha publicado la calificación del juego " + selectedGame.Title + ".");
                return selectedGame;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public async Task<User> InsertUser()
        {
            try
            {
                User userToInsert = DialogUtils.InputUser(await GetUsers());
                userToInsert.Login = false;
                if (userToInsert == null)
                {
                    Console.WriteLine("No se puede insertar este usuario.");
                    return null;
                }
                else
                {
                    await grpcClient.PostUserAsync(ProtoBuilder.UserModel(userToInsert));
                    Console.WriteLine("Se ha insertado el usuario: " + userToInsert.Name + ".");
                    return userToInsert;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public async Task<User> ModifyUser()
        {
            try
            {
                var users = await GetUsers();
                User userToModify = DialogUtils.SelectUser(users);

                if (userToModify == null)
                {
                    Console.WriteLine("Retorno al menú.");
                    return null;
                }
                else if (userToModify.Login == true)
                {
                    Console.WriteLine("No se puede modificar un usuario con sesión abierta.");
                    return null;
                }

                Console.WriteLine("Ingrese el nuevo nombre de usuario:");
                User modifiedUser = DialogUtils.InputUser(users);

                if (modifiedUser == null)
                {
                    Console.WriteLine("Retorno al menú.");
                    return null;
                }

                await grpcClient.UpdateUserAsync(ProtoBuilder.UsersModel(new List<User> { userToModify, modifiedUser }));
                Console.WriteLine("Se ha modificado el usuario: " + modifiedUser.Name + ".");
                return modifiedUser;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public async Task<User> DeleteUser()
        {
            try
            {
                User userToDelete = DialogUtils.SelectUser(await GetUsers());
                if (userToDelete == null)
                {
                    Console.WriteLine("Retorno al menú.");
                    return null;
                }
                else if (userToDelete.Login == true)
                {
                    Console.WriteLine("No se puede modificar un usuario con sesión abierta.");
                    return null;
                }

                await grpcClient.DeleteUserAsync(ProtoBuilder.UserModel(userToDelete));
                Console.WriteLine("Se ha eliminado el usuario: " + userToDelete.Name + ".");
                return userToDelete;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}