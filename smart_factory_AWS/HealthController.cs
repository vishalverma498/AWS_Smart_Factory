using System;
using Microsoft.AspNetCore.Mvc;

namespace smart_factory_AWS
{
    [Route("[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public HealthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var healthStatus = new
            {
                loading = "healthy",
                filling = "atRisk",
                dispatching = "warning"
            };

            return Ok(healthStatus);
        }
    }
}

