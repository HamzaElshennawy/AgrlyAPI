using AgrlyAPI.Models.Addresss;
using AgrlyAPI.Models.Billings;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AgrlyAPI.Models.User;
[Table( "users" )]
public class User : BaseModel
{
	[PrimaryKey("id",false)]
	public long Id { get; set; }
	[Column( "username" )]
	public string Username { get; set; }
	[Column( "first_name" )]
	public string FirstName { get; set; }
	[Column( "last_name" )]
	public string LastName { get; set; }
	[Column( "email" )]
	public string Email { get; set; }
	[Column( "password" )]
	public string Password { get; set; }
	[Column( "created_at")]
	public DateTime CreatedAt { get; set; }
	//[Column( "is_admin" )]
	//public bool isAdmin { get; set; } = false;
	//[Column( "token" )]
	//public string? Token { get; set; }
	//[Column("billing_id")]
	//public Billing? BillingID { get; set; }
	//[Column( "address_id" )]
	//public Address? AddressID { get; set; }

	public User()
	{
		Username = "";
		FirstName = "";
		LastName = "";
		Email = "";
		Password = "";
		//isAdmin = false;
		//Token = null;
		//BillingID = new Billing();
		//AddressID = new Address();
	}

	public User( string username, string firstName, string lastName, string email, string password )
	{
		Username = username;
		FirstName = firstName;
		LastName = lastName;
		Email = email;
		Password = password;
	}

}
