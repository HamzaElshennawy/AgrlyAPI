using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AgrlyAPI.Models.User;
using System.Threading.Tasks;

namespace AgrlyAPI.Controllers.users
{
    [Route("api/[controller]")]
    [ApiController]
    public class usersController : ControllerBase
    {

		public User user = new User();



		// This method needs some validation before executing
		// TODO: Validate the request
		// This request preform the basic functionality

		[HttpPost("adduser")]
		public async Task<IActionResult> Post(User newuser, Supabase.Client client)
		{
			if ( newuser != null )
			{
				user = newuser;
				newuser.CreatedAt = DateTime.Now;
				var response = await client.From<User>().Insert( user );
				var newuserID = response.Models.First().Id;
				return Ok( newuserID );
			}
			else
			{
				return BadRequest();
			}
		}


		// This method needs some validation before executing
		// TODO: Validate the request
		// This request preform the basic functionality

		[HttpGet("getallusers")]
		public async Task<IActionResult> Get(Supabase.Client client)
		{
			var response = await client.From<User>().Get();
			if ( response.Models is null ) return BadRequest( "No users found" );
			return Ok( response.Models );
		}


		// This method needs some validation before executing
		// TODO: Validate the request
		// This request preform the basic functionality
		[HttpDelete("deleteuser/{id}")]
		public async Task<IActionResult> Delete(long id,Supabase.Client client )
		{
			var reponse = await client.From<User>().Where( u => u.Id == id ).Get();
			if ( reponse.Model is null ) return BadRequest( "User does not exist" );
			await client.From<User>().Where( u => u.Id == id ).Delete();
			return NoContent();
		}
	}
}
