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

        public override Task<UserModel> CreateNewUser(UserModel request, ServerCallContext context)
        { 
            Console.WriteLine(JsonSerializer.Serialize(GameSystem.Users));
            User existingUser = GameSystem.Users.Find(u => u.Name.Equals(request.Name));
            Console.WriteLine(JsonSerializer.Serialize(existingUser));
            if (existingUser != null && existingUser.Login)
            {
                return Task.FromException<UserModel>(new RpcException(new Status(StatusCode.AlreadyExists, "User already exists")));
            }

            if (existingUser == null)
            {
                Console.WriteLine("Creating new user");
                User newUser = GameSystem.AddUser(request.Name);
                Console.WriteLine(JsonSerializer.Serialize(GameSystem.Users));
            }

            GameSystem.LoginUser(request.Name);
            return Task.FromResult(request);
        }
    }
}