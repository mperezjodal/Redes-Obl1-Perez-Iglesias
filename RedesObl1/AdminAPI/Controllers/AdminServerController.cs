
using Microsoft.AspNetCore.Mvc;
using ServiceAdapter;
using GRPCLibrary;
using System.Threading.Tasks;
using Domain;

namespace AdminAPI.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminServerController : ControllerBase
    {
        private IAdapter adapter;
        public AdminServerController(IAdapter adapter)
        {
            this.adapter = adapter;
        }

        [HttpPut]
        [Route("game/{gameToModify}")]
        public async Task<ActionResult<GamesModel>> PutGame([FromBody] Game game, [FromRoute] string gameToModify)
        {
            return Ok(await adapter.UpdateGameAsync(game, gameToModify));
        }

        [HttpPost]
        [Route("game")]
        public async Task<ActionResult<GamesModel>> PostGame([FromBody] Game game)
        {
            return Ok(await adapter.PostGameAsync(game));
        }

        [HttpDelete]
        [Route("game")]
        public async Task<ActionResult<GameModel>> DeleteGame([FromBody] Game game)
        {
            return Ok(await adapter.DeleteGameAsync(game));
        }

        [HttpPut]
        [Route("user/{userToModify}")]
        public async Task<ActionResult<UserModel>> PutUser([FromBody] User user, [FromRoute] string userToModify)
        {
            return Ok(await adapter.ModifyUserAsync(user, userToModify));
        }

        [HttpPost]
        [Route("user")]
        public async Task<ActionResult<GamesModel>> PostUser([FromBody] string user)
        {
            return Ok(await adapter.PostUserAsync(user));
        }

        [HttpDelete]
        [Route("user")]
        public async Task<ActionResult<GameModel>> DeleteUser([FromBody] User user)
        {
            return Ok(await adapter.DeleteUserAsync(user));
        }

        [HttpPost]
        [Route("user/{user}")]
        public async Task<ActionResult<GamesModel>> AdquireGame([FromBody] Game game, [FromRoute] string user)
        {
            return Ok(await adapter.AdquireGameAsync(game, user));
        }


    }
}
