
using Microsoft.AspNetCore.Mvc;
using ServiceAdapter;
using GRPCLibrary;
using System.Threading.Tasks;

namespace AdminAPI.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class UsersController : ControllerBase
    {
        private IAdapter adapter;
        public UsersController(IAdapter adapter)
        {
            this.adapter = adapter;
        }

        [HttpGet]
        public async Task<ActionResult<GamesModel>> Get()
        {
            GamesModel value = await adapter.GetGamesAsync();
            return Ok(value);
        }
    }
}
