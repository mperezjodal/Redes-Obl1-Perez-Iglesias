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
        public IActionResult Get(string juego = "", string usuario = "", string fechaDesde = "", string fechaHasta = "")
        {
            FilterParams queryParams = new FilterParams()
            {
                GameTitle = juego,
                Username = usuario
            };
            try 
            {
                queryParams.DateFrom = DateTime.Parse(fechaDesde);
                queryParams.DateTo = DateTime.Parse(fechaHasta);
            }
            catch (Exception) {}

            return Ok(serverLogs.Log(queryParams));
        }
    }
}
