using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgrlyAPI.Controllers.Media;

[Route( "api/[controller]" )]
[ApiController]
[Authorize]
public class MediaAssets : ControllerBase
{
	private readonly Supabase.Client _client;

	[HttpGet]
	public async Task<IActionResult> Get( Supabase.Client client )
	{
		return Ok( "Hello from MediaAssets" );
	}
}
