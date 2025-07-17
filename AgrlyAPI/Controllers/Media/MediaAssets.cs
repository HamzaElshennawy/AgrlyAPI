using AgrlyAPI.Models.Apartments;
using AgrlyAPI.Models.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace AgrlyAPI.Controllers.Media;

[Route( "api/[controller]" )]
[ApiController]
[Authorize]
[EnableRateLimiting( "fixed" )]
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
			var _ = int.Parse( apartmentId );
		}
		catch (Exception)
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

	[HttpPost( "upload-user-media" )]
	public async Task<IActionResult> UploadMediaUser( IFormFile? file, string type )
	{
		if(file == null || file.Length == 0)
		{
			return BadRequest( "No file provided." );
		}
		var userIdClaim = User.Claims.FirstOrDefault( c => c.Type == ClaimTypes.NameIdentifier );
		if ( userIdClaim == null || !long.TryParse( userIdClaim.Value, out var userId ) )
		{
			return Unauthorized( "Invalid or missing user ID." );
		}
		Console.WriteLine( $"Uploading user id {userId}" );
		
		var fileName = $"{Guid.NewGuid()}_{file.FileName}";
		var filePath = $"user-{userId}/{fileName}";
		
		byte[ ] fileBytes;
		var bucket = client.Storage.From( "user-media" );

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
		var photoFile = new FilesUser
		{
			UserId  = userId,
			FilePath = filePath,
			PublicUrl = publicUrl,
			Type = type,
			UploadedAt = DateTime.Now,
			UpdatedAt = DateTime.Now
		};
		Console.WriteLine( $"Uploading user id {photoFile.Id}" );

		var insertResponse = await client
			.From<FilesUser>()
			.Insert( photoFile );

		// TODO: This return needs to be tested !!!
		return insertResponse.ResponseMessage is { IsSuccessStatusCode: true } ? Ok( new { url = publicUrl } ) : StatusCode( 500, "Failed to upload file" );
	}

	[HttpPost( "upload-user-profile" )]
	public async Task<IActionResult> UploadUserProfile( IFormFile? file )
	{
		if ( file == null || file.Length == 0 )
		{
			return BadRequest( "No file provided." );
		}
		
		var userIdClaim = User.Claims.FirstOrDefault( c => c.Type == ClaimTypes.NameIdentifier );
		if ( userIdClaim == null || !long.TryParse( userIdClaim.Value, out var userId ) )
		{
			return Unauthorized( "Invalid or missing user ID." );
		}
		
		var filePath = $"profiles/{userId}.jpg";
		var bucket = client.Storage.From( "user-profiles" );
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
		var photoFile = new FilesUser
		{
			UserId = userId,
			FilePath = filePath,
			PublicUrl = publicUrl,
			Type = "profile",
			UploadedAt = DateTime.Now,
			UpdatedAt = DateTime.Now
		};
		var insertResponse = await client
			.From<FilesUser>()
			.Insert( photoFile );
		if ( insertResponse.ResponseMessage is { IsSuccessStatusCode: true } )
		{
			return Ok( new { url = publicUrl } );
		}
		else
		{
			return StatusCode( 500, "Failed to upload file" );
		}
	}

	// TODO: This endpoint needs testing again because i didnt test it enough.
	/// <summary>
	/// Updates the profile media (image) of the currently authenticated user.
	/// </summary>
	/// <param name="file">The profile image file uploaded by the user. It should be a non-null, non-empty image file.</param>
	/// <returns>
	/// Returns:
	/// - <see cref="BadRequestObjectResult"/> if the file is null or empty.
	/// - <see cref="UnauthorizedResult"/> if the user ID is missing or invalid in the JWT.
	/// - <see cref="ObjectResult"/> with 500 status code if uploading or updating the file fails.
	/// - <see cref="OkObjectResult"/> containing the public URL of the uploaded file if successful.
	/// </returns>
	
	[HttpPut( "update-user-profile-media" )]
	public async Task<IActionResult> UpdateUserProfileMedia( IFormFile? file )
	{
		if ( file == null || file.Length == 0 )
		{
			return BadRequest( "No file provided." );
		}
		
		// Get the user ID from the JWT claims
		var userIdClaim = User.Claims.FirstOrDefault( c => c.Type == ClaimTypes.NameIdentifier );
		if ( userIdClaim == null || !long.TryParse( userIdClaim.Value, out var userId ) )
		{
			return Unauthorized( "Invalid or missing user ID." );
		}
		
		
		var bucket = client.Storage.From( "user-profiles" );
		
		var filePath = $"profiles/{userId}.jpg";
		byte[ ] fileBytes;
		await using ( var ms = new MemoryStream() )
		{
			await file.CopyToAsync( ms );
			fileBytes = ms.ToArray();
		}
		
		var result = await bucket.Update( fileBytes, filePath, new Supabase.Storage.FileOptions
		{
			ContentType = file.ContentType,
			CacheControl = "3600",
			Upsert = true
		});

		if ( result.Length == 0 )
		{
			return StatusCode( 500, "Failed to upload file" );
		}

		var publicUrl = bucket.GetPublicUrl( filePath );
		var photoFile = new FilesUser
		{
			UserId = userId,
			FilePath = filePath,
			PublicUrl = publicUrl,
			Type = "profile",
			UploadedAt = DateTime.Now,
			UpdatedAt = DateTime.Now
		};

		var insertResponse = await client
			.From<FilesUser>()
			.Update( photoFile );
		if ( insertResponse.ResponseMessage is { IsSuccessStatusCode: true } )
		{
			return Ok( new { url = publicUrl } );
		}
		else
		{
			return StatusCode( 500, "Failed to upload file" );
		}
	}
}
