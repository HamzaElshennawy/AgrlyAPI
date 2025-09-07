using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AgrlyAPI.Models.Apartments;

[Table("categories")]
public class Category : BaseModel
{
	[PrimaryKey( "id" ,false)] public int Id { get; set; }

	[Column( "name" )] public string Name { get; set; }

	[Column( "description" )] public string? Description { get; set; }

	[Column( "icon" )] public string? Icon { get; set; }

	[Column( "is_active" )] public bool IsActive { get; set; }

	[Column( "display_order" )] public int DisplayOrder { get; set; }
	[Column("img_path")] public string? ImgPath { get; set; }

	[Column( "created_at" )] public DateTime CreatedAt { get; set; }

	[Column( "updated_at" )] public DateTime UpdatedAt { get; set; }
}
