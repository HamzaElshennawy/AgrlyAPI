#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
#pragma warning disable IDE1006 // Naming Styles
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AgrlyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
	[EnableRateLimiting("fixed")]
	public class docs : ControllerBase
	{
		private readonly string _baseURL = "http://agrly.runasp.net";
		[HttpGet]
		public IActionResult Get() => Redirect( $"{_baseURL}/swagger" );
		
	}
}
