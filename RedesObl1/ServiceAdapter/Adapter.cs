using System;
using GRPCLibrary;
using Grpc.Core;
using Grpc.Net.Client;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using Domain;
using System.Collections.Generic;
using System.Linq;


namespace ServiceAdapter
{
    public class Adapter : IAdapter
    {
        public GameSystemModel.GameSystemModelClient grpcClient;

        public Adapter(){
            var httpHandler = new HttpClientHandler();

            httpHandler.ServerCertificateCustomValidationCallback = 
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            var channel = GrpcChannel.ForAddress("http://localhost:5001",
                new GrpcChannelOptions { HttpHandler = httpHandler });
            grpcClient = new GameSystemModel.GameSystemModelClient(channel);
        }
        public async Task<UserModel> PostUserAsync(string user)
        {
            try
            {
                return await grpcClient.LoginAsync(new UserModel { Name = user });
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public  async Task<UserModel> ModifyUserAsync(User modifiedUser, string userToModify)
        {
            try
            {
                return await grpcClient.UpdateUserWithNameAsync(ProtoBuilder.UserModifyModel(modifiedUser, userToModify));
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<UserModel> DeleteUserAsync(User userToDelete)
        {
            try
            {
                return await grpcClient.DeleteUserAsync(ProtoBuilder.UserModel(userToDelete));
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<GameModel> DeleteGameAsync(Game deletedGame)
        {
            try
            {
                return await grpcClient.DeleteGameAsync(ProtoBuilder.GameModel(deletedGame));
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<GameModel> PostGameAsync(Game newGame)
        {
            try
            {
                return await grpcClient.PostGameAsync(ProtoBuilder.GameModel(newGame));
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<GameModel> UpdateGameAsync(Game modifiedGame, string gameToModify)
        {
            try
            {
                return await grpcClient.UpdateGameWithTitleAsync(ProtoBuilder.GameModifyModel(modifiedGame, gameToModify));
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<GameModel> AdquireGameAsync(Game deletedGame, string user)
        {
            try
            {
                return await grpcClient.AcquireGameAsync(ProtoBuilder.GameModel(deletedGame, user));
            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<GameModel> RemoveAcquireGameAsync(Game deletedGame, string user)
        {
            try
            {
                return await grpcClient.RemoveAcquireGameAsync(ProtoBuilder.GameModel(deletedGame, user));
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
