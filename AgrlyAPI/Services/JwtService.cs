using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AgrlyAPI.Models.Api;
using AgrlyAPI.Models.User;
using Microsoft.IdentityModel.Tokens;

namespace AgrlyAPI.Services;

public class JwtService( IConfiguration configuration, Supabase.Client client )
{
	public async Task<LoginResponseModel?> Authenticate( LoginRequestModel request )
	{
		if ( string.IsNullOrEmpty( request.Username ) || string.IsNullOrEmpty( request.Password ) )
			return null;

		var userAccount = await client
			.From<User>()
			.Where( u => u.Username == request.Username )
			.Get();
		var user = userAccount.Models.FirstOrDefault();
		if ( user is null || !PasswordHashHandler.VerifyPassword( request.Password, user.Password! ) )
			return null;

		var issuer = configuration["JwtConfig:Issuer"];
		var audience = configuration["JwtConfig:Audience"];
		var key = configuration["JwtConfig:Key"];
		var tokenValidityMins = configuration.GetValue<int>( "JwtConfig:TokenValidityMins" );
		var tokenExpiryTimeStamp = DateTime.Now.AddMinutes( tokenValidityMins );

		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(
				new[ ] {
				new Claim(ClaimTypes.Name, user.Username), // Correct claim for User.Identity.Name
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) // Correct claim for User.Identity.NameIdentifier
			} ),
			Expires = tokenExpiryTimeStamp,
			Issuer = issuer,
			Audience = audience,
			SigningCredentials = new SigningCredentials(
				new SymmetricSecurityKey( Encoding.ASCII.GetBytes( key! ) ),
				SecurityAlgorithms.HmacSha512Signature
			),
		};

		var tokenHandler = new JwtSecurityTokenHandler();
		var securityToken = tokenHandler.CreateToken( tokenDescriptor );
		var accessToken = tokenHandler.WriteToken( securityToken );

		return new LoginResponseModel
		{
			Id = user.Id,
			Username = request.Username,
			Token = accessToken,
			ExpiresIn = tokenValidityMins,
		};
	}
}
