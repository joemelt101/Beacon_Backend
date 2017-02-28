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
    [Route("api/Events")]
    public class EventsController : Controller
    {
        private readonly BeaconContext _context;

        public EventsController(BeaconContext context)
        {
            _context = context;
        }

        [HttpGet("da")]
        public async Task<IActionResult> DownloadAllEventsInArea([FromRoute] decimal latitude, [FromRoute] decimal longitude)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // TODO: Replace with more accurate / faster distance calculations
            var @events = await _context.Event.Where(e => (Math.Abs(e.Latitude.Value - latitude) < 1 && Math.Abs(e.Longitude.Value - longitude) < 1)).ToListAsync();
            
            return Json(@events);
        }

        [HttpGet("du")]
        public async Task<IActionResult> DownloadUpdates([FromRoute] decimal latitude, [FromRoute] decimal longitude, [FromRoute] DateTime lastUpdatedTime)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            // Download all events where the event was updated after the last client's updated time

            var returnStuff = await _context.Event.Where(e =>
            (Math.Abs(e.Latitude.Value - latitude) < 1 && Math.Abs(e.Longitude.Value - longitude) < 1)
            && e.TimeLastUpdated > lastUpdatedTime).ToListAsync();

            return Json(new { NumEvents = returnStuff.Count, Events = @returnStuff });
        }

        // GET: api/Events/5
        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetEvent([FromRoute] int id)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var @event = await _context.Event.SingleOrDefaultAsync(m => m.Id == id);

        //    if (@event == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(@event);
        //}

        // POST: api/Events
        [HttpPost]
        public async Task<IActionResult> PostEvent([FromBody] Event @event)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Event.Add(@event);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (EventExists(@event.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetEvent", new { id = @event.Id }, @event);
        }

        // DELETE: api/Events/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent([FromRoute] int id, [FromRoute] string token)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the token
            var matchingToken = await _context.Token.Where(t => t.Value == token).FirstAsync();

            if (matchingToken == null)
            {
                return BadRequest();
            }

            int loginId = matchingToken.CorrespondingLoginId;

            var @event = await _context.Event.SingleOrDefaultAsync(m => m.Id == id);
            if (@event == null)
            {
                return NotFound();
            }

            if (loginId == @event.CreatorId)
            {
                _context.Event.Remove(@event);
                await _context.SaveChangesAsync();
                return Ok(@event);
            }

            return BadRequest("You don't have permissions to do that!");
        }

        private bool EventExists(int id)
        {
            return _context.Event.Any(e => e.Id == id);
        }
    }
}