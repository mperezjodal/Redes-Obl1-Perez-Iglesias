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

        public Adapter()
        {
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
                return await grpcClient.PostUserAsync(new UserModel { Name = user });
            }
            catch (Exception e) { return null; }
        }

        public async Task<UserModel> UpdateUserAsync(User modifiedUser, string userToModify)
        {
            try
            {
                return await grpcClient.UpdateUserWithNameAsync(ProtoBuilder.UserModifyModel(modifiedUser, userToModify));
            }
            catch (Exception) { return null; }
        }

        public async Task<UserModel> DeleteUserAsync(string username)
        {
            try
            {
                return await grpcClient.DeleteUserAsync(new UserModel { Name = username });
            }
            catch (Exception) { return null; }
        }

        public async Task<GameModel> DeleteGameAsync(string title)
        {
            try
            {
                return await grpcClient.DeleteGameAsync(new GameModel { Title = title });
            }
            catch (Exception) { return null; }
        }

        public async Task<GameModel> PostGameAsync(Game newGame)
        {
            try
            {
                return await grpcClient.PostGameAsync(ProtoBuilder.GameModel(newGame));
            }
            catch (Exception) { return null; }
        }

        public async Task<GameModel> UpdateGameAsync(Game modifiedGame, string gameToModify)
        {
            try
            {
                return await grpcClient.UpdateGameWithTitleAsync(ProtoBuilder.GameModifyModel(modifiedGame, gameToModify));
            }
            catch (Exception) { return null; }
        }

        public async Task<GameModel> AdquireGameAsync(string gameTitle, string username)
        {
            try
            {
                return await grpcClient.AcquireGameAsync(new GameModel() { Title = gameTitle, User = username });
            }
            catch (Exception) { return null; }
        }

        public async Task<GameModel> RemoveAcquireGameAsync(string gameTitle, string username)
        {
            try
            {
                return await grpcClient.RemoveAcquireGameAsync(new GameModel() { Title = gameTitle, User = username });
            }
            catch (Exception) { return null; }
        }
    }
}
