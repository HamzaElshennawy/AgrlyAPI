using AgrlyAPI.Models.Apartments;
using AgrlyAPI.Models.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgrlyAPI.Controllers.Media;

[Route( "api/[controller]" )]
[ApiController]
[Authorize]
public class MediaAssetsController( Supabase.Client client ) : ControllerBase
{
	
	/// <summary>
	/// Retrieves all media files (Photo) associated with the apartments owned by the currently authenticated user.
	/// </summary>
	/// <returns>
	/// An <see cref="IActionResult"/> containing a list of <see cref="FilesApartments"/> if the user has apartments with media,
	/// an empty list if no apartments exist, or an <see cref="UnauthorizedResult"/> if the user ID is missing or invalid.
	/// </returns>
	
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
		var apartmentsResponse = await client
			.From<Apartment>()
			.Filter( "owner_id", Supabase.Postgrest.Constants.Operator.Equals, (int)ownerId )
			.Get();

		var apartments = apartmentsResponse.Models;
		if ( apartments.Count == 0 )
		{
			return Ok( new List<FilesApartments>() ); // No apartments = no media
		}

		// Step 3: Collect apartment IDs
		var apartmentIds = apartments.Select( a => a.Id ).Cast<object>().ToList();

		// Step 4: Fetch all media files linked to those apartments
		var filesResponse = await client
			.From<FilesApartments>()
			.Filter( "apartment_id", Supabase.Postgrest.Constants.Operator.In, apartmentIds )
			.Get();

		var files = filesResponse.Models;
		return Ok( files );
	}
	
	/// <summary>
	/// Uploads a media file to the storage bucket and stores its metadata in the database, associating it with the authenticated user.
	/// </summary>
	/// <param name="file">The media file to upload.</param>
	/// <param name="apartmentId">The ID of the apartment to associate the media file with.</param>
	/// <returns>
	/// An <see cref="IActionResult"/> containing the public URL of the uploaded file on success,
	/// <see cref="BadRequestResult"/> if the file is missing or empty,
	/// <see cref="UnauthorizedResult"/> if the user ID is invalid or missing,
	/// or <see cref="StatusCodeResult"/> (500) if the upload or database insertion fails.
	/// </returns>
	
	[HttpPost( "upload-apartment-photo" )]
	public async Task<IActionResult> UploadMediaApartment( IFormFile? file, string apartmentId )
	{
		try
		{
			int.Parse( apartmentId );
		}
		catch ( Exception e )
		{
			return BadRequest("Apartment ID must be a valid ID.");
		}
		if ( file == null || file.Length == 0 )
		{
			return BadRequest( "No file provided." );
		}

		var userIdClaim = User.Claims.FirstOrDefault( c => c.Type == ClaimTypes.NameIdentifier );
		if ( userIdClaim == null || !long.TryParse( userIdClaim.Value, out var userId ) )
		{
			return Unauthorized( "Invalid or missing user ID." );
		}

		var fileName = $"{Guid.NewGuid()}_{file.FileName}";
		var filePath = $"apartment-{apartmentId}/{fileName}";

		var bucket = client.Storage.From( "apartment-media" );

		// Convert a file to byte[]
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

		if ( result.Length == 0 )
		{
			return StatusCode( 500, "Failed to upload file" );
		}

		var publicUrl = bucket.GetPublicUrl( filePath );

		
		//update the database with the file path and user id
		var photo = new FilesApartments
		{
			UserID = userId,
			FilePath = filePath,
			PublicUrl = publicUrl,
			ApartmetnID = long.Parse(apartmentId),
			UploadedAt = DateTime.Now,
			UpdatedAt = DateTime.Now
		};

		var insertResponse = await client
			.From<FilesApartments>()
			.Insert( photo );

		// TODO: This return needs to be tested !!!
		return insertResponse.ResponseMessage is { IsSuccessStatusCode: true } ? Ok( new { url = publicUrl } ) : StatusCode( 500, "Failed to upload file" );
	}
}
