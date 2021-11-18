using System;
using System.Threading.Tasks;
using GRPCLibrary;
using Domain;

namespace ServiceAdapter
{
    public interface IAdapter
    {
        public Task<UserModel> PostUserAsync(string user);
        public Task<UserModel> ModifyUserAsync(User modifiedUser, string userToModify);
        public Task<UserModel> DeleteUserAsync(User userToDelete);
        public Task<GameModel> DeleteGameAsync(Game deletedGame);
        public Task<GameModel> PostGameAsync(Game newGame);
        public Task<GameModel> UpdateGameAsync(Game modifiedGame, string gameToModify);
        public Task<GameModel> AdquireGameAsync(Game deletedGame, string user);
        public Task<UserModel> DeleteGameOfUserAsync(Game deletedGame, User user);
    }
}
