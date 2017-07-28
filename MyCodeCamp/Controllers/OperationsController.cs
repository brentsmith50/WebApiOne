using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCodeCamp.Controllers
{
    [Route("api/[controller]")]
    public class OperationsController : Controller
    {
        private IConfigurationRoot config;
        private ILogger<OperationsController> logger;

        public OperationsController(ILogger<OperationsController> logger, IConfigurationRoot config)
        {
            this.logger = logger;
            this.config = config;
        }

        [HttpOptions("reloadConfig")]
        public IActionResult ReloadConfiguration()
        {
            try
            {
                this.config.Reload();
                return Ok("Configuration reloaded successfully.");
            }
            catch (Exception ex)
            {
                this.logger.LogError($"An Exception was thrown while reloading the configuration: {ex}");
            }
            return BadRequest("Could not reload Configuration.");
        }
    }
}
