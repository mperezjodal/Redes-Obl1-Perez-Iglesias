using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ServerLogs.Controllers
{
    [ApiController]
    [Route("api/logs")]
    public class ServerLogsController : ControllerBase
    {
        private IServerRabbitMq serverLogs;

        public ServerLogsController(IServerRabbitMq serverLogs)
        {
            this.serverLogs = serverLogs;
        }

        [HttpGet]
        public IActionResult Get()
        {
            Console.WriteLine("1");
            return Ok(serverLogs.Log());
        }
    }
}
