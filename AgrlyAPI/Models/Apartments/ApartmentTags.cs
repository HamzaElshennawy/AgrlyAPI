using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.Json.Serialization;

namespace AgrlyAPI.Models.Apartments
{
	[Table( "apartment_tags" )]
	public class ApartmentTags : BaseModel
	{
		[PrimaryKey( "id", false )]
		public long Id { get; set; }

		[Column( "apartment_id" )]
		public long ApartmentId { get; set; }


		[Column( "tag_id" )]
		public long TagId { get; set; }

		[Column( "tags" )]
		[JsonPropertyName( "tags" )]
		public Tag? Tag { get; set; }
	}
}
