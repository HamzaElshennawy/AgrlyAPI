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


	/**
	 * this method retrieves all the media files associated with the apartments owned by the user.
	 * TODO: NOT WORKING YET!!!
	 */
	[HttpGet( "apartments" )]
	public async Task<IActionResult> GetOwnerApartmentMedia()
	{
		var userIdClaim = User.Claims.FirstOrDefault( c => c.Type == ClaimTypes.NameIdentifier );
		if ( userIdClaim == null || !long.TryParse( userIdClaim.Value, out var ownerId ) )
		{
			return Unauthorized( "Invalid or missing user ID." );
		}

		var apartmentsResponse = await _client
			.From<Apartment>()
			.Filter( "owner_id", Supabase.Postgrest.Constants.Operator.Equals, ownerId )
			.Get();

		var apartments = apartmentsResponse.Models;
		if ( apartments == null || apartments.Count == 0 )
			return Ok( new List<Photos>() );

		var apartmentIds = apartments.Select( a => a.Id ).ToList();
		var apartmentIdCsv = string.Join( ",", apartmentIds );

		var filesResponse = await _client
			.From<Photos>()
			.Filter( "apartment_id", Supabase.Postgrest.Constants.Operator.In, $"({apartmentIdCsv})" )
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
			Type = "apartment_photo",
			ApartmetnID = 1,
			UploadedAt = DateTime.UtcNow
		};
		Console.WriteLine( "User ID: ", photo.UserID );
		Console.WriteLine( "File Path: ", photo.FilePath );
		Console.WriteLine( "Type: ", photo.Type );
		Console.WriteLine( "Uploaded at: ", photo.UploadedAt );
		var insertResponse = await _client
			.From<Photos>()
			.Insert( photo );


		return Ok( new { url = publicUrl } );
	}
}
