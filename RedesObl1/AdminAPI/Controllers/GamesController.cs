
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
            GameModel response = await adapter.UpdateGameAsync(game, gameToModify);
            if(response == null)
            {
                return BadRequest();
            }

            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<GamesModel>> PostGame([FromBody] Game game)
        {
            GameModel response = await adapter.PostGameAsync(game);
            if(response == null)
            {
                return BadRequest();
            }
            
            return new CreatedResult(string.Empty, response);
        }

        [HttpDelete("{title}")]
        public async Task<ActionResult<GameModel>> DeleteGame([FromRoute] string title)
        {
            GameModel response = await adapter.DeleteGameAsync(title);
            if(response == null)
            {
                return BadRequest();
            }

            return Ok(response);
        }
    }
}
