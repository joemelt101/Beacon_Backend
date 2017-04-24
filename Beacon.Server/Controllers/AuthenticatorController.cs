using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Beacon.Server.Models;
using Beacon.Server.Filters;
using Microsoft.EntityFrameworkCore;

namespace Beacon.Server.Controllers
{
    //TODO: Add error handling

    [Produces("application/json")]
    [Route("api/Authenticator")]
    public class AuthenticatorController : Controller
    {
        BeaconContext _context = new BeaconContext();

        //public AuthenticatorController(BeaconContext context)
        //{
        //    _context = context;
        //}

        [HttpGet("Token")]
        [ActionName("Token")]
        public async Task<IActionResult> Token([FromQuery] string username, [FromQuery] string password)
        {
            if (username == null || password == null)
            {
                int a = 10;
            }

            /////////////////////////////////
            // Validate Username and Password
            
            User user = await _context.User.FirstOrDefaultAsync(u => u.UserName.Equals(username));
            
            if (user == null)
            {
                return Json(new { LoginSuccessful = false, Message = "Username not found!" });
            }

            String hashedPassword = Helper.HashPassword(password, user.Salt);

            if (! hashedPassword.Equals(user.HashedPassword))
            {
                return Json(new { LoginSuccessful = false, Message = "Hashed passwords don't match!" });
            }

            ///////////////////
            // Generate a token

            // TODO: Beef up security
            char c = 'a';
            char[] randomCharArray = new char[30];
            Random r = new Random();

            for (int i = 0; i < 30; i++)
            {
                randomCharArray[i] = (char)(c + r.Next(0, 25));
            }

            Console.WriteLine(randomCharArray);

            string tokenValue = new string(randomCharArray);

            Token newToken = new Token()
            {
                CorrespondingLoginId = user.Id,
                Value = new string(randomCharArray)
            };

            //////////////////////////////
            // Write token to the database
            _context.Token.Add(newToken);
            await _context.SaveChangesAsync();

            return Json(new { LoginSuccessful = true, Token = tokenValue, UserId = user.Id });
        }

        [HttpGet("IsValidLogin")]
        public async Task<IActionResult> IsValidLogin([FromQuery] string username, [FromQuery] string password)
        {
            User user = await _context.User.FirstOrDefaultAsync(u => u.UserName.Equals(username));

            if (user == null)
            {
                return Json(new { WasSuccessful = false, Message = "Username does not exist!" });
            }

            // Hashed password
            string submittedHashedPassword = Helper.HashPassword(password, user.Salt);

            if (! submittedHashedPassword.Equals(user.HashedPassword))
            {
                // Passwords don't match!
                return Json(new { WasSuccessful = false, Message = "Invalid password." });
            }

            // Made it this far
            // Username exists
            // Passwords match
            // Therefore --->>> Good to go!
            return Json(new { WasSuccessful = true });
        }

        [HttpGet("Logout")]
        [BeaconAuthenticationFilter]
        public async Task<IActionResult> Logout([FromQuery] string token)
        {
            try
            {
                // Just delete the token to logout
                _context.Token.Remove(new Token() { Value = token });
                await _context.SaveChangesAsync();

                return Json(new { DeletedSuccessfully = true });
            }
            catch (Exception)
            {
                return Json(new { DeletedSuccessfully = false });
            }
        }

    }
}