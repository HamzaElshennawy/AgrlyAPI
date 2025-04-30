using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AgrlyAPI.Models.Api;
using AgrlyAPI.Models.User;
using Microsoft.IdentityModel.Tokens;

namespace AgrlyAPI.Services;

public class JwtService
{
	private readonly IConfiguration _configuration;
	private readonly Supabase.Client client;

	public JwtService(IConfiguration configuration, Supabase.Client client)
	{
		_configuration = configuration;
		this.client = client;
	}

	public async Task<LoginResponseModel?> Authenticate(LoginRequestModel request)
	{
		if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
			return null;

		var userAccount = await client
			.From<User>()
			.Where(u => u.Username == request.Username)
			.Get();
		var user = userAccount.Models.FirstOrDefault();
		if (user is null || !PasswordHashHandler.VerifyPassword(request.Password, user.Password))
			return null;

		var issuer = _configuration["JwtConfig:Issuer"];
		var audience = _configuration["JwtConfig:Audience"];
		var key = _configuration["JwtConfig:Key"];
		var tokenValidityMins = _configuration.GetValue<int>("JwtConfig:TokenValidityMins");
		var tokenExpiryTimeStamp = DateTime.Now.AddMinutes(tokenValidityMins);

		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(
				new[] { new Claim(JwtRegisteredClaimNames.Name, request.Username) }
			),
			Expires = tokenExpiryTimeStamp,
			Issuer = issuer,
			Audience = audience,
			SigningCredentials = new SigningCredentials(
				new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
				SecurityAlgorithms.HmacSha512Signature
			),
		};

		var tokenHandler = new JwtSecurityTokenHandler();
		var securityToken = tokenHandler.CreateToken(tokenDescriptor);
		var accessToken = tokenHandler.WriteToken(securityToken);

		return new LoginResponseModel
		{
			Username = request.Username,
			Token = accessToken,
			ExpiresIn = tokenValidityMins,
		};
	}
}
