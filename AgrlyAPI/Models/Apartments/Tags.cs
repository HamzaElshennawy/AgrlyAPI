using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AgrlyAPI.Models.Apartments;

[Table( "tags" )]
public class Tag : BaseModel
{
	[PrimaryKey( "id", false )]
	public long Id { get; set; }

	[Column( "name" )]
	public string? Name { get; set; }
}
