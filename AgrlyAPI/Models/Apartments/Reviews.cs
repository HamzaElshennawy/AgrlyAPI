using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AgrlyAPI.Models.Apartments;

[Table( "reviews" )]
public class Reviews : BaseModel
{
	[PrimaryKey( "id")]
	public long Id { get; set; }

	[Column( "booking_id" )]
	public long? BookingId { get; set; }

	[Column( "reviewer_id" )]
	public long? ReviewerId { get; set; }

	[Column( "apartment_id" )]
	public long? ApartmentId { get; set; }

	[Column( "host_id" )]
	public long? HostId { get; set; }

	[Column( "overall_rating" )]
	public int? OverallRating { get; set; }

	[Column( "cleanliness_rating" )]
	public int? CleanlinessRating { get; set; }

	[Column( "communication_rating" )]
	public int? CommunicationRating { get; set; }

	[Column( "check_in_rating" )]
	public int? CheckInRating { get; set; }

	[Column( "accuracy_rating" )]
	public int? AccuracyRating { get; set; }

	[Column( "location_rating" )]
	public int? LocationRating { get; set; }

	[Column( "value_rating" )]
	public int? ValueRating { get; set; }

	[Column( "title" )]
	public string? Title { get; set; }

	[Column( "comment" )]
	public string? Comment { get; set; }

	[Column( "review_type" )]
	public string? ReviewType { get; set; }

	[Column( "host_response" )]
	public string? HostResponse { get; set; }

	[Column( "host_response_date" )]
	public DateTime? HostResponseDate { get; set; }

	[Column( "created_at" )]
	public DateTime? CreatedAt { get; set; }

	[Column( "updated_at" )]
	public DateTime? UpdatedAt { get; set; }
}
