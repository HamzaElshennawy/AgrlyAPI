using AgrlyAPI.Models.Apartments;
using AgrlyAPI.Models.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Supabase.Postgrest.Interfaces;
using System.Security.Claims;
using Newtonsoft.Json;
using static Supabase.Postgrest.Constants;
using AgrlyAPI.Models.Users;

namespace AgrlyAPI.Controllers.Apartments;

[Route( "api/[controller]" )]
[ApiController]
[Authorize]
[EnableRateLimiting( "fixed" )]
public class ApartmentsController( Supabase.Client client ) : ControllerBase
{
	// GET: api/apartments
	[HttpGet]
	[AllowAnonymous]
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
	[AllowAnonymous]
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
				.Order( "rating", Ordering.Descending )
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
			return Ok( response );
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

	[HttpGet( "gethistory/{userid}" )]
	public async Task<IActionResult> GetRentHistory( string userId )
	{
		try
		{
			var userIdClaim = User.Claims.FirstOrDefault( c => c.Type == ClaimTypes.NameIdentifier );
			if ( userIdClaim == null )
			{
				return Unauthorized( "Invalid or missing user ID." );
			}

			// Step 1: Fetch rents for the user
			var rentResponse = await client
				.From<RentHistory>()
				.Select( "*, apartment:apartment_id(*)" )
				.Filter( "user_id", Operator.Equals, userId )
				.Order( "start_date", Ordering.Descending )
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

	[AllowAnonymous]
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
			.Filter( "owner_id", Operator.Equals, ownerId )
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

	[AllowAnonymous]
	[HttpGet( "get_property_by_location/{location}" )]
	public async Task<IActionResult> GetPropertyByLocation( string location )
	{
		if ( string.IsNullOrWhiteSpace( location ) )
		{
			return BadRequest( "Location cannot be empty." );
		}

		try
		{
			var response = await client
				.From<Apartment>()
				.Filter( "location", Operator.ILike, $"%{location}%" )
				.Get();
			if ( response.Models.Count == 0 )
			{
				return NotFound( "No apartments found for the specified location." );
			}

			return Ok( response.Models );
		}
		catch ( Exception ex )
		{
			return StatusCode( StatusCodes.Status500InternalServerError, $"Server error: {ex.Message}" );
		}
	}

	[AllowAnonymous]
	[HttpPost( "get_property_by_gategory/" )]
	public async Task<IActionResult> GetPropertyByCategory( [FromBody] List<string>? categoryNames )
	{
		if ( categoryNames == null || categoryNames.Count == 0 )
		{
			return BadRequest( "Invalid category ID." );
		}

		try
		{
			// call stored procedure to get apartments by category
			var response = await client.Rpc( "get_apartments_by_categories",
				new Dictionary<string, List<string>>
				{
					{ "category_names", categoryNames }
				} );

			if ( response.Content == null ) return NotFound();
			var apartments = JsonConvert.DeserializeObject<List<Apartment>>( response.Content );
			return Ok( apartments );
		}
		catch ( Exception ex )
		{
			return StatusCode( StatusCodes.Status500InternalServerError, $"Server error: {ex.Message}" );
		}
	}

	[AllowAnonymous]
	[HttpGet( "categories" )]
	public async Task<IActionResult> GetCategories()
	{
		try
		{
			var response = await client
				.From<Category>()
				.Select( "*" )
				.Get();

			if ( response.Models.Count == 0 )
			{
				return NotFound( "No categories found." );
			}

			return Ok( response.Models );
		}
		catch ( Exception ex )
		{
			return StatusCode( StatusCodes.Status500InternalServerError, $"Server error: {ex.Message}" );
		}
	}

	[HttpPost( "categories/add" )]
	public async Task<IActionResult> AddCategory( [FromBody] Category category )
	{
		if ( string.IsNullOrWhiteSpace( category.Name ) )
		{
			return BadRequest( "Category name is required." );
		}

		// Check if category already exists
		var existingCategoryResponse = await client
			.From<Category>()
			.Where( c => c.Name == category.Name )
			.Get();
		if ( existingCategoryResponse.Models.Count > 0 )
		{
			return Conflict( "Category with this name already exists." );
		}

		category.CreatedAt = DateTime.UtcNow;
		category.UpdatedAt = DateTime.UtcNow;

		var result = await client
			.From<Category>()
			.Insert( category );

		return Ok( result.Models.FirstOrDefault() );
	}

	[HttpPut( "categories/update" )]
	public async Task<IActionResult> updateCategory( [FromBody] Category category )
	{
		if ( string.IsNullOrWhiteSpace( category.Name ) )
		{
			return BadRequest( "Category is required." );
		}


		category.UpdatedAt = DateTime.UtcNow;

		var result = await client
			.From<Category>()
			.Upsert( category );

		return Ok( result.Models.FirstOrDefault() );
	}


	// PUT: api/apartments/5
	[HttpPut( "{id:long}" )]
	public async Task<IActionResult> Update( long id, [FromBody] Apartment updated )
	{
		var userIdClaim = User.Claims.FirstOrDefault( c => c.Type == ClaimTypes.NameIdentifier );
		if ( userIdClaim == null || !long.TryParse( userIdClaim.Value, out var userId ) )
		{
			return Unauthorized();
		}

		var apartmentResponse = await client
			.From<Apartment>()
			.Where( a => a.Id == id )
			.Get();

		var apartment = apartmentResponse.Models.FirstOrDefault();
		if ( apartment == null )
		{
			return NotFound( "Apartment not found." );
		}

		var userResponse = await client
			.From<User>()
			.Where( u => u.Id == userId )
			.Get();

		var user = userResponse.Models.FirstOrDefault();
		if ( user == null )
		{
			return BadRequest( "User not found." );
		}

		Console.WriteLine( $"Recieved apartment update request: {updated.PricePerNight}" );
		if ( user.IsAdmin || apartment.OwnerId == user.Id )
		{
			updated.OwnerId = apartment.OwnerId;
			if ( updated.Title != null )
			{
				apartment.Title = updated.Title;
			}

			if ( updated.Description != null )
			{
				apartment.Description = updated.Description;
			}

			if ( updated.Location != null )
			{
				apartment.Location = updated.Location;
			}

			if ( updated.PricePerNight > 0 )
			{
				apartment.PricePerNight = updated.PricePerNight;
			}

			if ( updated.Bedrooms > 0 )
			{
				apartment.Bedrooms = updated.Bedrooms;
			}

			if ( updated.MaxGuests > 0 )
			{
				apartment.MaxGuests = updated.MaxGuests;
			}

			if ( updated.SquareMeter > 0 )
			{
				apartment.SquareMeter = updated.SquareMeter;
			}

			if ( updated.Amenities != null )
			{
				apartment.Amenities = updated.Amenities;
			}

			if ( updated.AvailabilityStatus != null )
			{
				apartment.AvailabilityStatus = updated.AvailabilityStatus;
			}

			if ( updated.MinimumStay > 0 )
			{
				apartment.MinimumStay = updated.MinimumStay;
			}

			if ( updated.AddressLine1 != null )
			{
				apartment.AddressLine1 = updated.AddressLine1;
			}

			if ( updated.AddressLine2 != null )
			{
				apartment.AddressLine2 = updated.AddressLine2;
			}

			if ( updated.City != null )
			{
				apartment.City = updated.City;
			}

			if ( updated.State != null )
			{
				apartment.State = updated.State;
			}

			if ( updated.Country != null )
			{
				apartment.Country = updated.Country;
			}

			if ( updated.PostalCode != null )
			{
				apartment.PostalCode = updated.PostalCode;
			}

			if ( updated.Latitude.HasValue )
			{
				apartment.Latitude = updated.Latitude.Value;
			}

			if ( updated.Longitude.HasValue )
			{
				apartment.Longitude = updated.Longitude.Value;
			}

			if ( updated.PropertyType != null )
			{
				apartment.PropertyType = updated.PropertyType;
			}

			if ( updated.InstantBook )
			{
				apartment.InstantBook = updated.InstantBook;
			}

			// if (updated.Rating.HasValue) apartment.Rating = updated.Rating.Value;
			if ( updated.Photos != null )
			{
				apartment.Photos = updated.Photos;
			}

			apartment.UpdatedAt = DateTime.UtcNow;

			await client
				.From<Apartment>()
				.Where( a => a.Id == id )
				.Update( apartment );
			
			

			return Ok( "Updated" );
		}

		return Forbid("You are not authorized to update this apartment." );
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

	[AllowAnonymous]
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
				new Supabase.Postgrest.QueryFilter( "title", Operator.ILike, $"%{query}%" ),
				new Supabase.Postgrest.QueryFilter( "description", Operator.ILike, $"%{query}%" )
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

	// TODO: implemete this
	[HttpPost( "{apartmentId:long}/review" )]
	public async Task<IActionResult> AddReview( long apartmentId, [FromBody] Reviews review )
	{
		await Task.Delay( 0 );
		return Ok();
	}

	// create a new booking
	[HttpPost( "{apartmentId:long}/book" )]
	public async Task<IActionResult> BookApartment( long apartmentId, DateTime checkIn, DateTime checkOut,
		int numGuests )
	{
		Console.WriteLine( $"Apartment id: {apartmentId}" );
		var userIdClaim = User.Claims.FirstOrDefault( c => c.Type == ClaimTypes.NameIdentifier );
		if ( userIdClaim == null || !long.TryParse( userIdClaim.Value, out var guestId ) )
		{
			return Unauthorized( "Invalid or missing user ID." );
		}

		if ( checkOut <= checkIn )
		{
			return BadRequest( "Check-out date must be after check-in date." );
		}

		var apartmentResponse = await client
			.From<Apartment>()
			.Where( a => a.Id == apartmentId )
			.Get();
		var apartment = apartmentResponse.Models.FirstOrDefault();
		if ( apartment == null )
		{
			return NotFound( "Apartment not found." );
		}

		var checkInIso = checkIn.ToString( "yyyy-MM-ddTHH:mm:ss" );
		var checkOutIso = checkOut.ToString( "yyyy-MM-ddTHH:mm:ss" );

		var overlapCheck = await client
			.From<Booking>()
			.Filter( "apartment_id", Operator.Equals, apartmentId.ToString() ) // convert to string
			.Filter( "status", Operator.Equals, "Booked" )
			.Filter( "check_in_date", Operator.LessThan, checkOutIso ) // string format
			.Filter( "check_out_date", Operator.GreaterThan, checkInIso ) // string format
			.Get();

		if ( overlapCheck.Models.Count != 0 )
		{
			return Conflict( "Apartment is already booked for the selected dates." );
		}

		Console.WriteLine( $"Check in : {checkIn}, Check out: {checkOut}, num of guests: {numGuests}" );

		// Calculate pricing
		var nights = ( checkOut - checkIn ).Days;
		double basePrice = (double)apartment.PricePerNight * nights;
		double cleaningFee = 50; // Example
		double serviceFee = basePrice * 0.1;
		double taxes = basePrice * 0.05;
		double totalAmount = basePrice + cleaningFee + serviceFee + taxes;
		Console.WriteLine( $"{nights} \n{basePrice} \n{cleaningFee} \n{serviceFee} \n{taxes} \n{totalAmount}" );

		var booking = new Booking
		{
			ApartmentId = apartmentId,
			GuestId = guestId,
			HostId = apartment.OwnerId,
			CheckInDate = checkIn,
			CheckOutDate = checkOut,
			//Nights = nights,
			NumGuests = numGuests,
			BasePrice = basePrice,
			CleaningFee = cleaningFee,
			ServiceFee = serviceFee,
			Taxes = taxes,
			TotalAmount = totalAmount,
			Status = "pending",
			CreatedAt = DateTime.UtcNow
		};

		//Console.WriteLine( booking.Nights );
		var result = await client.From<Booking>().Insert( booking );
		var createdBooking = result.Models.FirstOrDefault();

		// Optional: insert into rent_history
		var history = new RentHistory
		{
			UserId = guestId,
			ApartmentId = apartmentId,
			StartDate = checkIn,
			EndDate = checkOut,
			CreatedAt = DateTime.UtcNow,
			Status = "pending"
		};
		await client.From<RentHistory>().Insert( history );
		return Ok( createdBooking );
	}
}
