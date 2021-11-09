using System.Threading.Tasks;
using Grpc.Core;
using GRPCLibrary;

namespace ServerAdmin
{
    public class UserServerAdmin : UsersService.UsersServiceBase
    {
        public override Task<UserModel> CreateNewUser(UserModel request, ServerCallContext context)
        { 
            return Task.FromResult(request);
        }
    }
}