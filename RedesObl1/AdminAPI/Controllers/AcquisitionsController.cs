
using Microsoft.AspNetCore.Mvc;
using ServiceAdapter;
using GRPCLibrary;
using System.Threading.Tasks;
using Domain;

namespace AdminAPI.Controllers
{
    [ApiController]
    [Route("api/acquisitions")]
    public class AcquisitionsController : ControllerBase
    {
        private IAdapter adapter;
        public AcquisitionsController(IAdapter adapter)
        {
            this.adapter = adapter;
        }

        [HttpPost("{user}")]
        public async Task<ActionResult<GamesModel>> AdquireGame([FromBody] string gameTitle, [FromRoute] string user)
        {
            return Ok(await adapter.AdquireGameAsync(gameTitle, user));
        }

        [HttpDelete("{user}")]
        public async Task<ActionResult<GamesModel>> DeleteAdquireGame([FromBody] string gameTitle, [FromRoute] string user)
        {
            return Ok(await adapter.RemoveAcquireGameAsync(gameTitle, user));
        }
    }
}
