using System.Threading.Tasks;
using Domain;
using Grpc.Core;
using GRPCLibrary;
using Microsoft.Extensions.Logging;

namespace ServerAdmin
{
    public class UserServerAdmin : UsersService.UsersServiceBase
    {
        // private readonly ILogger<UsersService> _logger;
        private GameSystem GameSystem;
        public UserServerAdmin()
        {
            GameSystem = new GameSystem();
        }

        public override Task<UserModel> CreateNewUser(UserModel request, ServerCallContext context)
        { 
            User existingUser = this.GameSystem.Users.Find(u => u.Name.Equals(request.Name));
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
    }
}