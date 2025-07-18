using AgrlyAPI.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using Supabase.Postgrest;
using Supabase.Postgrest.Interfaces;

namespace AgrlyAPI.Controllers.Transactions;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting("fixed")]
public class TransactionsController( Supabase.Client client ) : ControllerBase
{
	[HttpGet]
    public async Task<IActionResult> GetAll()
    {
		// make sure the user is admin
		var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
		if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
		{
			return Unauthorized("Invalid or missing user ID.");
		}
		var userResponse = await client.From<User>().Where(u => u.Id == userId).Get();
		var user = userResponse.Models.FirstOrDefault();
		if (user is not { IsAdmin: true } )
		{
			return Forbid();
		}
		// Fetch all transactions
		var response = await client.From<Models.Users.Transactions>().Get();
        return Ok(response.Models);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        // make sure this transaction belongs to the user
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized("Invalid or missing user ID.");
        }
        var userResponse = await client.From<User>().Where(u => u.Id == userId).Get();
        var user = userResponse.Models.FirstOrDefault();
        if (user == null)
        {
            return NotFound("User not found.");
        }

		var response = await client
			.From<Models.Users.Transactions>()
			.Where( t => t.Id == id ).Where(t=>t.senderID == userId || t.receiverID == userId).Get();

        var transaction = response.Models.FirstOrDefault();
        if (transaction == null)
        {
	        return NotFound();
        }

        return Ok(transaction);
    }

    [HttpPost("create-transaction")]
    public async Task<IActionResult> Create([FromBody] Models.Users.Transactions transaction)
    {
        transaction.CreatedAt = DateTime.UtcNow;
        var response = await client.From<Models.Users.Transactions>().Insert(transaction);
        return Ok(response.Models.FirstOrDefault());
    }

    [HttpDelete( "delete-transaction/{id:long}" )]
    public async Task<IActionResult> Delete( long id )
    {
	    if ( id <= 0 )
	    {
		    return BadRequest("Invalid transaction ID");
	    }
	    var response = await client.From<Models.Users.Transactions>().Where(t => t.Id == id).Get();
	    if( response.Models.Count == 0)
	    {
		    return NotFound("Transaction not found");
	    }
	    await client.From<Models.Users.Transactions>().Where(t => t.Id == id).Delete( );
	    return Ok();
    }

    [HttpGet( "user-transactions" )]
    public async Task<IActionResult> GetUserTransactions()
    {
	    var userIdClaim = User.Claims.FirstOrDefault( c => c.Type == ClaimTypes.NameIdentifier );
	    if ( userIdClaim == null || !long.TryParse( userIdClaim.Value, out var userId ) )
	    {
		    return Unauthorized();
	    }
	    var filters = new List<IPostgrestQueryFilter>
	    {
			new QueryFilter( "senderid", Supabase.Postgrest.Constants.Operator.Equals, userId ),
		    new QueryFilter( "receiverid", Supabase.Postgrest.Constants.Operator.Equals, userId ),
	    };
	    
	    var response = await client
		    .From<Models.Users.Transactions>()
		    .Or( filters )
		    .Get();
	    if ( response.Models.Count == 0 )
	    {
		    return NotFound();
	    }
	    return Ok(response.Models.ToArray());
    }
}
