using AgrlyAPI.Models.Addresss;
using AgrlyAPI.Models.Billings;

namespace AgrlyAPI.Models.User;

public class User
{
	public Guid Id { get; set; }
	public string Username { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string Email { get; set; }
	public string Password { get; set; }
	public bool isAdmin { get; set; } = false;
	public string? Token { get; set; }
	public List<Billing>? Billings { get; set; }
	public List<Address>? Addresses { get; set; }


	public User()
	{
		Id = Guid.NewGuid();
		Username = "";
		FirstName = "";
		LastName = "";
		Email = "";
		Password = "";
		isAdmin = false;
		Token = null;
		Billings = new List<Billing>();
		Addresses = new List<Address>();
	}

	public User( string username, string firstName, string lastName, string email, string password )
	{
		Username = username;
		FirstName = firstName;
		LastName = lastName;
		Email = email;
		Password = password;
	}

	public void AddBilling( Billing billing )
	{
		if ( Billings == null )
		{
			Billings = new List<Billing>();
		}
		Billings.Add( billing );
	}
	public void AddAddress( Address address )
	{
		if ( Addresses == null )
		{
			Addresses = new List<Address>();
		}
		Addresses.Add( address );
	}
	public List<Billing> GetBillings()
	{
		if( Billings == null )
		{
			Billings = new List<Billing>();
		}
		return Billings;
	}
	public List<Address> GetAddresses()
	{
		if ( Addresses == null )
		{
			Addresses = new List<Address>();
		}
		return Addresses;
	}
	public void RemoveBilling( Billing billing )
	{
		if ( Billings != null )
		{
			Billings.Remove( billing );
		}
	}
	public void RemoveAddress( Address address )
	{
		if ( Addresses != null )
		{
			Addresses.Remove( address );
		}
	}
	public void UpdateBilling( Billing billing )
	{
		if ( Billings != null )
		{
			var index = Billings.FindIndex( b => b.Id == billing.Id );
			if ( index != -1 )
			{
				Billings[index] = billing;
			}
		}
	}
}
