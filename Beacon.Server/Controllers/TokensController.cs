using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Beacon.Server.Models;

namespace Beacon.Server.Controllers
{
    [Produces("application/json")]
    [Route("api")]
    public class TokensController : Controller
    {
        private readonly BeaconContext _context;

        public TokensController(BeaconContext context)
        {
            _context = context;
        }

        // DELETE: api/Login/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteToken([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = await _context.Token.SingleOrDefaultAsync(m => m.Value == id);
            if (token == null)
            {
                return NotFound();
            }

            _context.Token.Remove(token);
            await _context.SaveChangesAsync();

            return Ok(token);
        }

        private bool TokenExists(string id)
        {
            return _context.Token.Any(e => e.Value == id);
        }
    }
}