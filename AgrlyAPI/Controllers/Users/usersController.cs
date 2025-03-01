using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgrlyAPI.Controllers.users
{
    [Route("api/[controller]")]
    [ApiController]
    public class usersController : ControllerBase
    {
		[HttpGet]
		public IActionResult Get()
		{
			return Ok( "Hello, World!" );
		}
	}
}
