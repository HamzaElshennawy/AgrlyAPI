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
		public User user = new User();

		// This method needs some validation before executing
		// TODO: Validate the request
		// This request preform the basic functionality
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

			// Set creation timestamp
			newuser.CreatedAt = DateTime.UtcNow; // Use UTC time for consistency

			// Hash password before saving
			newuser.Password = PasswordHashHandler.HashPassword(newuser.Password);

			var response = await client.From<User>().Insert(newuser);
			var newuserId = response.Models.First().Id;
			return Ok(newuserId);
		}

		// This method needs some validation before executing
		// TODO: Validate the request
		// This request preform the basic functionality

		[HttpGet("getallusers")]
		public async Task<IActionResult> Get(Supabase.Client client)
		{
			var response = await client.From<User>().Get();
			if (response.Models is null)
				return BadRequest("No users found");
			return Ok(response.Models);
		}

		// This method needs some validation before executing
		// TODO: Validate the request
		// This request preform the basic functionality
		[HttpDelete("deleteuser/{id}")]
		public async Task<IActionResult> Delete(long id, Supabase.Client client)
		{
			// Validate ID
			if (id <= 0)
				return BadRequest("Invalid user ID");

			// Get current user's identity
			var currentUsername = User.Identity?.Name;
			if (string.IsNullOrEmpty(currentUsername))
				return Unauthorized("User not authenticated");

			// Check if user exists
			var response = await client.From<User>().Where(u => u.Id == id).Get();
			if (!response.Models.Any())
				return NotFound("User not found");

			var userToDelete = response.Models.First();

			// Authorization check:
			// 1. User can delete their own account
			// 2. Add additional role-based checks if needed
			if (userToDelete.Username != currentUsername)
			{
				// Optional: Check if user has admin role
				// var isAdmin = User.IsInRole("Admin");
				// if (!isAdmin)
				return Forbid("You don't have permission to delete this user");
			}

			await client.From<User>().Where(u => u.Id == id).Delete();
			return NoContent();
		}
	}
}
