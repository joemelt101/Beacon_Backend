using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Beacon.Server.Models;
using System.Diagnostics;
using Beacon.Server.Filters;

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
        public IActionResult Token([FromQuery] string username, [FromQuery] string password)
        {
            /////////////////////////////////
            // Validate Username and Password

            // TODO: Add hashing
            //var us = _context.User.Where(u => u.UserName.Equals("joemelt101")).First();
            User user = _context.User.FirstOrDefault(u => u.UserName.Equals(username) && u.HashedPassword.Equals(password));

            if (user == null)
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
                CorrespondingLoginId = user.Id,
                Value = new string(randomCharArray)
            };

            //////////////////////////////
            // Write token to the database
            _context.Token.Add(newToken);
            _context.SaveChanges();

            return Json(new { LoginSuccessful = true, Token = tokenValue });
        }

        [HttpGet("Logout")]
        [BeaconAuthenticationFilter]
        public IActionResult Logout([FromQuery] string token)
        {
            try
            {
                // Just delete the token to logout
                _context.Token.Remove(new Token() { Value = token });
                _context.SaveChanges();

                return Json(new { DeletedSuccessfully = true });
            }
            catch (Exception)
            {
                return Json(new { DeletedSuccessfully = false });
            }
        }

    }
}