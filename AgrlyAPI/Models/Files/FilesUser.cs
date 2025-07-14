using Supabase.Postgrest.Attributes;

namespace AgrlyAPI.Models.Files;

[Table( "files_user" )]
public class FileUser
{
	[PrimaryKey( "id", false )]
	public long Id { get; set; }
	
	[Column( "user_id" )]
	public long UserId { get; set; }
	
	[Column( "apartment_id" )]
	public long ApartmentId { get; set; }
	
	[Column( "file_path" )]
	public string? FilePath { get; set; }
	
	[Column( "public_url" )]
	public string? PublicUrl { get; set; }
	
	[Column( "type" )]
	public string? Type { get; set; }
	
	[Column( "uploaded_at" )]
	public DateTime UploadedAt { get; set; }
	
}

