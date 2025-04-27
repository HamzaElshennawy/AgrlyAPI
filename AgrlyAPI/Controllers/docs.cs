#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
#pragma warning disable IDE1006 // Naming Styles
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgrlyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

	public class docs : ControllerBase
	{
		[HttpGet]
		public IActionResult Get() => Redirect( "http://localhost:5258/swagger" );
		
	}
}
