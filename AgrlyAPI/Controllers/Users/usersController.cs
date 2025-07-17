using AgrlyAPI.Models.User;
using AgrlyAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AgrlyAPI.Controllers.users
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	[EnableRateLimiting("fixed")]
	public class UsersController : ControllerBase
	{
		
		[AllowAnonymous]
		[HttpPost("adduser")]
		public async Task<IActionResult> Post(User? newuser, Supabase.Client client)
		{
			// Input validation
			if (newuser == null)
			{
				return BadRequest("User data is required");
			}

			// Validate required fields
			if (string.IsNullOrWhiteSpace(newuser.Email))
			{
				return BadRequest("Email is required");
			}

			if (string.IsNullOrWhiteSpace(newuser.Password))
			{
				return BadRequest("Password is required");
			}

			// Check for existing username
			var existingUser = await client
				.From<User>()
				.Where(u => u.Email == newuser.Email)
				.Get();
			if (existingUser.Models.Count != 0 )
			{
				return Conflict("Email already exists");
			}

			// Set the creation timestamp
			newuser.CreatedAt = DateTime.UtcNow; // Use UTC time for consistency

			// Hash password before saving
			newuser.Password = PasswordHashHandler.HashPassword(newuser.Password);

			var response = await client.From<User>().Insert(newuser);
			var newUserId = response.Models.First().Id;
			return Ok(newUserId);
		}


        [HttpGet("getallusers")]
        public async Task<IActionResult> Get(Supabase.Client client)
        {
            var usersResponse = await client.From<User>().Get();
            if (usersResponse.Models.Count == 0)
            {
                return BadRequest("No users found");
            }

            var users = usersResponse.Models;
            var userIds = users.Select(u => u.Id).ToArray();

            var transactionsResponse = await client
                .From<Models.Users.Transactions>()
                .Filter("senderid", Supabase.Postgrest.Constants.Operator.In, userIds.ToList())
                .Filter("receiverid", Supabase.Postgrest.Constants.Operator.In, userIds.ToList())
                .Get();

            var billingsResponse = await client
                .From<Billing>()
                .Filter("user_id", Supabase.Postgrest.Constants.Operator.In, userIds.ToList())
                .Get();

            var transactions = transactionsResponse.Models;
            var billings = billingsResponse.Models;

            var userResponses = users.Select(user => new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Password = user.Password,
                CreatedAt = user.CreatedAt,
                IsAdmin = user.IsAdmin,
                NationalID = user.NationalID,
                Phone = user.Phone,
                SentTransactions = transactions.Where(t => t.senderID == user.Id).Cast<Models.Users.Transactions?>().ToList(),
                ReceivedTransactions = transactions.Where(t => t.receiverID == user.Id).Cast<Models.Users.Transactions?>().ToList(),
                Billing = billings.Where(b => b.UserId == user.Id).Cast<Billing?>().ToList()
            }).ToList();

            return Ok(userResponses);
        }




		[HttpDelete("deleteuser/{id:long}")]
		public async Task<IActionResult> Delete(long id, Supabase.Client client)
        {
            if (id <= 0)
            {
	            return BadRequest("Invalid user ID");
            }

            var localCurrentUsername = User.Identity?.Name;

            if (string.IsNullOrEmpty(localCurrentUsername))
            {
	            return Unauthorized("User not authenticated");
            }

            var userIdClaim = long.Parse(User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            
			var currentUserResponse = await client.From<User>().Where( u => u.Id == userIdClaim).Get();
			var currentUser = currentUserResponse.Models.FirstOrDefault();

			if ( currentUser == null )
			{
				return Unauthorized( "Current user not found" );
			}

			// Check if user exists
			var response = await client.From<User>().Where( u => u.Id == id ).Get();
			if ( response.Models.Count == 0 )
			{
				return NotFound( "User not found" );
			}
			// Check if the current user is an admin
			if ( currentUser.IsAdmin )
			{
				await client.From<User>().Where( u => u.Id == id ).Delete();
				await client.From<Billing>()
				.Where( b => b.UserId == id )
				.Delete();
				var bucket = client.Storage.From( "user-profiles" );
				
				await bucket.Remove( $"profiles/{userIdClaim}.jpg" );

				bucket = client.Storage.From( "user-media" );

				await bucket.Remove( $"user-{userIdClaim}" );

				return Ok();
			}
			


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
