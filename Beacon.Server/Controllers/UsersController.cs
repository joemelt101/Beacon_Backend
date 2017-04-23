using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Beacon.Server.Models;
using System.Security.Cryptography;

namespace Beacon.Server.Controllers
{
    [Produces("application/json")]
    [Route("api/Users")]
    public class UsersController : Controller
    {
        private readonly BeaconContext _context = new BeaconContext();

        public UsersController() {}

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser([FromRoute] int id, [FromQuery] string token)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { WasSuccessful = false, Message = "Invalid model state" });
            }

            var user = await _context.User.SingleOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return Json(new { WasSuccessful = false, Message = "Invalid user id" });
            }

            var tokenMatch = await _context.Token.SingleOrDefaultAsync(t => t.Value.Equals(token));
            if (tokenMatch == null)
            {
                return Json(new { WasSuccessful = false, Message = "Invalid Token" });
            }

            return Json(new { WasSuccessful = true, User = user } );
        }

        [HttpGet("UsernameIsTaken")]
        public async Task<IActionResult> UsernameIsTaken([FromQuery] string username)
        {
            //TODO: Finish this!!!
            bool taken = await _context.User.AnyAsync(u => u.UserName.Equals(username));
            return Json(new { IsTaken = taken });
        }

        // POST: api/Users
        [HttpPost("PostUser")]
        public async Task<IActionResult> PostUser(
            [FromQuery] string username, 
            [FromQuery] string email, 
            [FromQuery] string password)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { WasSuccessful = false, Message = "Invalid model state." });
            }

            email = email.ToLower();
            username = username.ToLower();

            // Validate that the user doesn't currently exist...
            bool alreadyExists = false;

            // Initiate user...
            User user = new User()
            {
                UserName = username,
                Salt = Helper.GenerateSalt()
            };

            user.Id = 0; // Must be zero to ensure that the database sets it
            user.HashedPassword = Helper.HashPassword(password, user.Salt);
            _context.User.Add(user);

            await _context.SaveChangesAsync();

            return Json(new { WasSuccessful = true });
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromQuery] int id, [FromQuery] string oldPassword, [FromQuery] string newPassword)
        {
            if (! ModelState.IsValid)
            {
                return Json(new { WasSuccessful = false, Message = "Invalid model state" });
            }

            User user = await _context.User.FirstOrDefaultAsync(u => u.Id == id);
            if (user.HashedPassword.Equals(Helper.HashPassword(oldPassword, user.Salt)))
            {
                // Good to go!
                return Json(new { WasSuccessful = true });
            }

            return Json(new { WasSuccessful = false, Message = "Invalid old password" });
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] int id, [FromQuery] string token)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { WasSuccessful = false, Message = "Invalid model state!" });
            }

            var user = await _context.User.SingleOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return Json(new { WasSuccessful = false, Message = "Could not find user!" });
            }

            _context.User.Remove(user);
            await _context.SaveChangesAsync();

            return Json(new { WasSuccessful = true });
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
        }
    }
}