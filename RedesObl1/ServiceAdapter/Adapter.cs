using System;
using GRPCLibrary;
using Grpc.Core;
using Grpc.Net.Client;
using System.Threading.Tasks;

namespace ServiceAdapter
{
    public class Adapter : IAdapter
    {
        private static GameSystemModel.GameSystemModelClient grpcClient;

        public Adapter(){
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            // The port number(5001) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            grpcClient = new GameSystemModel.GameSystemModelClient(channel);
        }
        public async Task<GamesModel> GetGamesAsync()
        {
            var response = await grpcClient.GetGamesAsync(new EmptyRequest());
            Console.WriteLine("GetGames: ");
            return response;
        }
    }
}
