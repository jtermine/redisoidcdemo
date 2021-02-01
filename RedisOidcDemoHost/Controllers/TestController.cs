using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace RedisOidcDemoHost.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class TestController : Controller
    {
        private readonly ILogger _logger;

        public TestController(ILogger logger)
        {
            _logger = logger;
        }

        [HttpGet(""), Authorize(JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult GetHome()
        {
            var name = User.Claims.FirstOrDefault(f =>
                f.Type.Equals("name",
                    StringComparison.InvariantCultureIgnoreCase));
            
            _logger.Verbose("Test invoked");
            
            var returnObject = new
                {objectName = "Test", objectType = 2, name = name?.Value};
            
            return Ok(returnObject);
        }
    }
}