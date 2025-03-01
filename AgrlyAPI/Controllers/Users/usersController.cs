using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AgrlyAPI.Models.User;

namespace AgrlyAPI.Controllers.users
{
    [Route("api/[controller]")]
    [ApiController]
    public class usersController : ControllerBase
    {

		public User user = new User();


		[HttpGet]
		public IActionResult Get()
		{
			return Ok( user );
		}
	}
}
