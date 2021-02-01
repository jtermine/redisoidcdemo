using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace RedisOidcDemoHost.Controllers
{
    [ApiController, Route("")]
    public class HomeController : Controller
    {
        private readonly ILogger _logger;

        public HomeController(ILogger logger)
        {
            _logger = logger;
        }

        [HttpGet(""), Authorize(OpenIdConnectDefaults.AuthenticationScheme)]
        public async Task<ActionResult> GetHome()
        {
            _logger.Verbose("GetHome invoked");

            var idToken = await HttpContext.GetTokenAsync(OpenIdConnectDefaults.AuthenticationScheme, "id_token");
            var accessToken = await HttpContext.GetTokenAsync(OpenIdConnectDefaults.AuthenticationScheme, "access_token");

            var returnObject = new
                {objectName = "Home", objectType = 2, id_token = idToken, access_token = accessToken};
            
            return Ok(returnObject);
        }
    }
}