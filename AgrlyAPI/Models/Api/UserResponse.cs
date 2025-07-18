using AgrlyAPI.Models.Users;
using AgrlyAPI.Models.Users;


namespace AgrlyAPI.Models.Api;
public class UserResponse
{
	public long Id { get; set; }
	public string? Username { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Email { get; set; }
	public string? Password { get; set; }
	public DateTime CreatedAt { get; set; }
	public bool IsAdmin { get; set; }
	public string? NationalID { get; set; }
	public string? Phone { get; set; }

	public List<Transactions?> SentTransactions { get; set; } = new();
	public List<Transactions?> ReceivedTransactions { get; set; } = new();
	public List<Billing?> Billing { get; set; } = new();
}
