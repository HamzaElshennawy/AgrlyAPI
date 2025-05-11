using System.Threading.Tasks;
using AgrlyAPI.Models.User;
using AgrlyAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgrlyAPI.Controllers.users
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class usersController : ControllerBase
	{
		
		[AllowAnonymous]
		[HttpPost("adduser")]
		public async Task<IActionResult> Post(User newuser, Supabase.Client client)
		{
			// Input validation
			if (newuser == null)
				return BadRequest("User data is required");

			// Validate required fields
			if (string.IsNullOrWhiteSpace(newuser.Username))
				return BadRequest("Username is required");
			if (string.IsNullOrWhiteSpace(newuser.Password))
				return BadRequest("Password is required");

			// Check for existing username
			var existingUser = await client
				.From<User>()
				.Where(u => u.Username == newuser.Username)
				.Get();
			if (existingUser.Models.Any())
				return Conflict("Username already exists");

			// Set the creation timestamp
			newuser.CreatedAt = DateTime.UtcNow; // Use UTC time for consistency

			// Hash password before saving
			newuser.Password = PasswordHashHandler.HashPassword(newuser.Password);

			var response = await client.From<User>().Insert(newuser);
			var newuserId = response.Models.First().Id;
			return Ok(newuserId);
		}


		[HttpGet("getallusers")]
		public async Task<IActionResult> Get(Supabase.Client client)
		{
			var response = await client.From<User>().Get();
			if (response.Models is null)
				return BadRequest("No users found");
			return Ok(response.Models);
		}

		
		[HttpDelete("deleteuser/{id}")]
        public async Task<IActionResult> Delete(long id, Supabase.Client client)
        {
            if (id <= 0)
                return BadRequest("Invalid user ID");

            var localCurrentUsername = User.Identity?.Name;

            if (string.IsNullOrEmpty(localCurrentUsername))
                return Unauthorized("User not authenticated");

            var userIdClaim = long.Parse(User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            
			var currentUserResponse = await client.From<User>().Where( u => u.Id == userIdClaim).Get();
			var currentUser = currentUserResponse.Models.FirstOrDefault();

			if ( currentUser == null )
				return Unauthorized( "Current user not found" );

			if ( currentUser.IsAdmin )
			{
				await client.From<User>().Where( u => u.Id == id ).Delete();
				return Ok();
			}
			

			// Check if user exists
			var response = await client.From<User>().Where( u => u.Id == id ).Get();
			if ( !response.Models.Any() )
				return NotFound( "User not found" );

			var userToDelete = response.Models.First();

			if ( userToDelete.Username != localCurrentUsername )
			{
				return Forbid(); 
			}

			await client.From<User>().Where(u => u.Id == id).Delete();
			return Ok();
        }
	}
}
