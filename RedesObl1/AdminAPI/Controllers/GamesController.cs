
using System.Runtime.ConstrainedExecution;
using Microsoft.AspNetCore.Mvc;
using ServiceAdapter;
using GRPCLibrary;
using System.Threading.Tasks;
using Domain;
using System;

namespace AdminAPI.Controllers
{
    [ApiController]
    [Route("api/games")]
    public class GamesController : ControllerBase
    {
        private IAdapter adapter;
        public GamesController(IAdapter adapter)
        {
            this.adapter = adapter;
        }

        [HttpPut("{gameToModify}")]
        public async Task<ActionResult<GamesModel>> PutGame([FromBody] Game game, [FromRoute] string gameToModify)
        {
            return Ok(await adapter.UpdateGameAsync(game, gameToModify));
        }

        [HttpPost]
        public async Task<ActionResult<GamesModel>> PostGame([FromBody] Game game)
        {
            return Ok(await adapter.PostGameAsync(game));
        }

        [HttpDelete("{title}")]
        public async Task<ActionResult<GameModel>> DeleteGame([FromRoute] string title)
        {
            return Ok(await adapter.DeleteGameAsync(title));
        }
    }
}
