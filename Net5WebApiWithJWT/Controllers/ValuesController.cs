using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Net5WebApiWithJWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private static readonly string[] weekdays = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        private readonly ILogger<ValuesController> _logger;
     

        public ValuesController(ILogger<ValuesController> logger)
        {
            _logger = logger;
        }
        [Authorize(Roles ="Admin")]
        [HttpGet]
        [Route("[action]")]
        public IActionResult Get()
        {
            return Ok(weekdays);
        }
        [HttpGet]
        [Route("[action]")]
        public IActionResult GetValues()
        {
            return Ok(weekdays);
        }
    }
}
