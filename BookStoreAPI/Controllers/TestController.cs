using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [Authorize("AdminPolicy")]
        [HttpGet]
        public string Get()
        {
            return "It's working for admins.";
        }

        [Authorize("UserPolicy")]
        [HttpGet("GetAgain")]
        public string GetAgain()
        {
            return "It's working for users.";
        }
    }
}
