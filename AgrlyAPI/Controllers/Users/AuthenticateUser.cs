using AgrlyAPI.Models.Api;
using AgrlyAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AgrlyAPI.Controllers.Users;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting("fixed")]
public class AuthenticateUser( JwtService jwtService ) : ControllerBase
{
	[AllowAnonymous]
	[HttpPost("auth")]
	public async Task<ActionResult<LoginResponseModel>> Login(LoginRequestModel request)
	{
		var response = await jwtService.Authenticate(request);
		if (response is null)
		{
			return Unauthorized("Invalid username or password");
		}

		return Ok(response);
	}
}
