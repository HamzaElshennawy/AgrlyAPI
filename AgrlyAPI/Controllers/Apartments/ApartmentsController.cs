﻿using AgrlyAPI.Models.Apartments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgrlyAPI.Controllers.Apartments;

[Route( "api/[controller]" )]
[ApiController]
[Authorize]
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

	// GET: api/apartments/5
	[HttpGet( "{id:long}" )]
	public async Task<IActionResult> GetById( long id )
	{
		var response = await client
			.From<Apartment>()
			.Where( a => a.Id == id )
			.Get();

		var apartment = response.Models.FirstOrDefault();
		if ( apartment == null )
		{
			return NotFound();
		}

		return Ok( apartment );
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

		return NoContent();
	}
}
