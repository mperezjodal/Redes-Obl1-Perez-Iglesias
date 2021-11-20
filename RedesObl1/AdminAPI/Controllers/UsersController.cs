
using Microsoft.AspNetCore.Mvc;
using ServiceAdapter;
using GRPCLibrary;
using System.Threading.Tasks;
using Domain;

namespace AdminAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private IAdapter adapter;
        public UsersController(IAdapter adapter)
        {
            this.adapter = adapter;
        }

        [HttpPut("{userToModify}")]
        public async Task<ActionResult<UserModel>> PutUser([FromBody] User user, [FromRoute] string userToModify)
        {
            return Ok(await adapter.UpdateUserAsync(user, userToModify));
        }

        [HttpPost]
        public async Task<ActionResult<GamesModel>> PostUser([FromBody] string user)
        {
            return new CreatedResult("", await adapter.PostUserAsync(user));
        }

        [HttpDelete("{username}")]
        public async Task<ActionResult<GameModel>> DeleteUser([FromRoute] string username)
        {
            return Ok(await adapter.DeleteUserAsync(username));
        }
    }
}
