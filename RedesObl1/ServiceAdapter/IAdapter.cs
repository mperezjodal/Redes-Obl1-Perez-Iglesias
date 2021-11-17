using System;
using System.Threading.Tasks;
using GRPCLibrary;

namespace ServiceAdapter
{
    public interface IAdapter
    {
        public Task<GamesModel> GetGamesAsync();
    }
}
