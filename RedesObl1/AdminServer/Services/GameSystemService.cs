using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Domain;
using Grpc.Core;
using GRPCLibrary;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using FileStreamLibrary;
using System.IO;
using Google.Protobuf;
using System.Threading;

namespace AdminServer
{
    public class GameSystemService : GameSystemModel.GameSystemModelBase
    {
        private static IGameSystem GameSystem;
        public SemaphoreSlim semaphoreAddGame = new SemaphoreSlim(1, 1);
        public SemaphoreSlim semaphoreModifyGame = new SemaphoreSlim(1, 1);
        public SemaphoreSlim semaphoreDeleteGame = new SemaphoreSlim(1, 1);
        private const string SimpleQueue = "m6bBasicQueue";
        private const string HostName = "localhost";
        private static ConnectionFactory _factory;
        private static IConnection _connection;
        private static IModel _channel;

        public GameSystemService(IGameSystem gameSystem)
        {
            GameSystem = gameSystem;
            _factory = new ConnectionFactory 
            { 
                HostName = HostName, 
            };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            DeclareQueue(_channel);
        }

        public override Task<UserModel> Login(UserModel request, ServerCallContext context)
        {
            User existingUser = GameSystem.Users.Find(u => u.Name.Equals(request.Name));
            if (existingUser != null && existingUser.Login)
            {
                return Task.FromException<UserModel>(new AlreadyExistsException("Usuario ya tiene una sesión abierta"));
            }
            if (existingUser == null)
            {
                User newUser = GameSystem.AddUser(request.Name);
                WriteInLog(null, "Insert User", newUser);
            }

            GameSystem.LoginUser(request.Name);
            return Task.FromResult(request);
        }

        public override Task<UserModel> PostUser(UserModel request, ServerCallContext context)
        {
            User existingUser = GameSystem.Users.Find(u => u.Name.Equals(request.Name));
            if (existingUser != null)
            {
                return Task.FromException<UserModel>(new AlreadyExistsException("Usuario ya existe"));
            }

            User user = GameSystem.AddUser(request.Name);
            
            WriteInLog(null, "Insert User", user);
            return Task.FromResult(request);
        }

        public override Task<UserModel> UpdateUser(UsersModel request, ServerCallContext context)
        {
            User existingUser = GameSystem.Users.Find(u => u.Name.Equals(request.Users[0].Name));
            if (existingUser == null)
            {
                return Task.FromException<UserModel>(new NotFoundException("Usuario no encontrado"));
            }
            if (existingUser.Login)
            {
                return Task.FromException<UserModel>(new NotFoundException("Usuario ya tiene una sesión abierta"));
            }

            User updatedUser = GameSystem.UpdateUser(ProtoBuilder.User(request.Users[0]), ProtoBuilder.User(request.Users[1]));
            WriteInLog(null, "Modify User", updatedUser);
            return Task.FromResult(request.Users[1]);
        }

        public override Task<UserModel> UpdateUserWithName(UserModifyModel request, ServerCallContext context)
        {
            User existingUser = GameSystem.Users.Find(u => u.Name.Equals(request.NameUserToModify));
            if (existingUser == null)
            {
                return Task.FromException<UserModel>(new RpcException(new Status(StatusCode.NotFound, "Usuario no encontrado")));
            }
            if (existingUser.Login)
            {
                return Task.FromException<UserModel>(new NotFoundException("Usuario ya tiene una sesión abierta"));
            }

            User updatedUser = GameSystem.UpdateUser(existingUser, ProtoBuilder.User(request));
            WriteInLog(null, "Modify User", updatedUser);
            return Task.FromResult(ProtoBuilder.UserModel(updatedUser));
        }

        public override Task<UserModel> DeleteUser(UserModel request, ServerCallContext context)
        {
            User existingUser = GameSystem.Users.Find(u => u.Name.Equals(request.Name));
            if (existingUser == null)
            {
                return Task.FromException<UserModel>(new NotFoundException("Usuario no encontrado"));
            }
            if (existingUser.Login)
            {
                return Task.FromException<UserModel>(new NotFoundException("Usuario ya tiene una sesión abierta"));
            }

            GameSystem.Users.RemoveAll(u => u.Name.Equals(existingUser.Name));
            WriteInLog(null, "Deleted User", existingUser);
            return Task.FromResult(request);
        }

        public override Task<UserModel> Logout(UserModel request, ServerCallContext context)
        {
            User user = GameSystem.LogoutUser(request.Name);
            WriteInLog(null, "Logout", user);
            return Task.FromResult(request);
        }

        public async override Task<GameModel> PostGame(GameModel request, ServerCallContext context)
        {
            await semaphoreAddGame.WaitAsync();
            try
            {
                GameSystem.AddGame(ProtoBuilder.Game(request));
                WriteInLog(ProtoBuilder.Game(request), "Post Game" + request.User, null);
            }
            finally
            {
                semaphoreAddGame.Release();
            }

            return request;
        }

        public async override Task<CoverModel> PostCover(CoverModel request, ServerCallContext context)
        {
            FileStreamHandler _fileStreamHandler = new FileStreamHandler();
            await _fileStreamHandler.WriteDataAsync(request.FileName, request.Data.ToByteArray());

            return request;
        }

        public override Task<CoverSize> GetCoverSize(CoverRequest request, ServerCallContext context)
        {
            var fileInfo = new FileInfo(request.Cover);
            long fileLength = fileInfo.Length;
            byte[] fileSizeDataLength = BitConverter.GetBytes(fileLength);
            long fileSize = BitConverter.ToInt64(fileSizeDataLength);

            return Task.FromResult(new CoverSize { Size = fileSize });
        }

