using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AgrlyAPI.Models.Files;

[Table("files_apartments")]
public class FilesApartments : BaseModel
{
	[PrimaryKey( "id" ,false)]
	public long id { get; set; }
	[Column( "user_id" )]
	public long UserID { get; set; }
	[Column( "apartment_id" )]
	public long ApartmetnID { get; set; }
	[Column( "file_path" )]
	public string? FilePath { get; set; }
	[Column("public_url")]
	public string? PublicUrl { get; set; }
	
	[Column( "uploaded_at" )]
	public DateTime UploadedAt { get; set; }
	
	[Column( "updated_at" )]
	public DateTime UpdatedAt { get; set; }

}

