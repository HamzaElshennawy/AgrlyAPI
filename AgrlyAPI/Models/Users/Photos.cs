using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AgrlyAPI.Models.Users;

[Table("files")]
public class Photos : BaseModel
{
	[PrimaryKey( "id" ,false)]
	public long id { get; set; }
	[Column( "user_id" )]
	public long UserID { get; set; }
	[Column( "apartment_id" )]
	public long ApartmetnID { get; set; }
	[Column( "file_path" )]
	public string? FilePath { get; set; }
	[Column( "type" )]
	public string? Type { get; set; }
	[Column( "uploaded_at" )]
	public DateTime UploadedAt { get; set; }
}

