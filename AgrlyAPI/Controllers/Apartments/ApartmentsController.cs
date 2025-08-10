using AgrlyAPI.Models.Apartments;
using AgrlyAPI.Models.Api;
using AgrlyAPI.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Supabase.Postgrest.Interfaces;
using System.Linq;
using System.Security.Claims;
using static Supabase.Postgrest.Constants;

namespace AgrlyAPI.Controllers.Apartments;

[Route( "api/[controller]" )]
[ApiController]
[Authorize]
[EnableRateLimiting( "fixed" )]
public class ApartmentsController( Supabase.Client client ) : ControllerBase
{
	// GET: api/apartments
	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		var response = await client
			.From<Apartment>()
			.Get();

		return Ok( response.Models );
	}

	/// <summary>
	/// Get the avaiable apartments for rent (for Home page)
	/// </summary>
	/// <returns></returns>
	[HttpGet( "getavailable" )]
	public async Task<IActionResult> GetAvailable( int currentPage = 0 )
	{
		const int pageSize = 25;
		if ( currentPage < 0 )
		{
			currentPage = 0;
		}

		int from = currentPage * pageSize;
		int to = from + pageSize - 1;

		try
		{
			var apartmentsResponse = await client
		.From<Apartment>()
		.Select( "*" )
		.Order( "rating", Supabase.Postgrest.Constants.Ordering.Descending )
		.Range( from, to )
		.Get();

			var apartments = apartmentsResponse.Models;

			// Step 2: Fetch apartment_tags
			var apartmentTagsResponse = await client
				.From<ApartmentTags>()
				.Select( "*" )
				.Get();

			var apartmentTags = apartmentTagsResponse.Models;

			// Step 3: Fetch all tags
			var tagsResponse = await client
				.From<Tag>()
				.Select( "*" )
				.Get();

			var tags = tagsResponse.Models;

			// Step 4: Create lookup dictionary for tagId -> tagName
			var tagDict = tags.ToDictionary( t => t.Id, t => t.Name );

			// Step 5: Map tags to apartments
			foreach ( var apartment in apartments )
			{
				var tagIds = apartmentTags
					.Where( at => at.ApartmentId == apartment.Id )
					.Select( at => at.TagId )
					.ToList();

				apartment.ApartmentTags = tagIds
					.Where( tagDict.ContainsKey )
					.Select( tagId => tagDict[tagId] )
					.ToList()!;
			}
			var response = new AvailableApartmentsResponse
			{
				Apartments = apartments,
				CurrentPage = currentPage,
				StatusCode = 200
			};
			return Ok(response);
		}
		catch ( Supabase.Postgrest.Exceptions.PostgrestException ex )
		{
			return StatusCode( StatusCodes.Status500InternalServerError, "Server error: " + ex.Message );
		}
		catch ( Exception ex )
		{
			return StatusCode( StatusCodes.Status500InternalServerError, "Unexpected error: " + ex.Message );
		}
	}
	[HttpGet("gethistory/{userid}")]
	public async Task<IActionResult> GetRentHistory( string userId )
	{
		try
		{
			var userIdClaim = User.Claims.FirstOrDefault( c => c.Type == ClaimTypes.NameIdentifier );
			if ( userIdClaim == null || !long.TryParse( userIdClaim.Value, out var ownerId ) )
			{
				return Unauthorized( "Invalid or missing user ID." );
			}

			// Step 1: Fetch rents for the user
			var rentResponse = await client
				.From<RentHistory>()
				.Select( "*, apartment:apartment_id(*)" )
				.Filter( "user_id", Supabase.Postgrest.Constants.Operator.Equals, userId )
				.Order( "start_date", Supabase.Postgrest.Constants.Ordering.Descending )
				.Get();

			var rents = rentResponse.Models;

			var response = new RentHistoryResponse
			{
				UserId = userId,
				History = rents
			};

			return Ok( response );
		}
		catch ( Exception ex )
		{
			return StatusCode( 500, $"Server error: {ex.Message}" );
		}
	}
	// GET: api/apartments/5
	[HttpGet( "{id:long}" )]
	public async Task<IActionResult> GetById( long id )
	{
		var response = await client
			.From<Apartment>()
			.Where( a => a.Id == id )
			.Get();
		var apartment = response.Models.FirstOrDefault();
		// fetch the reviews for the apartment
		var reviewsResponse = await client
			.From<Reviews>()
			.Where( r => r.ApartmentId == id )
			.Get();


		if ( apartment == null )
		{
			return NotFound();
		}
		var reviews = reviewsResponse.Models;
		var apartmentByIdResponse = new ApartmentByIdResponse
		{
			Apartment = apartment,
			Reviews = reviews
		};

		return Ok( apartmentByIdResponse );
	}

	// GET: api/apartments/owned
	[HttpGet( "owned" )]
	public async Task<IActionResult> GetOwned()
	{
		var userIdClaim = User.Claims.FirstOrDefault( c => c.Type == ClaimTypes.NameIdentifier );
		if ( userIdClaim == null || !long.TryParse( userIdClaim.Value, out var ownerId ) )
		{
			return Unauthorized( "Invalid or missing user ID." );
		}

		var response = await client
			.From<Apartment>()
			.Filter( "owner_id", Supabase.Postgrest.Constants.Operator.Equals, ownerId )
			.Get();

		return Ok( response.Models );
	}

	// POST: api/apartments
	[HttpPost]
	public async Task<IActionResult> Create( [FromBody] Apartment apartment )
	{
		var userIdClaim = User.Claims.FirstOrDefault( c => c.Type == ClaimTypes.NameIdentifier );
		if ( userIdClaim == null || !long.TryParse( userIdClaim.Value, out var ownerId ) )
		{
			return Unauthorized( "Invalid or missing user ID." );
		}

		apartment.OwnerId = ownerId;
		apartment.CreatedAt = DateTime.UtcNow;

		var result = await client
			.From<Apartment>()
			.Insert( apartment );

		return Ok( result.Models.FirstOrDefault() );
	}

	// PUT: api/apartments/5
	[HttpPut( "{id:long}" )]
	public async Task<IActionResult> Update( long id, [FromBody] Apartment updated )
	{
		var userIdClaim = User.Claims.FirstOrDefault( c => c.Type == ClaimTypes.NameIdentifier );
		if ( userIdClaim == null || !long.TryParse( userIdClaim.Value, out var ownerId ) )
		{
			return Unauthorized();
		}

		var existing = await client
			.From<Apartment>()
			.Where( a => a.Id == id )
			.Get();

		var original = existing.Models.FirstOrDefault();
		if ( original == null || original.OwnerId != ownerId )
		{
			return Forbid( "You do not own this apartment." );
		}

		// Update only allowed fields
		original.Title = updated.Title ?? original.Title;
		original.Description = updated.Description ?? original.Description;

		var result = await client
			.From<Apartment>()
			.Update( original );

		return Ok( result.Models.FirstOrDefault() );
	}


	// DELETE: api/apartments/5
	[HttpDelete( "{id:long}" )]
	public async Task<IActionResult> Delete( long id )
	{
		var existing = await client
			.From<Apartment>()
			.Where( a => a.Id == id )
			.Get();

		var apartment = existing.Models.FirstOrDefault();
		if ( apartment == null )
		{
			return NotFound();
		}

		await client
			.From<Apartment>()
			.Delete( apartment );

		return StatusCode( StatusCodes.Status410Gone, "Apartment deleted successfully." );
	}


	// TODO: This needs more testing
	[HttpGet( "search" )]
	public async Task<IActionResult> Search( string query, int currentPage = 0 )
	{
		const int pageSize = 25;
		if ( currentPage < 0 )
		{
			currentPage = 0;
		}
		int from = currentPage * pageSize;
		int to = from + pageSize - 1;
		try
		{
			var filters = new List<IPostgrestQueryFilter>
			{
				new Supabase.Postgrest.QueryFilter( "title", Supabase.Postgrest.Constants.Operator.ILike, $"%{query}%" ),
				new Supabase.Postgrest.QueryFilter( "description", Supabase.Postgrest.Constants.Operator.ILike, $"%{query}%" )
			};
			var apartmentsRequest = await client
				.From<Apartment>()
				.Select( "*" )
				.Or( filters )
				.Range( from, to )
				.Get();
			var apartments = apartmentsRequest.Models;
			if ( apartments.Count == 0 )
			{
				var emptyResponse = new AvailableApartmentsResponse
				{
					Apartments = new List<Apartment>(),
					CurrentPage = currentPage,
					StatusCode = 200
				};
				return Ok( emptyResponse );
			}
			var response = new AvailableApartmentsResponse
			{
				Apartments = apartments,
				CurrentPage = currentPage,
				StatusCode = 200
			};
			return Ok( response );
		}
		catch ( Exception _ )
		{
			return StatusCode( StatusCodes.Status500InternalServerError, $"Server error: {_.Message}" );
		}
	}

}
