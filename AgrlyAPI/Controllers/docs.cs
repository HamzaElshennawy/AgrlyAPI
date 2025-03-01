using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgrlyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class docs : ControllerBase
    {
		[HttpGet]
		public IActionResult Get()
		{
			return Redirect( "/swagger" );
		}
	}
}