        public async override Task<CoverModel> GetCover(CoverRequest request, ServerCallContext context)
        {
            return new CoverModel { 
                FileName = request.Cover, 
                Data = ByteString.CopyFrom(await FileCommunicationHandler.GetFilePartBytes(request.Cover, request.Part, request.Offset)) 
            };
        }

        public override Task<GamesModel> GetGames(EmptyRequest request, ServerCallContext context)
        {
            return Task.FromResult(ProtoBuilder.GamesModel(GameSystem.Games));
        }

        public override Task<UsersModel> GetUsers(EmptyRequest request, ServerCallContext context)
        {
            return Task.FromResult(ProtoBuilder.UsersModel(GameSystem.Users));
        }

        public override Task<GameModel> PostReview(GameModel request, ServerCallContext context)
        {
            var gameToModify = GameSystem.Games.Find(g => g.Title.Equals(request.Title));
            if (GameSystem.IsGameBeingModified(gameToModify))
            {
                return Task.FromException<GameModel>(new AlreadyModifyingException("El juego esta siendo modificado"));
            }

            GameSystem.UpdateReviews(gameToModify, ProtoBuilder.GetReviews(request));
            WriteInLog(gameToModify, "Post Review by: " + request.User);
            return Task.FromResult(request);
        }

        public override Task<GameModel> ToModify(GameModel request, ServerCallContext context)
        {
            var gameToModify = GameSystem.Games.Find(g => g.Title.Equals(request.Title));
            if (GameSystem.IsGameBeingModified(gameToModify))
            {
                return Task.FromException<GameModel>(new AlreadyModifyingException("El juego esta siendo modificado"));
            }

            GameSystem.AddGameBeingModified(gameToModify, request.User);
            WriteInLog(gameToModify, "Added Game to be modified by: " + request.User);
            return Task.FromResult(request);
        }

        public async override Task<GameModel> UpdateGame(GamesModel request, ServerCallContext context)
        {
            var gameToModify = GameSystem.Games.Find(g => g.Title.Equals(request.Games[0].Title));
            
            await semaphoreModifyGame.WaitAsync();
            try
            {
                if (GameSystem.IsGameBeingModifiedByAnother(gameToModify, request.Games[0].User))
                {
                    return new GameModel { Code = 4 };
                }
                GameSystem.DeleteGameBeingModified(gameToModify);
                GameSystem.UpdateGame(gameToModify, ProtoBuilder.Game(request.Games[1]));
                WriteInLog(gameToModify, "Update Game by: " + request.Games[1].User);
            }
            finally
            {
                semaphoreModifyGame.Release();
            }

            return ProtoBuilder.GameModel(gameToModify);
        }

        public async override Task<GameModel> UpdateGameWithTitle(GameModifyModel request, ServerCallContext context)
        {
            var gameToModify = GameSystem.Games.Find(g => g.Title.Equals(request.TitleGameToModify));
            this.ToModify(ProtoBuilder.GameModel(gameToModify), context);
            await semaphoreModifyGame.WaitAsync();
            try
            {
                if (GameSystem.IsGameBeingModifiedByAnother(gameToModify, request.User))
                {
                    return new GameModel { Code = 4 };
                }
                GameSystem.DeleteGameBeingModified(gameToModify);
                GameSystem.UpdateGame(gameToModify, ProtoBuilder.Game(request));
                WriteInLog(gameToModify, "Update Game by: " + request.User);
            }
            finally
            {
                semaphoreModifyGame.Release();
            }

            return ProtoBuilder.GameModel(request);
        }

        public async override Task<GameModel> DeleteGame(GameModel request, ServerCallContext context)
        {
            var gameToDelete = GameSystem.Games.Find(g => g.Title.Equals(request.Title));

            if (GameSystem.IsGameBeingModified(gameToDelete))
            {
                return new GameModel { Code = 4 };
            }
            await semaphoreDeleteGame.WaitAsync();
            try
            {
                GameSystem.DeleteGame(gameToDelete);
                WriteInLog(gameToDelete, "Delete Game by: " + request.User);
            }
            finally
            {
                semaphoreDeleteGame.Release();
            }

            return request;
        }

        public override Task<GameModel> AcquireGame(GameModel request, ServerCallContext context)
        {
            var user = GameSystem.Users.Find(u => u.Name.Equals(request.User));
            var game = GameSystem.Games.Find(g => g.Title.Equals(request.Title));
            user.AcquireGame(game);
            WriteInLog(game, "Adquire Game", user);

            return Task.FromResult(request);
        }

        public override Task<GameModel> RemoveAcquireGame(GameModel request, ServerCallContext context)
        {
            var user = GameSystem.Users.Find(u => u.Name.Equals(request.User));
            var game = GameSystem.Games.Find(g => g.Title.Equals(request.Title));
            user.Games.Remove(game);
            WriteInLog(game, "Remove Acquire Game", user);
            
            return Task.FromResult(request);
        }

        public override Task<GamesModel> GetAcquiredGames(UserModel request, ServerCallContext context)
        {
            var user = GameSystem.Users.Find(u => u.Name.Equals(request.Name));
            return Task.FromResult(ProtoBuilder.GamesModel(user.Games));
        }

        private static void WriteInLog(Game game = null, string action = null, User user = null)
        {
            LogEntry logEntry = new LogEntry() {User = user, Game = game, Date = DateTime.Now, Action = action};
            PublishMessage(logEntry.Encode());
        }

        private static void DeclareQueue(IModel channel)
        {
            channel.QueueDeclare(
                queue: SimpleQueue,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        public static void PublishMessage(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(
                exchange: string.Empty,
                routingKey: SimpleQueue,
                body: data
            );
        }
    }
}