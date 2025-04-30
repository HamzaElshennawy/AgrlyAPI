using AgrlyAPI.Models.Api;
using AgrlyAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgrlyAPI.Controllers.Users;

[Route("api/[controller]")]
[ApiController]
public class AuthenticateUser : ControllerBase
{
	private readonly JwtService _jwtService;

	public AuthenticateUser(JwtService jwtService) => _jwtService = jwtService;

	[AllowAnonymous]
	[HttpPost("auth")]
	public async Task<ActionResult<LoginResponseModel>> Login(LoginRequestModel request)
	{
		var response = await _jwtService.Authenticate(request);
		if (response is null)
			return Unauthorized("Invalid username or password");
		return Ok(response);
	}
}
