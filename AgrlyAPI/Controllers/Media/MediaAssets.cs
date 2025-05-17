using AgrlyAPI.Models;
using AgrlyAPI.Models.Apartments;
using AgrlyAPI.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgrlyAPI.Controllers.Media;

[Route( "api/[controller]" )]
[ApiController]
[Authorize]
public class MediaAssetsController : ControllerBase
{
	private readonly Supabase.Client _client;

	public MediaAssetsController( Supabase.Client client )
	{
		_client = client;
	}


	
	[HttpGet( "apartments" )]
	public async Task<IActionResult> GetOwnerApartmentMedia()
	{
		// Step 1: Extract the current user's ID from JWT claims
		var userIdClaim = User.Claims.FirstOrDefault( c => c.Type == ClaimTypes.NameIdentifier );
		if ( userIdClaim == null || !long.TryParse( userIdClaim.Value, out var ownerId ) )
		{
			return Unauthorized( "Invalid or missing user ID." );
		}

		// Step 2: Fetch all apartments owned by this user
		var apartmentsResponse = await _client
			.From<Apartment>()
			.Filter( "owner_id", Supabase.Postgrest.Constants.Operator.Equals, (int)ownerId )
			.Get();

		var apartments = apartmentsResponse.Models;
		if ( apartments == null || apartments.Count == 0 )
		{
			return Ok( new List<Photos>() ); // No apartments = no media
		}

		// Step 3: Collect apartment IDs
		var apartmentIds = apartments.Select( a => a.Id ).Cast<object>().ToList();

		// Step 4: Fetch all media files linked to those apartments
		var filesResponse = await _client
			.From<Photos>()
			.Filter( "apartment_id", Supabase.Postgrest.Constants.Operator.In, apartmentIds )
			.Get();

		var files = filesResponse.Models;
		return Ok( files );
	}


	[HttpPost( "upload" )]
	public async Task<IActionResult> UploadMedia( IFormFile file)
	{
		if ( file == null || file.Length == 0 )
			return BadRequest( "No file provided." );

		var userIdClaim = User.Claims.FirstOrDefault( c => c.Type == ClaimTypes.NameIdentifier );
		if ( userIdClaim == null || !long.TryParse( userIdClaim.Value, out var userId ) )
			return Unauthorized( "Invalid or missing user ID." );

		var fileName = $"{Guid.NewGuid()}_{file.FileName}";
		var filePath = $"user-{userId}/{fileName}";

		var bucket = _client.Storage.From( "user-media" );

		// Convert file to byte[]
		byte[ ] fileBytes;
		await using ( var ms = new MemoryStream() )
		{
			await file.CopyToAsync( ms );
			fileBytes = ms.ToArray();
		}

		var result = await bucket.Upload( fileBytes, filePath, new Supabase.Storage.FileOptions
		{
			ContentType = file.ContentType,
			CacheControl = "3600",
			Upsert = true
		} );

		if ( !result.Any() )
			return StatusCode( 500, "Failed to upload file" );

		var publicUrl = bucket.GetPublicUrl( filePath );


		//update the database with the file path and user id
		var photo = new Photos
		{
			UserID = userId,
			FilePath = filePath,
			PublicUrl = publicUrl,
			Type = "apartment_photo",
			ApartmetnID = 1,
			UploadedAt = DateTime.UtcNow
		};

		var insertResponse = await _client
			.From<Photos>()
			.Insert( photo );


		return Ok( new { url = publicUrl } );
	}
}
