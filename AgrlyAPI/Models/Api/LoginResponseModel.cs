namespace AgrlyAPI.Models.Api;

public class LoginResponseModel
{
	public long Id { get; set; }
	public string? Username { get; set; }
	public string? Token { get; set; }
	public int ExpiresIn { get; set; }
}
