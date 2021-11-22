using System;
using System.Threading.Tasks;
using GRPCLibrary;
using Domain;

namespace ServiceAdapter
{
    public interface IAdapter
    {
        public Task<UserModel> PostUserAsync(string user);
        public Task<UserModel> UpdateUserAsync(User modifiedUser, string userToModify);
        public Task<UserModel> DeleteUserAsync(string userToDelete);
        public Task<GameModel> DeleteGameAsync(string deletedGame);
        public Task<GameModel> PostGameAsync(Game newGame);
        public Task<GameModel> UpdateGameAsync(Game modifiedGame, string gameToModify);
        public Task<GameModel> AdquireGameAsync(string game, string user);
        public Task<GameModel> RemoveAcquireGameAsync(string game, string user);
    }
}
