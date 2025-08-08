using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace AgrlyAPI.Models.Apartments;

[Table( "apartments" )]
public class Apartment : BaseModel
{
	/*
	 * "","","","","","","",""]
	 */
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

	[Column( "price_per_night" )]
	public decimal PricePerNight { get; set; }

	[Column( "bedrooms" )] 
	public int Bedrooms { get; set; } = 0;

	[Column( "max_guests" )] 
	public int MaxGuests { get; set; } = 0;

	[Column( "square_meter" )] 
	public int SquareMeter { get; set; } = 0;
	
	[Column("amenities")]
	public JsonObject? Amenities { get; set; }
	
	[Column("availability_status")]
	public string? AvailabilityStatus { get; set; }
	
	[Column( "minimum_stay" )]
	public int MinimumStay { get; set; }
	
	[Column("address_line1")]
	public string? AddressLine1 { get; set; }
	
	[Column("address_line2")]
	public string? AddressLine2 { get; set; }
	
	[Column("city")]
	public string? City { get; set; }
	
	[Column("state")]
	public string? State { get; set; }
	
	[Column("country")]
	public string? Country { get; set; }
	
	[Column( "postal_code")]
	public string? PostalCode { get; set; }
	
	[Column("latitude")]
	public float? Latitude { get; set; }
	
	[Column( "longitude")]
	public float? Longitude { get; set; }
	
	[Column("property_type")]
	public string? PropertyType { get; set; }
	
	[Column("instant_book")]
	public bool InstantBook { get; set; }
	
	[Column("rating")]
	public float Rating { get; set; }
	
	[Column("photos")]
	public List<string>? Photos { get; set; } = [];

	[Column( "created_at" )]
	public DateTime CreatedAt { get; set; }
	
	[Column( "updated_at" )]
	public DateTime UpdatedAt { get; set; }
	[Column( "apartment_tags" )]
	[JsonPropertyName( "apartment_tags" )]
	public List<string>? ApartmentTags { get; set; }
}
