using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table( "billing" )]
public class Billing : BaseModel
{
	[PrimaryKey( "id", false )]
	public int Id { get; set; }
	[Column( "cardholdername" )]
	public string? CardHolderName { get; set; }
	[Column( "cardnumber" )]
	public string? CardNumber { get; set; }
	[Column( "expirationdate" )]
	public string? ExpirationDate { get; set; }
	[Column( "cvv" )]
	public string? CVV { get; set; }
	[Column( "cardtype" )]
	public string? CardType { get; set; }

	[Column( "user_id" )]
	public long UserId { get; set; }  // Foreign key to users
}
