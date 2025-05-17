using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AgrlyAPI.Models.Apartments;

[Table( "apartments" )]
public class Apartment : BaseModel
{
	[PrimaryKey( "id", false )]
	public long Id { get; set; }

	[Column( "owner_id" )]
	public long OwnerId { get; set; }

	[Column( "title" )]
	public string? Title { get; set; }

	[Column( "description" )]
	public string? Description { get; set; }

	[Column( "location" )]
	public string? Location { get; set; }

	[Column( "price" )]
	public decimal PricePerNight { get; set; }

	//[Column( "is_available" )]
	//public bool IsAvailable { get; set; } = true;

	[Column( "created_at" )]
	public DateTime CreatedAt { get; set; }
}
