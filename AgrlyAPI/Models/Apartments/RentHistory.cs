using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AgrlyAPI.Models.Apartments
{
	[Table( "rent_history" )]
	public class RentHistory : BaseModel
	{
		[PrimaryKey( "id" )]
		public int Id { get; set; }

		[Column( "user_id" )]
		public long UserId { get; set; }

		[Column( "apartment_id" )]
		public long ApartmentId { get; set; }

		[Column( "start_date" )]
		public DateTime StartDate { get; set; }

		[Column( "end_date" )]
		public DateTime EndDate { get; set; }
		[Column("status")]
		public string? Status { get; set; }

		
		[Column("created_at")]
		public DateTime CreatedAt { get; set; }
		[Column("updated_at")]
		public DateTime UpdatedAt { get; set; }
	}
}
