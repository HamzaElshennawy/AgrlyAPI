using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json.Serialization;

namespace AgrlyAPI.Models.Apartments;

[Table( "bookings" )]
public class Booking : BaseModel
{
	[PrimaryKey( "id", false )]
	public long Id { get; set; }
	[Column( "apartment_id" )] 
	public long ApartmentId { get; set; }
	[Column( "guest_id" )] 
	public long GuestId { get; set; }
	[Column( "host_id" )] 
	public long HostId { get; set; }
	[Column( "check_in_date" )] 
	public DateTime CheckInDate { get; set; }
	[Column( "check_out_date" )] 
	public DateTime CheckOutDate { get; set; }
	//[Column( "nights" )]
	//[JsonIgnore]
	//public int? Nights { get; set; }
	[Column( "num_guests" )]
	public int NumGuests { get; set; }
	[Column( "base_price" )]
	public double BasePrice { get; set; }
	[Column( "cleaning_fee" )]
	public double CleaningFee { get; set; }
	[Column( "service_fee" )]
	public double ServiceFee { get; set; }
	[Column( "taxes" )]
	public double Taxes { get; set; }
	[Column( "total_amount" )]
	public double TotalAmount { get; set; }
	[Column( "status" )]
	public string? Status { get; set; }
	[Column( "special_requests" )]
	public string? SpecialRequests { get; set; }
	[Column( "created_at" )]
	public DateTime CreatedAt { get; set; }
	[Column( "updated_at" )]
	public DateTime UpdatedAt { get; set; }
	[Column( "cancelled_at" )]
	public DateTime? CancelledAt { get; set; }
	[Column( "cancellation_reason" )]
	public string? CancellationReason { get; set; }
}
