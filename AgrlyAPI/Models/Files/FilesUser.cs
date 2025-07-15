using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AgrlyAPI.Models.Files;

[Table( "files_user" )]
public class FilesUser : BaseModel
{
	[PrimaryKey( "id", false )]
	public long Id { get; set; }
	
	[Column( "user_id" )]
	public long UserId { get; set; }
	
	[Column( "file_path" )]
	public string? FilePath { get; set; }
	
	[Column( "public_url" )]
	public string? PublicUrl { get; set; }
	
	[Column( "type" )]
	public string? Type { get; set; }
	
	[Column( "uploaded_at" )]
	public DateTime UploadedAt { get; set; }
	[Column( "updated_at" )]
	public DateTime UpdatedAt { get; set; }
	
}

