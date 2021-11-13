using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Domain;
using Grpc.Core;
using GRPCLibrary;
using Microsoft.Extensions.Logging;

namespace ServerAdmin
{
    public class GameSystemManager : GameSystemService.GameSystemServiceBase
    {
        private static IGameSystem GameSystem;
        public object lockAddGame = new object();
        public object lockModifyGame = new object();
        public object lockDeleteGame = new object();

        public GameSystemManager(IGameSystem gameSystem)
        {
            GameSystem = gameSystem;
        }

        public override Task<UserModel> Login(UserModel request, ServerCallContext context)
        {
            User existingUser = GameSystem.Users.Find(u => u.Name.Equals(request.Name));
            if (existingUser != null && existingUser.Login)
            {
                return Task.FromException<UserModel>(new RpcException(new Status(StatusCode.AlreadyExists, "User already exists")));
            }

            if (existingUser == null)
            {
                User newUser = GameSystem.AddUser(request.Name);
            }

            GameSystem.LoginUser(request.Name);
            return Task.FromResult(request);
        }

        public override Task<UserModel> Logout(UserModel request, ServerCallContext context)
        {
            GameSystem.LogoutUser(request.Name);
            return Task.FromResult(request);
        }

        public override Task<GameModel> PostGame(GameModel request, ServerCallContext context)
        {
            lock (lockAddGame)
            {
                GameSystem.AddGame(ProtoBuilder.Game(request));
            }

            return Task.FromResult(request);
        }

        public override Task<GameCoverModel> PostGameCover(GameCoverModel request, ServerCallContext context)
        {
            Game game = GameSystem.Games.Find(g => g.Id == request.Id);
            GameSystem.AddGameCover(game, request.Cover);

            return Task.FromResult(request);
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
                return Task.FromException<GameModel>(new RpcException(new Status(StatusCode.AlreadyExists, "Game is being modified")));
            }

            GameSystem.UpdateReviews(gameToModify, ProtoBuilder.GetReviews(request));
            return Task.FromResult(request);
        }

        public override Task<GameModel> ToModify(GameModel request, ServerCallContext context)
        {
            var gameToModify = GameSystem.Games.Find(g => g.Title.Equals(request.Title));
            if (GameSystem.IsGameBeingModified(gameToModify))
            {
                return Task.FromException<GameModel>(new RpcException(new Status(StatusCode.AlreadyExists, "Game is being modified")));
            }

            GameSystem.AddGameBeingModified(gameToModify, request.User);
            return Task.FromResult(request);
        }

        public override Task<GameModel> UpdateGame(GamesModel request, ServerCallContext context)
        {
            var gameToModify = GameSystem.Games.Find(g => g.Title.Equals(request.Games[0].Title));

            lock (lockModifyGame)
            {
                if (GameSystem.IsGameBeingModifiedByAnother(gameToModify, request.Games[0].User))
                {
                    return Task.FromException<GameModel>(new RpcException(new Status(StatusCode.AlreadyExists, "Game is being modified")));
                }
                GameSystem.DeleteGameBeingModified(gameToModify);
                GameSystem.UpdateGame(gameToModify, ProtoBuilder.Game(request.Games[1]));
            }

            return Task.FromResult(ProtoBuilder.GameModel(gameToModify));
        }

        public override Task<GameModel> DeleteGame(GameModel request, ServerCallContext context)
        {
            var gameToDelete = GameSystem.Games.Find(g => g.Title.Equals(request.Title));

            if (GameSystem.IsGameBeingModified(gameToDelete))
            {
                return Task.FromException<GameModel>(new RpcException(new Status(StatusCode.AlreadyExists, "Game is being modified")));
            }
            lock (lockDeleteGame)
            {
                GameSystem.DeleteGame(gameToDelete);
            }

            return Task.FromResult(request);
        }
    }
}