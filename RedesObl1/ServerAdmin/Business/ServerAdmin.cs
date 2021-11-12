using System;
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
        
    }
}