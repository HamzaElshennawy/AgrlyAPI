using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AgrlyAPI.Models.Users;

[Table("users")]
public class User : BaseModel
{
	[PrimaryKey("id", false)]
	public long Id { get; set; }

	[Column("username")]
	public string? Username { get; set; }

	[Column("first_name")]
	public string? FirstName { get; set; }

	[Column("last_name")]
	public string? LastName { get; set; }

	[Column("email")]
	public string? Email { get; set; }

	[Column("password")]
	public string? Password { get; set; }

	[Column("created_at")]
	public DateTime CreatedAt { get; set; }

    [Column("is_admin")]
    public bool IsAdmin { get; set; }

	[Column( "nationalid" )]
	public string? NationalID { get; set; }
	[Column( "phone" )]
	public string? Phone { get; set; }
	[Column( "profile_picture_url" )]
	public string? ProfilePictureUrl { get; set; }
	[Column( "bio" )]
	public string? Bio { get; set; }
	[Column( "date_of_birth" )]
	public DateTime? DateOfBirth { get; set; }
	[Column( "government_id_verified")]
	public bool GovernmentIdVerified { get; set; }
	[Column( "email_verified" )]
	public bool EmailVerified { get; set; }
	[Column( "phone_verified" )]
	public bool PhoneVerified { get; set; }
	[Column( "host_since" )]
	public DateTime? HostSince { get; set; }
	[Column( "is_superhost" )]
	public bool IsSuperhost { get; set; }
	[Column( "preferred_language" )]
	public string? PreferredLanguage { get; set; }
	[Column( "timezone" )]
	public string? Timezone { get; set; }
}
