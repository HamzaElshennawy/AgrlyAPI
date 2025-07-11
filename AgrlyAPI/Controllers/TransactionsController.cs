using AgrlyAPI.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgrlyAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
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
		var response = await client.From<Transactions>().Get();
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
			.From<Transactions>()
			.Where( t => t.Id == id ).Where(t=>t.senderID == userId || t.receiverID == userId).Get();

        var transaction = response.Models.FirstOrDefault();
        if (transaction == null)
        {
	        return NotFound();
        }

        return Ok(transaction);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Transactions transaction)
    {
        transaction.CreatedAt = DateTime.UtcNow;
        var response = await client.From<Transactions>().Insert(transaction);
        return Ok(response.Models.FirstOrDefault());
    }
}
