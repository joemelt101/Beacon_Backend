using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Beacon.Server.Models;

namespace Beacon.Server.Controllers
{
    //TODO: Add error handling

    [Produces("application/json")]
    [Route("api/Authenticator")]
    public class AuthenticatorController : Controller
    {
        BeaconContext _context;

        public AuthenticatorController(BeaconContext context)
        {
            _context = context;
        }

        [HttpGet("Token")]
        public IActionResult Token([FromRoute] string username, [FromRoute] string password)
        {
            /////////////////////////////////
            // Validate Username and Password

            // TODO: Add hashing
            bool userExists = _context.User.Where(u => u.UserName.Equals(username) && u.HashedPassword.Equals(password)).Any();

            if (!userExists)
            {
                return Json(new { LoginSuccessful = false });
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
                Value = new string(randomCharArray)
            };

            //////////////////////////////
            // Write token to the database
            _context.Token.Add(newToken);
            _context.SaveChanges();

            return Json(new { LoginSuccessful = true, Token = tokenValue });
        }

        [HttpGet("Logout")]
        public IActionResult Logout([FromRoute] string tokenValue)
        {
            try
            {
                // Just delete the token to logout
                _context.Token.Remove(new Models.Token() { Value = tokenValue });
                _context.SaveChanges();

                return Json(new { DeletedSuccessfully = true });
            }
            catch (Exception)
            {
                return Json(new { DeletedSuccessfully = true });
            }
        }

    }
}